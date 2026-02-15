// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Properties;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Services.FileCollector;
using Brightbits.BSH.Engine.Utils;
using Serilog;

namespace Brightbits.BSH.Engine.Jobs;

/// <summary>
/// Class for the backup task
/// </summary>
public class BackupJob : Job
{
    private static readonly ILogger _logger = Log.ForContext<BackupJob>();

    private readonly HashSet<string> junctionFolders = new();

    private readonly IFileCollectorServiceFactory fileCollectorServiceFactory;
    private readonly IVssClient vssClient;
    private readonly IBackupMutationRepository backupMutationRepository;

    public string Title
    {
        get; set;
    }

    public string Description
    {
        get; set;
    }

    public bool FullBackup
    {
        get; set;
    }

    public string Sources
    {
        get; set;
    }

    public string SourceFolder
    {
        get; set;
    }

    public string Password
    {
        get; set;
    }

    public BackupJob(IStorageProvider storage,
        IDbClientFactory dbClientFactory,
        IQueryManager queryManager,
        IConfigurationManager configurationManager,
        IFileCollectorServiceFactory fileCollectorServiceFactory,
        IVssClient vssClient,
        IVersionQueryRepository versionQueryRepository,
        IBackupMutationRepository backupMutationRepository,
        bool silent = false) : base(storage, dbClientFactory, queryManager, configurationManager, versionQueryRepository, silent)
    {
        ArgumentNullException.ThrowIfNull(fileCollectorServiceFactory);
        ArgumentNullException.ThrowIfNull(vssClient);
        ArgumentNullException.ThrowIfNull(backupMutationRepository);

        this.fileCollectorServiceFactory = fileCollectorServiceFactory;
        this.backupMutationRepository = backupMutationRepository;
        this.vssClient = vssClient;
    }

    /// <summary>
    /// Starts the backup task and copies files from the sources to the backup
    /// device. The method also executes database queries to update and retrieve
    /// details for incremental backups.
    /// </summary>
    /// <param name="token"></param>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="NoSourceFolderSelectedException"></exception>
    /// <exception cref="DatabaseFileNotUpdatedException"></exception>
    public async Task BackupAsync(CancellationToken token)
    {
        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

        // report status
        _logger.Information("Begin backup", new { Title, FullBackup, Sources, SourceFolder });

        ReportState(JobState.RUNNING);
        ReportStatus(Resources.STATUS_PREPARE, Resources.STATUS_BACKUP_PREPARE);
        ReportProgress(0, 0);

        // check medium
        if (!await storage.CheckMedium())
        {
            _logger.Error("Backup storage is not ready. Backup will be cancelled.");

            ReportState(JobState.ERROR);
            throw new DeviceNotReadyException();
        }

        // check source
        if (string.IsNullOrEmpty(SourceFolder))
        {
            _logger.Error("Source folder is empty so no files can be backuped.");

            ReportState(JobState.ERROR);
            throw new NoSourceFolderSelectedException();
        }

        // connect to database
        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            ///
            /// PHASE 1 : Initialization
            ///

            // get last version
            var lastVersionDate = await versionQueryRepository.GetLastVersionDateAsync(dbClient);

            // full backup?
            var fullBackup = string.IsNullOrEmpty(lastVersionDate?.ToString()) || FullBackup;

            // open storage
            storage.Open();

            // get backup version date
            var newVersionDate = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");

            // save current backup version, so we know in case of an error that something wrong happend
            configurationManager.LastVersionDate = newVersionDate;

            // begin with transaction
            dbClient.BeginTransaction();

            // variable for new files
            var newFiles = false;

            // store all folders or just the selection
            var selectedFolders = string.IsNullOrEmpty(Sources) ? SourceFolder.Split('|') : Sources.Split('|');
            var folderList = GetSourceFolders(selectedFolders);

            var folderListString = string.Join("|", folderList);

            var newVersionId = await backupMutationRepository.CreateVersionAsync(dbClient, newVersionDate, Title, Description, fullBackup, folderListString);

            ///
            /// PHASE 2 : Obtain new files and backup them
            /// 

            // obtain all files
            var files = new List<FileTableRow>();
            var emptyFolder = new List<FolderTableRow>();

            foreach (var folderEntry in folderList)
            {
                var fileCollector = fileCollectorServiceFactory.Create();

                // file exclusions
                fileCollector.FileExclusionHandlers.Add(new DatabaseFileExclusion());
                fileCollector.FileExclusionHandlers.Add(new PathFileExclusion(configurationManager));
                fileCollector.FileExclusionHandlers.Add(new TypeFileExclusion(configurationManager));
                fileCollector.FileExclusionHandlers.Add(new SizeFileExclusion(configurationManager));
                fileCollector.FileExclusionHandlers.Add(new MaskFileExclusion(configurationManager));
                fileCollector.FileExclusionHandlers.Add(new NameFileExclusion(configurationManager));

                // folder exclusions
                fileCollector.FolderExclusionHandlers.Add(new PathFolderExclusion(configurationManager));
                fileCollector.FolderExclusionHandlers.Add(new MaskFolderExclusion(configurationManager));
                fileCollector.FolderExclusionHandlers.Add(new ReparsePointFolderExclusion());
                fileCollector.FolderExclusionHandlers.Add(new SystemFolderExclusion());
                fileCollector.FolderExclusionHandlers.Add(new TemporaryFolderExclusion());

                var filesList = fileCollector.GetLocalFileList(folderEntry, true);
                emptyFolder.AddRange(fileCollector.EmptyFolders);
                files.AddRange(filesList);
            }

            // report progress
            _logger.Information("{numFiles} files and {numFolders} folders are collected for backup.", files.Count, emptyFolder.Count);

            ReportStatus(Resources.STATUS_BACKUP_COPY_SHORT, Resources.STATUS_BACKUP_COPY_TEXT);
            ReportProgress(files.Count, 0);

            // keep system running
            Win32Stuff.KeepSystemAwake();

            // process empty folders
            await ProcessEmptyFolders(dbClient, newVersionId, emptyFolder);

            // process all files
            var cancel = false;
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                ReportProgress(files.Count, i);

                var isModifiedFile = true;

                try
                {
                    // refresh status
                    ReportFileProgress(file.FileNamePath());

                    // search for database entry
                    var filePath = "\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\";
                    var fileId = await backupMutationRepository.GetFileIdAsync(dbClient, file.FileName, filePath);
                    file.FileId = fileId?.ToString();

                    if (!fileId.HasValue)
                    {
                        // file does not have an entry
                        fileId = await backupMutationRepository.CreateFileAsync(dbClient, file.FileName, filePath);
                        file.FileId = fileId.ToString();
                    }
                    else
                    {
                        // file was found, so we already have a version of the file
                        if (!FullBackup)
                        {
                            var fileVersionId = await backupMutationRepository.GetMatchingFileVersionIdAsync(dbClient, fileId.Value, file.FileSize, file.FileDateModified);
                            file.FilePackage = fileVersionId?.ToString();

                            if (fileVersionId.HasValue)
                            {
                                // file is the same, so only create a link
                                await backupMutationRepository.AddFileLinkAsync(dbClient, fileVersionId.Value, newVersionId);
                                isModifiedFile = false;
                            }
                        }
                    }

                    // check if we need to backup the file
                    if (isModifiedFile)
                    {
                        // store folder reparse point
                        await SaveJunctionAsync(file, dbClient);

                        // backup file
                        _logger.Debug("Copy file {fileName} to backup device.", file.FileNamePath());

                        if (await CopyFileToDeviceAsync(storage, file, newVersionId, newVersionDate, dbClient))
                        {
                            _logger.Debug("{fileName} backed up successfully.", file.FileNamePath());

                            newFiles = true;
                        }
                    }
                }
                catch (FileNotProcessedException ex)
                {
                    var fileExceptionEntry = AddFileErrorToList(newVersionDate, newVersionId, file, ex);
                    _logger.Error(ex.InnerException, "File {fileName} could not be backuped. {exception}", file.FileNamePath(), fileExceptionEntry);

                    if (ex.RequestCancel)
                    {
                        _logger.Error("Backup job is being cancelled due to permanent storage exception.");
                        cancel = true;

                        await RequestShowErrorInsufficientDiskSpaceAsync();
                    }
                }
                catch (Exception ex)
                {
                    var fileExceptionEntry = AddFileErrorToList(newVersionDate, newVersionId, file, ex);
                    _logger.Error(ex.InnerException, "File {fileName} could not be backuped. {exception}", file.FileNamePath(), fileExceptionEntry);
                }

                // cancellation token requested?
                if (token.IsCancellationRequested || cancel)
                {
                    // report progress
                    _logger.Information("Cancellation of backup job requested. Rollback all transfers.");
                    ReportStatus(Resources.STATUS_CANCELLED_SHORT, Resources.STATUS_CANCELLED_TEXT);

                    // undo
                    storage.DeleteDirectory(newVersionDate);
                    storage.Dispose();

                    dbClient.RollbackTransaction();

                    // standby
                    Win32Stuff.AllowSystemSleep();

                    ReportState(JobState.CANCELED);
                    return;
                }
            }

            // report exceptions during job
            if (FileErrorList.Count > 0)
            {
                _logger.Error("{numFiles} files could not be copied to device.", FileErrorList.Count);

                // can we still write to device?
                if (!storage.CanWriteToStorage())
                {
                    // report progress
                    _logger.Error("Cannot write to backup device. Rollback all transfers.");
                    ReportStatus(Resources.STATUS_CANCELLED_SHORT, Resources.STATUS_CANCELLED_ERROR);

                    // undo
                    storage.DeleteDirectory(newVersionDate);
                    storage.Dispose();

                    dbClient.RollbackTransaction();

                    // standby
                    Win32Stuff.AllowSystemSleep();

                    ReportExceptions(FileErrorList);
                    ReportState(JobState.ERROR);
                    return;
                }
            }

            // commit backup
            if (newFiles)
            {
                dbClient.CommitTransaction();
            }
            else
            {
                // no new files, so optimize workload
                dbClient.RollbackTransaction();

                try
                {
                    var lastVersion = await queryManager.GetLastBackupAsync();

                    if (lastVersion != null && storage.RenameDirectory(lastVersion.CreationDate.ToString("dd-MM-yyyy HH-mm-ss"), newVersionDate))
                    {
                        await backupMutationRepository.RenameVersionDateAsync(dbClient, long.Parse(lastVersion.Id), newVersionDate);
                    }
                }
                catch (Exception ex)
                {
                    // don't do anything
                    _logger.Error(ex, "Backup could not be refreshed. New backup will be ignored.");
                }
            }

            // save version infos
            configurationManager.LastBackupDone = newVersionDate;
            configurationManager.LastVersionDate = "";

            if (int.TryParse(configurationManager.OldBackupPrevent, out var databaseVersion))
            {
                configurationManager.OldBackupPrevent = (databaseVersion + 1).ToString();
            }
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

        ReportStatus(Resources.STATUS_BACKUP_FINISHED_SHORT, Resources.STATUS_BACKUP_FINISHED_TEXT);
        ReportState(FileErrorList.Count > 0 ? JobState.ERROR : JobState.FINISHED);

        _logger.Information("Backup job finished.");
    }

    private async Task ProcessEmptyFolders(DbClient dbClient, long newVersionId, List<FolderTableRow> emptyFolder)
    {
        foreach (var folder in emptyFolder)
        {
            // backup folder
            var folderPath = "\\" + Path.Combine(Path.GetFileName(folder.RootPath), IOUtils.GetRelativeFolder(folder.Folder, folder.RootPath)) + "\\";
            var folderId = await backupMutationRepository.AddOrGetFolderIdAsync(dbClient, folderPath);
            await backupMutationRepository.AddFolderLinkAsync(dbClient, folderId, newVersionId);
        }
    }

    /// <summary>
    /// Retrieve all folders that should be backupped.
    /// </summary>
    /// <param name="selectedFolders">List of source folders.</param>
    /// <returns></returns>
    private static List<string> GetSourceFolders(string[] selectedFolders)
    {
        var folderList = new List<string>();

        foreach (var folder in selectedFolders)
        {
            if (folder.Length > 3)
            {
                // single folder
                folderList.Add(folder);
                continue;
            }

            // entire drive
            var subFolders = Directory.GetDirectories(folder);

            foreach (var subFolder in subFolders)
            {
                try
                {
                    var folderInfo = new DirectoryInfo(subFolder);

                    // check if the folder is not a system folder
                    if ((folderInfo.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint &&
                        (folderInfo.Attributes & FileAttributes.System) != FileAttributes.System &&
                        (folderInfo.Attributes & FileAttributes.Temporary) != FileAttributes.Temporary)
                    {
                        folderList.Add(subFolder);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Could not obtain directory information for {dir}; directory will be ignored.", subFolder);
                }
            }
        }

        return folderList;
    }

    /// <summary>
    /// Stores the information that a folder junction is present for the
    /// backuped folder in the database.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="dbClient"></param>
    private async Task SaveJunctionAsync(FileTableRow file, DbClient dbClient)
    {
        var path = Path.GetDirectoryName(file.FileNamePath());
        if (!junctionFolders.Add(path))
        {
            return;
        }

        // get display name
        var displayName = Win32Stuff.GetDisplayName(path);

        if (!string.IsNullOrEmpty(displayName) && Path.GetFileName(path) != displayName)
        {
            path = Path.GetFileName(file.FileRoot) + path.Replace(file.FileRoot, "", StringComparison.OrdinalIgnoreCase);
            await backupMutationRepository.AddFolderJunctionAsync(dbClient, path, displayName);
        }
    }

    /// <summary>
    /// Copies a single file from the local source to the backup device via the
    /// StorageManager.
    /// </summary>
    /// <param name="storage"></param>
    /// <param name="file"></param>
    /// <param name="newVersionId"></param>
    /// <param name="newVersionDate"></param>
    /// <param name="dbClient"></param>
    /// <param name="normalCopy"></param>
    /// <param name="useVss"></param>
    /// <returns></returns>
    /// <exception cref="FileNotProcessedException"></exception>
    private async Task<bool> CopyFileToDeviceAsync(IStorageProvider storage, FileTableRow file, long newVersionId, string newVersionDate, DbClient dbClient, bool normalCopy = false, bool useVss = false)
    {
        // file variables
        var localFileName = file.FileNamePath();
        var remoteFileName = Path.Combine(newVersionDate, Path.GetFileName(file.FileRoot), file.FilePath, file.FileName);
        var longFileName = "";

        // copy via vss?
        if (useVss)
        {
            try
            {
                _logger.Information("File {fileName} will be attempted to backup via Volume Shadow Copy Service.", localFileName);

                // temporary file path
                var vssTempFile = Path.Combine(Path.GetTempPath(), file.FileName);

                if (vssClient.CopyFile(localFileName, vssTempFile))
                {
                    if (!File.Exists(vssTempFile))
                    {
                        // we could not copy the file
                        _logger.Warning("File {fileName} could not be copied via Volume Shadow Copy Service.", localFileName);
                        throw new FileNotProcessedException();
                    }

                    // set new local file name
                    localFileName = vssTempFile;
                }
                else
                {
                    // error during copy, so delete temporary file
                    try
                    {
                        File.Delete(vssTempFile);
                    }
                    catch
                    {
                        // not necessary to handle this error
                    }

                    // we could not copy the file
                    _logger.Warning("File {fileName} could not be copied via Volume Shadow Copy Service.", localFileName);
                    throw new FileNotProcessedException();
                }
            }
            catch (Exception ex)
            {
                // we could not copy the file
                throw new FileNotProcessedException(ex);
            }
        }

        // check if we need to compress the file
        var doNotCompress = false;

        if (configurationManager.Compression == 1)
        {
            var fileExt = Path.GetExtension(file.FileNamePath()).ToLower(CultureInfo.InvariantCulture);

            if (!string.IsNullOrEmpty(fileExt))
            {
                var exts = configurationManager.ExcludeCompression.Split('|');

                if (exts.Contains(fileExt))
                {
                    doNotCompress = true;
                }
            }

            if (CompressionUtils.IsCompressedFile(fileExt) || file.FileSize < 4 * 8)
            {
                doNotCompress = true;
            }
        }

        // compress or encrypt?
        var compress = configurationManager.Compression == 1 && !normalCopy && !doNotCompress;
        var encrypt = configurationManager.Encrypt == 1 && !normalCopy && file.FileSize > 0;

        // check if path is too long
        if (storage.IsPathTooLong(remoteFileName, compress, encrypt))
        {
            longFileName = Guid.NewGuid().ToString();
            longFileName += Path.GetExtension(localFileName);

            remoteFileName = Path.Combine(newVersionDate, "_LONGFILES_", longFileName);

            _logger.Debug("{fileName} path is too long to be copied, it will be renamed instead to {longFile}.", file.FileNamePath(), longFileName);
        }

        // send file to storage provider
        bool result;
        try
        {
            if (compress)
            {
                result = storage.CopyFileToStorageCompressed(localFileName, remoteFileName);
            }
            else if (encrypt)
            {
                result = storage.CopyFileToStorageEncrypted(localFileName, remoteFileName, Password);
            }
            else
            {
                result = storage.CopyFileToStorage(localFileName, remoteFileName);
            }

            if (useVss && localFileName.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Delete(localFileName);
                }
                catch
                {
                    // could not delete temporary file
                }
            }
        }
        catch (IOException ex)
        {
            _logger.Warning(ex, "Could not copy file {localFileName} due to IO error.", localFileName);

            // file does not exist anymore?
            if (ex.GetType() == typeof(DirectoryNotFoundException) || ex.GetType() == typeof(FileNotFoundException))
            {
                throw new FileNotProcessedException(ex);
            }

            // second attempt? or via VSS
            if (normalCopy || useVss)
            {
                throw new FileNotProcessedException(ex);
            }

            // special exception --> stop backup
            if (Win32Stuff.IsDiskFull(ex))
            {
                throw new FileNotProcessedException(ex, true);
            }

            return await CopyFileToDeviceAsync(storage, file, newVersionId, newVersionDate, dbClient, true, true);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Could not copy file {localFileName} via regular file copy.", localFileName);

            // already second attempt or other non-fixable error
            if (normalCopy || useVss)
            {
                throw new FileNotProcessedException(ex);
            }

            return await CopyFileToDeviceAsync(storage, file, newVersionId, newVersionDate, dbClient, true, true);
        }

        // backup successful?
        if (!result)
        {
            throw new FileNotProcessedException();
        }

        // add database entry
        await AddFileVersionDatabaseEntryAsync(dbClient, file, newVersionId, longFileName, compress, encrypt);

        return true;
    }

    /// <summary>
    /// Adds a new file version to the database.
    /// </summary>
    /// <param name="dbClient"></param>
    /// <param name="file"></param>
    /// <param name="newVersionId"></param>
    /// <param name="longFileName"></param>
    /// <param name="compress"></param>
    /// <param name="encrypt"></param>
    private async Task AddFileVersionDatabaseEntryAsync(DbClient dbClient, FileTableRow file, long newVersionId, string longFileName, bool compress, bool encrypt)
    {
        // correct path
        if (!file.FilePath.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
        {
            file.FilePath += "\\";
        }

        var fileType = 1;
        if (storage.Kind == StorageProviderKind.LocalFileSystem)
        {
            if (compress)
            {
                fileType = 2;
            }

            if (encrypt)
            {
                fileType = 6;
            }
        }
        else
        {
            fileType = 3;
            if (compress)
            {
                fileType = 4;
            }

            if (encrypt)
            {
                fileType = 5;
            }
        }

        if (!long.TryParse(file.FileId, out var fileId))
        {
            throw new InvalidOperationException("File id must be available before file version metadata can be stored.");
        }

        await backupMutationRepository.AddFileVersionWithLinkAsync(
            dbClient,
            fileId,
            newVersionId,
            file.FileSize,
            file.FileDateCreated,
            file.FileDateModified,
            fileType,
            longFileName);
    }
}
