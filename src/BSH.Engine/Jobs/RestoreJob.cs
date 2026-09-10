// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
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
public class RestoreJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<RestoreJob>();

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

    private bool overwritePromptCanceled;

    public RestoreJob(IStorageProvider storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager,
        IVersionQueryRepository versionQueryRepository) : base(storage, dbClientFactory, queryManager, configurationManager, versionQueryRepository)
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

        storage.Open();
        try
        {
            // connect to database
            using var dbClient = dbClientFactory.CreateDbClient();

            // obtain files that need to be restored
            var countFiles = 0;
            System.Data.Common.DbDataReader reader;

            // single file or path?
            var singleFileName = string.IsNullOrEmpty(File) ? string.Empty : (Path.GetFileName(File) ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(singleFileName))
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

                countFiles = 1;
                reader = await versionQueryRepository.GetRestoreSingleFileAsync(dbClient, Version, fileName, filePath);
            }
            else
            {
                // correct path
                if (!File.EndsWith('\\'))
                {
                    File += "\\";
                }

                countFiles = await versionQueryRepository.CountRestoreFilesByPathAsync(dbClient, Version, File);
                reader = await versionQueryRepository.GetRestoreFilesByPathAsync(dbClient, Version, File);
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
            _logger.Information("{CountFiles} files will be restored.", countFiles);
            ReportStatus(Resources.STATUS_RESTORE_COPY_SHORT, Resources.STATUS_RESTORE_COPY_TEXT);
            ReportProgress(countFiles, 0);

            // restore files
            using (reader)
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
                        _logger.Information("File {FileName} restored successfully.", filePath + fileName);
                    }
                    catch (Exception ex)
                    {
                        var fileExceptionEntry = AddFileErrorToList(new FileTableRow() { FilePath = filePath, FileName = fileName }, ex);

                        // file could not be not restored
                        _logger.Error(ex.InnerException, "File {FileName} could not be restored due to exception. {Exception}", filePath + fileName, fileExceptionEntry);
                    }

                    // cancellation token requested?
                    if (token.IsCancellationRequested || overwritePromptCanceled)
                    {
                        // report progress
                        _logger.Information("User requested cancellation of restore job.");

                        ReportStatus(Resources.STATUS_CANCELLED_SHORT, Resources.STATUS_CANCELLED_TEXT);
                        ReportExceptions(FileErrorList);
                        ReportState(JobState.CANCELED);
                        return;
                    }
                }

                await reader.CloseAsync();
            }

            // restore folders
            using (var folderReader = await versionQueryRepository.GetRestoreFoldersByPathAsync(dbClient, Version, File))
            {
                while (await folderReader.ReadAsync(token))
                {
                    var fileDest = GetFileDestination(destFolders, folderReader.GetString("folder"));

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

                await folderReader.CloseAsync();
            }

            // report file errors
            ReportExceptions(FileErrorList);
            ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);

            _logger.Information("Restore of files successfully finished.");
        }
        finally
        {
            storage.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the destination path for a file.
    /// </summary>
    /// <param name="destFolders"></param>
    /// <param name="fileDest"></param>
    /// <returns></returns>
    private static string GetFileDestination(List<string> destFolders, string fileDest)
    {
        ArgumentNullException.ThrowIfNull(destFolders);
        ArgumentNullException.ThrowIfNull(fileDest);

        if (destFolders.Count > 1)
        {
            var match = destFolders.Find(folder => fileDest.StartsWith("\\" + Path.GetFileName(folder) + "\\", StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                // stored path does not match any known source root; fall back to the first destination
                fileDest = destFolders[0] + "\\" + fileDest.TrimStart('\\');
            }
            else
            {
                var needle = "\\" + Path.GetFileName(match) + "\\";
                var idx = fileDest.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
                fileDest = idx >= 0
                    ? match + "\\" + fileDest[(idx + needle.Length)..]
                    : match + "\\" + fileDest.TrimStart('\\');
            }
        }
        else if (destFolders.Count == 1)
        {
            // strip the stored drive/root prefix ("\\C\\...") and re-root at the destination
            var separator = fileDest.IndexOf('\\', 2);
            var remainder = separator >= 0 ? fileDest[(separator + 1)..] : fileDest.TrimStart('\\');
            fileDest = destFolders[0] + "\\" + remainder;
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
    public async Task CopyFileFromDevice(IStorageProvider storage, IDataReader reader, string destination, bool warning = true)
    {
        ArgumentNullException.ThrowIfNull(storage);

        var localFilePath = Path.Combine(destination, reader.GetString("fileName"));
        var fileType = reader.GetInt32("fileType");

        if (await ShouldSkipOverwriteAsync(localFilePath, destination, reader, warning))
        {
            return;
        }

        DeleteExistingFile(localFilePath);

        try
        {
            var remoteFilePath = BuildRemoteFilePath(reader);
            CopyFromStorage(storage, fileType, localFilePath, remoteFilePath);
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

    private async Task<bool> ShouldSkipOverwriteAsync(string localFilePath, string destination, IDataReader reader, bool warning)
    {
        if (!warning || !System.IO.File.Exists(localFilePath))
        {
            return false;
        }

        if (FileOverwrite == FileOverwrite.DontCopy)
        {
            return true;
        }

        if (FileOverwrite != FileOverwrite.Ask)
        {
            return false;
        }

        var overwriteRequest = await GetOverwriteDecisionAsync(localFilePath, destination, reader);
        if (overwriteRequest == RequestOverwriteResult.NoOverwriteAll || overwriteRequest == RequestOverwriteResult.OverwriteAll)
        {
            overwriteRequestPersistent = overwriteRequest;
        }

        if (overwriteRequest == RequestOverwriteResult.None)
        {
            overwritePromptCanceled = true;
            return true;
        }

        return overwriteRequest == RequestOverwriteResult.NoOverwrite || overwriteRequest == RequestOverwriteResult.NoOverwriteAll;
    }

    private async Task<RequestOverwriteResult> GetOverwriteDecisionAsync(string localFilePath, string destination, IDataReader reader)
    {
        if (overwriteRequestPersistent != RequestOverwriteResult.None)
        {
            return overwriteRequestPersistent;
        }

        var localFileInfo = new FileInfo(localFilePath);
        var localFile = new FileTableRow
        {
            FileName = reader.GetString("fileName"),
            FilePath = destination,
            FileSize = localFileInfo.Length,
            FileDateModified = localFileInfo.LastWriteTime
        };

        var remoteFile = new FileTableRow
        {
            FileName = reader.GetString("fileName"),
            FileSize = (long)reader.GetDouble(reader.GetOrdinal("fileSize")),
            FileDateModified = reader.GetDateTime("fileDateModified")
        };

        return await RequestOverwrite(localFile, remoteFile);
    }

    private static void DeleteExistingFile(string localFilePath)
    {
        if (!System.IO.File.Exists(localFilePath))
        {
            return;
        }

        try
        {
            System.IO.File.Delete(localFilePath);
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }
    }

    private static string BuildRemoteFilePath(IDataReader reader)
    {
        if (!string.IsNullOrEmpty(reader.GetString("longfilename")))
        {
            return reader.GetString("versionDate") + "\\_LONGFILES_\\" + reader.GetString("longfilename");
        }

        return reader.GetString("versionDate") + reader.GetString("filePath") + reader.GetString("fileName");
    }

    private void CopyFromStorage(IStorageProvider storage, int fileType, string localFilePath, string remoteFilePath)
    {
        if (fileType == 1 || fileType == 3)
        {
            storage.CopyFileFromStorage(localFilePath, remoteFilePath);
            return;
        }

        if (fileType == 2 || fileType == 4)
        {
            storage.CopyFileFromStorageCompressed(localFilePath, remoteFilePath);
            return;
        }

        if (fileType == 5 || fileType == 6)
        {
            storage.CopyFileFromStorageEncrypted(localFilePath, remoteFilePath, Password);
        }
    }
}
