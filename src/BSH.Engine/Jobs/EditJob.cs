// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
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
using Brightbits.BSH.Engine.Storage;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for edit tasks
/// </summary>
public class EditJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<EditJob>();
    private readonly IBackupMutationRepository backupMutationRepository;

    public string Password
    {
        get; set;
    }

    public EditJob(
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
    /// Starts the decryption task for all files of all backups that are encrypted.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public async Task EditAsync(CancellationToken cancellationToken = default)
    {
        ApplyJobCulture();

        // report status
        _logger.Information("Begin edit backup.");

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_EDIT_PREPARE);
        ReportProgress(0, 0);

        cancellationToken.ThrowIfCancellationRequested();

        // check medium
        if (!await storage.CheckMedium())
        {
            _logger.Error("Backup storage is not ready. Backup will be cancelled.");

            ReportState(JobState.ERROR);
            throw new DeviceNotReadyException();
        }

        cancellationToken.ThrowIfCancellationRequested();

        // connect to database
        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            // begin with transaction
            dbClient.BeginTransaction();

            // open storage
            storage.Open();

            var numFiles = await versionQueryRepository.CountEditableFilesAsync(dbClient);
            ReportProgress(numFiles, 0);

            var editableFiles = new List<EditableFile>(numFiles);
            using (var reader = await versionQueryRepository.GetEditableFilesAsync(dbClient))
            {
                while (await reader.ReadAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    editableFiles.Add(new EditableFile(
                        reader.GetString("fileName"),
                        reader.GetString("filePath"),
                        reader.GetString("versionDate"),
                        reader.GetString("longfilename"),
                        reader.GetInt32("fileType"),
                        Convert.ToInt64(reader["fileversionid"])));
                }

                await reader.CloseAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();

            for (var i = 0; i < editableFiles.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var file = editableFiles[i];
                var remoteFilePath = file.GetRemoteFilePath();

                ReportFileProgress(remoteFilePath);
                ReportProgress(numFiles, i);

                try
                {
                    await EditFileFromDeviceAsync(dbClient, remoteFilePath, file.FileType, file.FileVersionId);
                }
                catch (Exception ex)
                {
                    var fileExceptionEntry = AddFileErrorToList(new FileTableRow()
                    {
                        FilePath = file.FilePath,
                        FileName = file.FileName
                    }, ex);

                    _logger.Error(ex, "File {FileName} could not be edited. {Exception}", remoteFilePath, fileExceptionEntry);
                }
            }

            dbClient.CommitTransaction();
        }

        // Only clear global encryption metadata after every encrypted file was updated successfully.
        if (FileErrorList.Count == 0)
        {
            configurationManager.Encrypt = 0;
            configurationManager.EncryptPassMD5 = "";
        }

        // close all database connections
        DbClientFactory.ClosePool();

        // store database
        UpdateDatabaseOnStorage();

        ReportExceptions(FileErrorList);

        ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);
        ReportStatus(Resources.STATUS_EDIT_FINISHED_SHORT, Resources.STATUS_EDIT_FINISHED_TEXT);

        _logger.Information("Edit job finished.");
    }

    /// <summary>
    /// Decrypts a single file from the backup device via the StorageManager.
    /// </summary>
    /// <param name="dbClient"></param>
    /// <param name="remoteFile"></param>
    /// <param name="fileType"></param>
    /// <param name="fileVersionId"></param>
    private async Task EditFileFromDeviceAsync(DbClient dbClient, string remoteFile, int fileType, long fileVersionId)
    {
        var decryptedFileType = fileType.ToFileTypeKind() switch
        {
            FileTypeKind.StoredEncrypted => FileTypeKind.StoredCopy,
            FileTypeKind.Encrypted => FileTypeKind.RegularCopy,
            _ => (FileTypeKind?)null
        };

        if (decryptedFileType == null)
        {
            return;
        }

        if (!storage.DecryptOnStorage(remoteFile, Password))
        {
            throw new IOException($"Storage failed to decrypt '{remoteFile}'.");
        }

        await backupMutationRepository.UpdateFileVersionTypeAsync(dbClient, fileVersionId, (int)decryptedFileType.Value);
    }

    private sealed record EditableFile(
        string FileName,
        string FilePath,
        string VersionDate,
        string LongFileName,
        int FileType,
        long FileVersionId)
    {
        public string GetRemoteFilePath()
        {
            return StoragePath.BuildRemoteFilePath(VersionDate, FilePath, FileName, LongFileName);
        }
    }
}
