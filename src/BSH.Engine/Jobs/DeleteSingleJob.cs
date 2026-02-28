// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.Globalization;
using System.IO;
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
using Brightbits.BSH.Engine.Utils;
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
    public async Task DeleteSingleAsync(string fileFilter, string pathFilter)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        // report status
        _logger.Information("Begin delete single file.");

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_DELETE_SINGLE_PREPARE);
        ReportProgress(0, 0);

        // check medium
        if (!await storage.CheckMedium())
        {
            _logger.Error("Backup storage is not ready. Backup will be cancelled.");

            ReportState(JobState.ERROR);
            throw new DeviceNotReadyException();
        }

        // connect to database
        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            // begin with transaction
            dbClient.BeginTransaction();

            // open storage
            storage.Open();

            // get file list
            var fileIds = await versionQueryRepository.GetFileIdsForDeleteSingleAsync(dbClient, fileFilter, pathFilter);

            // report progress
            _logger.Information("{numFiles} files determined for deletion.", fileIds.Count);
            ReportProgress(fileIds.Count, 0);

            foreach (var fileId in fileIds)
            {
                // obtain all file versions
                using var reader = await versionQueryRepository.GetFileVersionsForDeleteSingleAsync(dbClient, fileId);
                var i = 0;
                while (await reader.ReadAsync())
                {
                    // get file name
                    var fileName = reader.GetString("filePath") + reader.GetString("fileName");

                    ReportFileProgress(fileName);
                    ReportProgress(fileIds.Count, i);
                    i++;

                    // delete file
                    try
                    {
                        DeleteFileFromDevice(
                            reader.GetString("fileName"),
                            reader.GetString("filePath"),
                            reader.GetString("longfilename"),
                            reader.GetString("versionDate"),
                            reader.GetInt32("fileType")
                        );
                    }
                    catch (FileNotProcessedException ex)
                    {
                        // file not deleted
                        var fileExceptionEntry = AddFileErrorToList(new FileTableRow()
                        {
                            FileName = reader.GetString("fileName"),
                            FilePath = reader.GetString("filePath")
                        },
                            ex);

                        _logger.Error(ex.InnerException, "File {fileName} could not be deleted. {exception}", reader.GetString("fileName"), fileExceptionEntry);
                    }

                }

                await reader.CloseAsync();
            }

            // delete metadata from database
            await backupMutationRepository.DeleteSingleFileMetadataAsync(dbClient, fileFilter, pathFilter);

            dbClient.CommitTransaction();
        }

        // report exceptions during job
        if (FileErrorList.Count > 0)
        {
            _logger.Error("{numFiles} could not be deleted to device.", FileErrorList.Count);
        }

        // refresh free diskspace
        await UpdateFreeDiskSpaceAsync();

        // store database version
        if (int.TryParse(configurationManager.OldBackupPrevent, out var databaseVersion))
        {
            configurationManager.OldBackupPrevent = (databaseVersion + 1).ToString();
        }

        // close all database connections
        DbClientFactory.ClosePool();

        // store database
        UpdateDatabaseOnStorage();

        // close storage provider
        storage.Dispose();

        // report exceptions during job
        if (FileErrorList.Count > 0)
        {
            ReportExceptions(FileErrorList);
        }

        _logger.Information("Deletion of single files successfully.");
        ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);
    }

    /// <summary>
    /// Deletes a single file from the backup device via the StorageManager.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="filePath"></param>
    /// <param name="longFileName"></param>
    /// <param name="versionDate"></param>
    /// <param name="fileType"></param>
    /// <exception cref="FileNotProcessedException"></exception>
    private void DeleteFileFromDevice(string fileName, string filePath, string longFileName, string versionDate, int fileType)
    {
        // determine remote file name
        var remoteFile = !string.IsNullOrEmpty(longFileName)
            ? Path.Combine(versionDate, "_LONGFILES_", longFileName)
            : Path.Combine(versionDate + filePath, fileName);

        // delete file
        try
        {
            if (BackupFileType.IsRegular(fileType))
            {
                storage.DeleteFileFromStorage(remoteFile);
            }
            else if (BackupFileType.IsCompressed(fileType))
            {
                storage.DeleteFileFromStorageCompressed(remoteFile);
            }
            else if (BackupFileType.IsEncrypted(fileType))
            {
                storage.DeleteFileFromStorageEncrypted(remoteFile);
            }
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }
    }
}
