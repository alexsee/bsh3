// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Properties;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for single file deletion
/// </summary>
public class DeleteSingleJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<DeleteSingleJob>();
    private readonly IBackupMutationRepository backupMutationRepository;

    public DeleteSingleJob(
        IStorageProvider storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager,
        IVersionQueryRepository versionQueryRepository,
        IBackupMutationRepository backupMutationRepository) : base(storage, dbClientFactory, queryManager, configurationManager, versionQueryRepository)
    {
        ArgumentNullException.ThrowIfNull(backupMutationRepository);
        this.backupMutationRepository = backupMutationRepository;
    }

    /// <summary>
    /// Starts the single file deletion from the backup device.
    /// </summary>
    /// <param name="fileFilter"></param>
    /// <param name="pathFilter"></param>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public Task DeleteSingleAsync(string fileFilter, string pathFilter)
    {
        return DeleteSingleAsync(fileFilter, pathFilter, null);
    }

    /// <summary>
    /// Starts the single file deletion from the backup device.
    /// When <paramref name="versionIds"/> is null or empty, the file is removed from all backups.
    /// Otherwise only the specified backup versions are unlinked (storage is deleted only when no
    /// remaining version still references the content).
    /// </summary>
    /// <param name="fileFilter"></param>
    /// <param name="pathFilter"></param>
    /// <param name="versionIds">Optional version IDs to scope the delete; null/empty means all versions.</param>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public async Task DeleteSingleAsync(string fileFilter, string pathFilter, IReadOnlyList<int> versionIds)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        var scopedVersions = versionIds is { Count: > 0 } ? versionIds : null;
        LogDeleteStart(scopedVersions);

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_DELETE_SINGLE_PREPARE);
        ReportProgress(0, 0);

        if (!await storage.CheckMedium())
        {
            _logger.Error("Backup storage is not ready. Backup will be cancelled.");

            ReportState(JobState.ERROR);
            throw new DeviceNotReadyException();
        }

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            dbClient.BeginTransaction();
            storage.Open();

            var fileIds = await versionQueryRepository.GetFileIdsForDeleteSingleAsync(dbClient, fileFilter, pathFilter);
            _logger.Information("{NumFiles} files determined for deletion.", fileIds.Count);
            ReportProgress(fileIds.Count, 0);

            await DeletePhysicalVersionsAsync(dbClient, fileIds, scopedVersions);
            await backupMutationRepository.DeleteSingleFileMetadataAsync(dbClient, fileFilter, pathFilter, scopedVersions);

            dbClient.CommitTransaction();
        }

        if (FileErrorList.Count > 0)
        {
            _logger.Error("{NumFiles} could not be deleted to device.", FileErrorList.Count);
        }

        await UpdateFreeDiskSpaceAsync();

        if (int.TryParse(configurationManager.OldBackupPrevent, out var databaseVersion))
        {
            configurationManager.OldBackupPrevent = (databaseVersion + 1).ToString();
        }

        DbClientFactory.ClosePool();
        UpdateDatabaseOnStorage();
        storage.Dispose();

        if (FileErrorList.Count > 0)
        {
            ReportExceptions(FileErrorList);
        }

        _logger.Information("Deletion of single files successfully.");
        ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);
    }

    private static void LogDeleteStart(IReadOnlyList<int> scopedVersions)
    {
        if (scopedVersions == null)
        {
            _logger.Information("Begin delete single file from all versions.");
            return;
        }

        _logger.Information("Begin delete single file from {NumVersions} versions.", scopedVersions.Count);
    }

    private async Task DeletePhysicalVersionsAsync(
        DbClient dbClient,
        IReadOnlyList<int> fileIds,
        IReadOnlyList<int> scopedVersions)
    {
        foreach (var fileId in fileIds)
        {
            using var reader = scopedVersions == null
                ? await versionQueryRepository.GetFileVersionsForDeleteSingleAsync(dbClient, fileId)
                : await versionQueryRepository.GetFileVersionsExclusiveToVersionsForDeleteSingleAsync(dbClient, fileId, scopedVersions);

            var i = 0;
            while (await reader.ReadAsync())
            {
                var fileName = reader.GetString("filePath") + reader.GetString("fileName");
                ReportFileProgress(fileName);
                ReportProgress(fileIds.Count, i);
                i++;

                try
                {
                    DeleteFileFromDevice(
                        reader.GetString("fileName"),
                        reader.GetString("filePath"),
                        reader.GetString("longfilename"),
                        reader.GetString("versionDate"),
                        reader.GetInt32("fileType").ToString());
                }
                catch (FileNotProcessedException ex)
                {
                    var fileExceptionEntry = AddFileErrorToList(new FileTableRow()
                    {
                        FileName = reader.GetString("fileName"),
                        FilePath = reader.GetString("filePath")
                    },
                        ex);

                    _logger.Error(ex.InnerException, "File {FileName} could not be deleted. {Exception}", reader.GetString("fileName"), fileExceptionEntry);
                }
            }

            await reader.CloseAsync();
        }
    }
}
