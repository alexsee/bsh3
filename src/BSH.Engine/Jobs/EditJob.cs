﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Properties;
using Brightbits.BSH.Engine.Storage;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for edit tasks
/// </summary>
public class EditJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<DeleteJob>();

    public string Password
    {
        get; set;
    }

    public EditJob(IStorage storage, IDbClientFactory dbClientFactory, IQueryManager queryManager, IConfigurationManager configurationManager) : base(storage, dbClientFactory, queryManager, configurationManager)
    {
    }

    /// <summary>
    /// Starts the decryption task for all files of all backups that are encrypted.
    /// </summary>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public async Task EditAsync()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        // report status
        _logger.Information("Begin edit backup.");

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_EDIT_PREPARE);
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

            var commandSQL = "FROM filetable a, fileversiontable b, filelink c, versiontable d " +
                "WHERE(c.fileversionid = b.fileversionid And a.fileid = b.fileid) " +
                "and d.versionid = b.filepackage";

            var numFiles = int.Parse((await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT COUNT(1) " + commandSQL, null)).ToString());
            ReportProgress(numFiles, 0);

            // determine files of backup to edit
            using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * " + commandSQL, null))
            {
                var i = 0;
                while (await reader.ReadAsync())
                {
                    // determine remote file
                    string remoteFilePath;

                    if (!string.IsNullOrEmpty(reader.GetString("longfilename")))
                    {
                        remoteFilePath = reader.GetString("versionDate") + "\\_LONGFILES_\\" + reader.GetString("longfilename");
                    }
                    else
                    {
                        remoteFilePath = reader.GetString("versionDate") + reader.GetString("filePath") + reader.GetString("fileName");
                    }

                    ReportFileProgress(remoteFilePath);
                    ReportProgress(numFiles, i);
                    i++;

                    // change file
                    try
                    {
                        await EditFileFromDeviceAsync(
                            dbClient,
                            remoteFilePath,
                            reader.GetInt32("fileType"),
                            reader.GetInt32("fileversionid"));
                    }
                    catch (Exception ex)
                    {
                        var fileExceptionEntry = AddFileErrorToList(new FileTableRow()
                        {
                            FilePath = reader.GetString("filePath"),
                            FileName = reader.GetString("fileName")
                        }, ex);

                        _logger.Error(ex.InnerException, "File {fileName} could not be edited. {exception}", remoteFilePath, fileExceptionEntry);
                    }
                }

                await reader.CloseAsync();
            }

            dbClient.CommitTransaction();
        }

        // set new metadata
        configurationManager.Encrypt = 0;
        configurationManager.EncryptPassMD5 = "";

        // close all database connections
        DbClientFactory.ClosePool();

        // store database
        UpdateDatabaseOnStorage();

        // close storage provider
        storage.Dispose();

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
    private async Task EditFileFromDeviceAsync(DbClient dbClient, string remoteFile, int fileType, int fileVersionId)
    {
        if (fileType == 5)
        {
            // decrypt on device
            storage.DecryptOnStorage(remoteFile, Password);

            // modify entry
            await dbClient.ExecuteNonQueryAsync($"UPDATE fileversiontable SET fileType = 3 WHERE fileversionid = {fileVersionId}");
        }
        else if (fileType == 6)
        {
            // decrypt on device
            storage.DecryptOnStorage(remoteFile, Password);

            // modify entry
            await dbClient.ExecuteNonQueryAsync($"UPDATE fileversiontable SET fileType = 1 WHERE fileversionid = {fileVersionId}");
        }
    }
}
