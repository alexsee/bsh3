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
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Properties;
using Brightbits.BSH.Engine.Storage;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for single file deletion
/// </summary>
public class DeleteSingleJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<DeleteJob>();

    public DeleteSingleJob(IStorage storage, IDbClientFactory dbClientFactory, IQueryManager queryManager, IConfigurationManager configurationManager) : base(storage, dbClientFactory, queryManager, configurationManager)
    {
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
            var fileIds = new List<int>();
            var fileVersionIds = new List<int>();

            string selectFileSQL;
            (string, object)[] selectFileParameters;

            if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
            {
                selectFileSQL = "SELECT ft.fileID " +
                                "FROM fileTable AS ft " +
                                "WHERE fileName = @fileName AND filePath = @filePath";
                selectFileParameters =
                [
                    ("fileName", fileFilter),
                    ("filePath", pathFilter)
                ];
            }
            else
            {
                selectFileSQL = "SELECT ft.fileID " +
                                "FROM fileTable AS ft " +
                                "WHERE filePath LIKE @filePath";
                selectFileParameters = [
                    ("filePath", pathFilter)
                ];
            }

            // obtain files
            using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, selectFileSQL, selectFileParameters))
            {
                while (await reader.ReadAsync())
                {
                    fileIds.Add(reader.GetInt32(0));
                }
                await reader.CloseAsync();
            }

            // report progress
            _logger.Information("{numFiles} files determined for deletion.", fileIds.Count);
            ReportProgress(fileIds.Count, 0);

            foreach (var fileId in fileIds)
            {
                // obtain all file versions
                var deleteFileParams = new (string, object)[]
                {
                    ("fileId", fileId)
                };

                using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text,
                    "SELECT * FROM fileVersionTable AS fvt " +
                                                    "INNER JOIN fileTable AS ft ON " +
                                                    "  ft.fileID = fvt.fileID " +
                                                    "INNER JOIN versionTable AS vt ON " +
                                                    "  fvt.filePackage = vt.versionID " +
                                                    "WHERE fvt.fileID = @fileId",
                    deleteFileParams);
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
                            reader.GetInt32("fileType").ToString()
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

                    // add to deleted id list
                    fileVersionIds.Add(reader.GetInt32(reader.GetOrdinal("fileversionid")));
                }

                await reader.CloseAsync();
            }

            // delete metadata from database
            string subQuerySQL;
            var deleteParams = new List<(string, object)>();

            if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
            {
                subQuerySQL = "SELECT ft.fileID FROM fileTable AS ft WHERE fileName = @fileName AND filePath = @filePath";

                deleteParams.Add(("fileName", fileFilter));
                deleteParams.Add(("filePath", pathFilter));
            }
            else
            {
                subQuerySQL = "SELECT ft.fileID FROM fileTable AS ft WHERE filePath LIKE @filePath";

                deleteParams.Add(("filePath", pathFilter));
            }

            // delete metadata
            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM fileLink WHERE fileversionid IN (SELECT fileversionid FROM fileversiontable AS fvt WHERE fvt.fileID IN (" + subQuerySQL + "))", deleteParams.ToArray());
            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM fileversiontable WHERE fileID IN (" + subQuerySQL + ")", deleteParams.ToArray());
            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM fileTable AS ft WHERE " + subQuerySQL[(subQuerySQL.IndexOf("WHERE ") + 6)..], deleteParams.ToArray());

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
    private void DeleteFileFromDevice(string fileName, string filePath, string longFileName, string versionDate, string fileType)
    {
        // determine remote file name
        string remoteFile;
        if ((fileType == "1" || fileType == "2" || fileType == "6") && !string.IsNullOrEmpty(longFileName))
        {
            remoteFile = Path.Combine(versionDate, "_LONG_FILES", longFileName);
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
                storage.DeleteFileFromStorageEncryped(remoteFile);
            }
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }
    }
}