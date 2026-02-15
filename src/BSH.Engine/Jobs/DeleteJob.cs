// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
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
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for the restore task
/// </summary>
public class DeleteJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<DeleteJob>();
    private readonly IBackupMutationRepository backupMutationRepository;

    public string Version
    {
        get; set;
    }

    public DeleteJob(IStorageProvider storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager,
        IVersionQueryRepository versionQueryRepository,
        IBackupMutationRepository backupMutationRepository,
        bool silent = false) : base(storage, dbClientFactory, queryManager, configurationManager, versionQueryRepository, silent)
    {
        ArgumentNullException.ThrowIfNull(backupMutationRepository);
        this.backupMutationRepository = backupMutationRepository;
    }

    /// <summary>
    /// Starts the delete task and removes files from the backup device that
    /// are only needed to restore the specific backup.
    /// </summary>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public async Task DeleteAsync()
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

        // report status
        _logger.Information("Begin delete backup.");

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_DELETE_PREPARE);
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

            Win32Stuff.KeepSystemAwake();

            var version = int.Parse(Version);

            // obtain files to delete
            using var files = versionQueryRepository.GetFilesToDeleteForVersion(dbClient, version);

            // report progress
            _logger.Information("{numFiles} files determined for deletion.", files.Tables[0].Rows.Count);

            ReportProgress(files.Tables[0].Rows.Count, 0);
            ReportStatus(Resources.STATUS_DELETE_REMOVE_SHORT, Resources.STATUS_DELETE_REMOVE_TEXT);

            // delete files
            for (var i = 0; i < files.Tables[0].Rows.Count; i++)
            {
                var file = files.Tables[0].Rows[i];

                ReportProgress(files.Tables[0].Rows.Count, i);
                ReportFileProgress(file["fileName"]?.ToString());

                try
                {
                    // delete file from storage device
                    DeleteFileFromDevice(
                        file["fileName"].ToString(),
                        file["filePath"].ToString(),
                        file["longfilename"].ToString(),
                        file["versionDate"].ToString(),
                        file["fileType"].ToString());
                }
                catch (Exception ex)
                {
                    // file not deleted
                    var fileExceptionEntry = AddFileErrorToList(file["versionDate"].ToString(), new FileTableRow() { FileName = file["fileName"].ToString(), FilePath = file["filePath"].ToString() }, ex);

                    _logger.Error(ex.InnerException, "File {fileName} could not be deleted. {exception}", file["fileName"].ToString(), fileExceptionEntry);
                }

                // update database
                await backupMutationRepository.DeleteFileVersionAsync(dbClient, Convert.ToInt64(file["fileversionid"]));
            }

            // update backup metadata
            await backupMutationRepository.DeleteVersionMetadataAsync(dbClient, version);

            dbClient.CommitTransaction();
        }

        // report exceptions during job
        if (FileErrorList.Count > 0)
        {
            _logger.Error("{numFiles} could not be deleted to device.", FileErrorList.Count);
        }

        // refresh free diskspace
        try
        {
            configurationManager.FreeSpace = storage.GetFreeSpace().ToString();

            using var dbClient = dbClientFactory.CreateDbClient();
            configurationManager.BackupSize = (await versionQueryRepository.GetTotalBackupFileSizeAsync(dbClient)).ToString();
        }
        catch (Exception ex)
        {
            // not important
            _logger.Warning(ex, "Could not update free space variable due to exception.");
        }

        // clean storage folders
        if (storage.Kind == StorageProviderKind.LocalFileSystem)
        {
            using var dbClient = dbClientFactory.CreateDbClient();
            using var reader = await versionQueryRepository.GetOrphanedVersionDatesAsync(dbClient);

            while (await reader.ReadAsync())
            {
                try
                {
                    var remoteFolder = reader.GetString("versiondate");
                    storage.DeleteDirectory(remoteFolder);
                }
                catch
                {
                    // not necessary to handle this error
                }
            }

            await reader.CloseAsync();
        }

        // store database version
        if (int.TryParse(configurationManager.OldBackupPrevent, out var databaseVersion))
        {
            configurationManager.OldBackupPrevent = (databaseVersion + 1).ToString();
        }

        // refresh free diskspace
        await UpdateFreeDiskSpaceAsync();

        // close all database connections
        DbClientFactory.ClosePool();

        // store database
        UpdateDatabaseOnStorage();

        // close storage provider
        storage.Dispose();

        // standby mode
        Win32Stuff.AllowSystemSleep();

        // report exceptions during job
        if (FileErrorList.Count > 0)
        {
            ReportExceptions(FileErrorList);
        }

        _logger.Information("Deletion of backup {version} successfully.", Version);
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
    private void DeleteFileFromDevice(string fileName, string filePath, string longFileName, string versionDate, string fileType)
    {
        // determine remote file name
        string remoteFile;
        if ((fileType == "1" || fileType == "2" || fileType == "6") && !string.IsNullOrEmpty(longFileName))
        {
            remoteFile = Path.Combine(versionDate, "_LONGFILES_", longFileName);
        }
        else
        {
            remoteFile = Path.Combine(versionDate + filePath, fileName);
        }

        // delete file
        try
        {
            if (fileType == "1" || fileType == "3")
            {
                storage.DeleteFileFromStorage(remoteFile);
            }
            else if (fileType == "2" || fileType == "4")
            {
                storage.DeleteFileFromStorageCompressed(remoteFile);
            }
            else if (fileType == "5" || fileType == "6")
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
