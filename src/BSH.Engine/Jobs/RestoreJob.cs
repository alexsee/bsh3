﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
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
/// Class for the restore task
/// </summary>
public class RestoreJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<DeleteJob>();

    public int Version
    {
        get; set;
    }

    public string File
    {
        get; set;
    }

    public string Destination
    {
        get; set;
    }

    public FileOverwrite FileOverwrite
    {
        get; set;
    }

    public string Password
    {
        get; set;
    }


    private RequestOverwriteResult overwriteRequestPersistent = RequestOverwriteResult.None;

    public RestoreJob(IStorage storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager) : base(storage, dbClientFactory, queryManager, configurationManager)
    {
    }

    /// <summary>
    /// Starts the restore task to copy files from the backup device to
    /// the local file system. The method takes care to gather all information
    /// from the database to correctly restore the corresponding file
    /// versions.
    /// </summary>
    /// <param name="token"></param>
    /// <exception cref="DeviceNotReadyException"></exception>
    public async Task RestoreAsync(CancellationToken token)
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

        // report status
        _logger.Information("Begin restore", new { Version, File, Destination });

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_RESTORE_PREPARE);
        ReportProgress(0, 0);

        // check medium
        if (!await storage.CheckMedium())
        {
            _logger.Error("Storage device is not ready. Restore will be cancelled.");

            ReportState(JobState.ERROR);
            throw new DeviceNotReadyException();
        }

        // connect to database
        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            storage.Open();

            // obtain files that need to be restored
            var countFiles = 0;
            var getFileSQL = "";

            // single file or path?
            if (!string.IsNullOrEmpty(Path.GetFileName(File).Trim()))
            {
                var fileName = Path.GetFileName(File);
                var filePath = Path.GetDirectoryName(File);

                if (!filePath.EndsWith('\\'))
                {
                    filePath += "\\";
                }

                if (!filePath.StartsWith('\\'))
                {
                    filePath = "\\" + filePath;
                }

                // query single file
                getFileSQL = $"SELECT " +
                    $"fileversiontable.*, filetable.*, versiontable.versionDate " +
                    $"FROM filetable, fileversiontable, versiontable, filelink " +
                    $"WHERE filelink.fileversionID = fileversiontable.fileversionID " +
                    $"AND filelink.versionID = {Version} " +
                    $"AND fileversiontable.filePackage = versiontable.versionID " +
                    $"AND filetable.fileName LIKE \"{fileName}\" " +
                    $"AND filetable.filePath LIKE \"{filePath}\" " +
                    $"AND fileversiontable.fileID = filetable.fileID LIMIT 1";

                countFiles = 1;
            }
            else
            {
                // correct path
                if (!File.EndsWith('\\'))
                {
                    File += "\\";
                }

                var c = await dbClient.ExecuteScalarAsync(CommandType.Text,
                    $"SELECT COUNT(*) FROM filetable, fileversiontable, filelink " +
                    $"WHERE filelink.fileversionID = fileversiontable.fileversionID " +
                    $"AND fileversiontable.fileID = filetable.fileID " +
                    $"AND filelink.versionID = {Version} " +
                    $"AND filetable.filePath LIKE \"{File}%\"", null);

                countFiles = int.Parse(c.ToString());

                getFileSQL = $"SELECT filetable.*, versiontable.versionDate, fileversiontable.* " +
                    $"FROM filetable, versiontable, fileversiontable, filelink " +
                    $"WHERE filelink.fileversionID = fileversiontable.fileversionID " +
                    $"AND fileversiontable.fileID = filetable.fileID " +
                    $"AND filePackage = versiontable.versionID " +
                    $"AND filelink.versionID = {Version} " +
                    $"AND filePath LIKE \"{File}%\"";
            }

            // determine target folder
            var destFolders = new List<string>();

            if (string.IsNullOrEmpty(Destination))
            {
                var sources = (await queryManager.GetVersionByIdAsync(Version.ToString())).Sources;

                if (string.IsNullOrEmpty(sources))
                {
                    sources = configurationManager.SourceFolder;
                }

                foreach (var source in sources.Split('|'))
                {
                    destFolders.Add(source);
                }
            }
            else
            {
                destFolders.Add(Destination);
            }

            // refresh status
            _logger.Information("{countFiles} files will be restored.", countFiles);
            ReportStatus(Resources.STATUS_RESTORE_COPY_SHORT, Resources.STATUS_RESTORE_COPY_TEXT);
            ReportProgress(countFiles, 0);

            // restore files
            using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, getFileSQL, null))
            {
                var i = 0;
                while (await reader.ReadAsync(token))
                {
                    var filePath = reader.GetString("filePath");
                    var fileName = reader.GetString("fileName");

                    // report status
                    ReportProgress(countFiles, i);
                    ReportFileProgress(filePath + fileName);
                    i++;

                    // determine target folder
                    var fileDest = GetFileDestination(destFolders, filePath);

                    // create path
                    try
                    {
                        Directory.CreateDirectory(fileDest);
                    }
                    catch
                    {
                        // ignore
                    }

                    // copy path
                    try
                    {
                        await CopyFileFromDevice(storage, reader, fileDest);
                        _logger.Information("File {fileName} restored successfully.", filePath + fileName);
                    }
                    catch (Exception ex)
                    {
                        var fileExceptionEntry = AddFileErrorToList(new FileTableRow() { FilePath = filePath, FileName = fileName }, ex);

                        // file could not be not restored
                        _logger.Error(ex.InnerException, "File {fileName} could not be restored due to exception. {exception}", filePath + fileName, fileExceptionEntry);
                    }

                    // cancellation token requested?
                    if (token.IsCancellationRequested)
                    {
                        // report progress
                        _logger.Information("User requested cancellation of restore job.");

                        ReportStatus(Resources.STATUS_CANCELLED_SHORT, Resources.STATUS_CANCELLED_TEXT);
                        ReportExceptions(FileErrorList);
                        ReportState(JobState.FINISHED);
                        return;
                    }
                }

                await reader.CloseAsync();
            }

            // restore folders
            using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, $"SELECT folder FROM foldertable, folderlink WHERE foldertable.id = folderlink.folderid AND folderlink.versionid = {Version} AND foldertable.folder LIKE \"{File}%\"", null))
            {
                while (await reader.ReadAsync(token))
                {
                    var fileDest = GetFileDestination(destFolders, reader.GetString("folder"));

                    // create path
                    try
                    {
                        Directory.CreateDirectory(fileDest);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                await reader.CloseAsync();
            }
        }

        // report file errors
        ReportExceptions(FileErrorList);
        ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);

        _logger.Information("Restore of files successfully finished.");
    }

    /// <summary>
    /// Retrieves the destination path for a file.
    /// </summary>
    /// <param name="destFolders"></param>
    /// <param name="fileDest"></param>
    /// <returns></returns>
    private static string GetFileDestination(List<string> destFolders, string fileDest)
    {
        if (destFolders.Count > 1)
        {
            var folder = destFolders.Find(folder => fileDest.StartsWith("\\" + Path.GetFileName(folder) + "\\", StringComparison.OrdinalIgnoreCase));
            var idx = fileDest.ToLower().IndexOf(("\\" + Path.GetFileName(folder) + "\\").ToLower(), StringComparison.OrdinalIgnoreCase);
            fileDest = folder + "\\" + fileDest[(idx + Path.GetFileName(folder).Length + 2)..];
        }
        else
        {
            // path found
            fileDest = fileDest[(fileDest.IndexOf('\\', 2) + 1)..];
            fileDest = destFolders[0] + "\\" + fileDest;
        }

        // correct path
        if (!fileDest.EndsWith('\\'))
        {
            fileDest += "\\";
        }

        return fileDest;
    }

    /// <summary>
    /// Copies a single file from the backup device to the local file system
    /// using the StorageManager.
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="reader"></param>
    /// <param name="destination"></param>
    /// <param name="warning"></param>
    /// <exception cref="FileNotProcessedException"></exception>
    public async Task CopyFileFromDevice(IStorage storage, IDataReader reader, string destination, bool warning = true)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var localFilePath = Path.Combine(destination, reader.GetString("fileName"));
        var fileType = reader.GetInt32("fileType");

        // check overwrite
        if (warning && System.IO.File.Exists(localFilePath))
        {
            if (FileOverwrite == FileOverwrite.Ask)
            {
                var localFileInfo = new FileInfo(localFilePath);

                var localFile = new FileTableRow()
                {
                    FileName = reader.GetString("fileName"),
                    FilePath = destination,
                    FileSize = localFileInfo.Length,
                    FileDateModified = localFileInfo.LastWriteTime
                };

                var remoteFile = new FileTableRow()
                {
                    FileName = reader.GetString("fileName"),
                    FileSize = (long)reader.GetDouble(reader.GetOrdinal("fileSize")),
                    FileDateModified = reader.GetDateTime("fileDateModified")
                };

                // request user input for overwrite
                var overwriteRequest = this.overwriteRequestPersistent != RequestOverwriteResult.None ? this.overwriteRequestPersistent : await RequestOverwrite(localFile, remoteFile);

                if (overwriteRequest == RequestOverwriteResult.NoOverwriteAll || overwriteRequest == RequestOverwriteResult.OverwriteAll)
                {
                    this.overwriteRequestPersistent = overwriteRequest;
                }

                if (overwriteRequest == RequestOverwriteResult.NoOverwrite || overwriteRequest == RequestOverwriteResult.NoOverwriteAll)
                {
                    return;
                }
            }
            else if (FileOverwrite == FileOverwrite.DontCopy)
            {
                return;
            }
        }

        // remove old file
        if (System.IO.File.Exists(localFilePath))
        {
            try
            {
                System.IO.File.Delete(localFilePath);
            }
            catch (Exception ex)
            {
                throw new FileNotProcessedException(ex);
            }
        }

        try
        {
            // copy file
            string remoteFilePath;

            if (!string.IsNullOrEmpty(reader.GetString("longfilename")))
            {
                remoteFilePath = reader.GetString("versionDate") + "\\_LONGFILES_\\" + reader.GetString("longfilename");
            }
            else
            {
                remoteFilePath = reader.GetString("versionDate") + reader.GetString("filePath") + reader.GetString("fileName");
            }

            if (fileType == 1 || fileType == 3)
            {
                storage.CopyFileFromStorage(localFilePath, remoteFilePath);
            }
            else if (fileType == 2 || fileType == 4)
            {
                storage.CopyFileFromStorageCompressed(localFilePath, remoteFilePath);
            }
            else if (fileType == 5 || fileType == 6)
            {
                storage.CopyFileFromStorageEncrypted(localFilePath, remoteFilePath, Password);
            }
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }

        // set creation and last write tme
        try
        {
            System.IO.File.SetCreationTime(localFilePath, reader.GetDateTime("fileDateCreated"));
            System.IO.File.SetLastWriteTime(localFilePath, reader.GetDateTime("fileDateModified"));
        }
        catch
        {
            // ignore this exception
        }
    }
}
