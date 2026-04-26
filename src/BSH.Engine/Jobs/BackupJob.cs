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
    private const double SafetyMarginFactor = 1.2d;

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

            // Track whether this version contains any persisted backup content.
            // Reused file links and empty-folder links count as valid history too.
            var versionHasEntries = false;

            // store all folders or just the selection
            var folderList = GetSourceFolders(GetSelectedFolders());

            var folderListString = string.Join("|", folderList);

            var newVersionId = await backupMutationRepository.CreateVersionAsync(dbClient, newVersionDate, Title, Description, fullBackup, folderListString);

            ///
            /// PHASE 2 : Obtain new files and backup them
            /// 

            // obtain all files
            var (files, emptyFolder) = CollectBackupItems(folderList);

            // report progress
            _logger.Information("{numFiles} files and {numFolders} folders are collected for backup.", files.Count, emptyFolder.Count);

            ReportStatus(Resources.STATUS_BACKUP_COPY_SHORT, Resources.STATUS_BACKUP_COPY_TEXT);
            ReportProgress(files.Count, 0);

            // keep system running
            Win32Stuff.KeepSystemAwake();

            // process empty folders
            await ProcessEmptyFolders(dbClient, newVersionId, emptyFolder);

            if (emptyFolder.Count > 0)
            {
                versionHasEntries = true;
            }

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
                    var fileVersionId = await GetExistingFileVersionIdAsync(dbClient, file, fullBackup);

                    if (fileVersionId.HasValue)
                    {
                        // file is the same, so only create a link
                        await backupMutationRepository.AddFileLinkAsync(dbClient, fileVersionId.Value, newVersionId);
                        isModifiedFile = false;
                        versionHasEntries = true;
                    }
                    else if (string.IsNullOrEmpty(file.FileId))
                    {
                        // file does not have an entry
                        var filePath = GetBackupFilePath(file);
                        var fileId = await backupMutationRepository.CreateFileAsync(dbClient, file.FileName, filePath);
                        file.FileId = fileId.ToString();
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

                            versionHasEntries = true;
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

            if (!versionHasEntries)
            {
                _logger.Warning("Backup version {versionDate} has no persisted content. Rolling back version.", newVersionDate);
                dbClient.RollbackTransaction();
                configurationManager.LastVersionDate = "";
            }
            else
            {
                // Commit the new version even if all files were reused from older backups.
                // The version row and file links are the backup history.
                dbClient.CommitTransaction();

                // save version infos
                configurationManager.LastBackupDone = newVersionDate;
                configurationManager.LastVersionDate = "";

                if (int.TryParse(configurationManager.OldBackupPrevent, out var databaseVersion))
                {
                    configurationManager.OldBackupPrevent = (databaseVersion + 1).ToString();
                }
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

    public async Task<long?> EstimateRequiredSpaceAsync()
    {
        try
        {
            using var dbClient = dbClientFactory.CreateDbClient();

            var lastVersionDate = await versionQueryRepository.GetLastVersionDateAsync(dbClient);
            var fullBackup = string.IsNullOrEmpty(lastVersionDate?.ToString()) || FullBackup;

            var selectedFolders = GetSelectedFolders();
            if (selectedFolders.Length == 0)
            {
                return null;
            }

            var folderList = GetSourceFolders(selectedFolders);
            if (folderList.Count == 0)
            {
                return null;
            }

            var (files, _) = CollectBackupItems(folderList);
            long requiredSpace = 0;

            foreach (var file in files)
            {
                if (await GetExistingFileVersionIdAsync(dbClient, file, fullBackup) != null)
                {
                    continue;
                }

                requiredSpace += (long)Math.Ceiling(file.FileSize);
            }

            return ApplySafetyMargin(requiredSpace);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Backup space could not be estimated.");
            return null;
        }
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

    private IFileCollectorService CreateFileCollector()
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

        return fileCollector;
    }

    private string[] GetSelectedFolders()
    {
        var selectedFolders = string.IsNullOrEmpty(Sources) ? SourceFolder : Sources;
        return string.IsNullOrWhiteSpace(selectedFolders) ? Array.Empty<string>() : selectedFolders.Split('|');
    }

    private (List<FileTableRow> files, List<FolderTableRow> emptyFolders) CollectBackupItems(List<string> folderList)
    {
        var files = new List<FileTableRow>();
        var emptyFolders = new List<FolderTableRow>();

        foreach (var folderEntry in folderList)
        {
            var fileCollector = CreateFileCollector();
            var filesList = fileCollector.GetLocalFileList(folderEntry, true);
            emptyFolders.AddRange(fileCollector.EmptyFolders);
            files.AddRange(filesList);
        }

        return (files, emptyFolders);
    }

    private async Task<long?> GetExistingFileVersionIdAsync(DbClient dbClient, FileTableRow file, bool fullBackup)
    {
        var filePath = GetBackupFilePath(file);
        var fileId = await backupMutationRepository.GetFileIdAsync(dbClient, file.FileName, filePath);
        file.FileId = fileId?.ToString();

        if (!fileId.HasValue || fullBackup)
        {
            return null;
        }

        return await backupMutationRepository.GetMatchingFileVersionIdAsync(dbClient, fileId.Value, file.FileSize, file.FileDateModified);
    }

    private static string GetBackupFilePath(FileTableRow file)
    {
        return "\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\";
    }

    private static long ApplySafetyMargin(long bytes)
    {
        if (bytes <= 0)
        {
            return 0;
        }

        return (long)Math.Ceiling(bytes * SafetyMarginFactor);
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
        var localFileName = file.FileNamePath();
        var remoteFileName = Path.Combine(newVersionDate, Path.GetFileName(file.FileRoot), file.FilePath, file.FileName);
        var longFileName = "";

        if (useVss)
        {
            localFileName = PrepareVssTempFile(localFileName, file);
        }

        var compress = ShouldCompress(file, normalCopy);
        var encrypt = ShouldEncrypt(file, normalCopy);
        remoteFileName = EnsureSupportedRemotePath(storage, remoteFileName, localFileName, file, newVersionDate, compress, encrypt, out longFileName);

        bool result;
        try
        {
            result = CopyFileToStorage(storage, localFileName, remoteFileName, compress, encrypt);
            DeleteVssTempFile(localFileName, useVss);
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

    private string PrepareVssTempFile(string localFileName, FileTableRow file)
    {
        try
        {
            _logger.Information("File {fileName} will be attempted to backup via Volume Shadow Copy Service.", localFileName);

            var vssTempFile = Path.Combine(Path.GetTempPath(), file.FileName);
            if (vssClient.CopyFile(localFileName, vssTempFile) && File.Exists(vssTempFile))
            {
                return vssTempFile;
            }

            TryDeleteFile(vssTempFile);
            _logger.Warning("File {fileName} could not be copied via Volume Shadow Copy Service.", localFileName);
            throw new FileNotProcessedException();
        }
        catch (Exception ex)
        {
            throw new FileNotProcessedException(ex);
        }
    }

    private bool ShouldCompress(FileTableRow file, bool normalCopy)
    {
        if (configurationManager.Compression != 1 || normalCopy)
        {
            return false;
        }

        var fileExt = Path.GetExtension(file.FileNamePath()).ToLower(CultureInfo.InvariantCulture);
        if (CompressionUtils.IsCompressedFile(fileExt) || file.FileSize < 4 * 8)
        {
            return false;
        }

        if (string.IsNullOrEmpty(fileExt))
        {
            return true;
        }

        var excludedExtensions = configurationManager.ExcludeCompression.Split('|');
        return !excludedExtensions.Contains(fileExt);
    }

    private bool ShouldEncrypt(FileTableRow file, bool normalCopy)
    {
        return configurationManager.Encrypt == 1 && !normalCopy && file.FileSize > 0;
    }

    private string EnsureSupportedRemotePath(
        IStorageProvider storage,
        string remoteFileName,
        string localFileName,
        FileTableRow file,
        string newVersionDate,
        bool compress,
        bool encrypt,
        out string longFileName)
    {
        longFileName = "";
        if (!storage.IsPathTooLong(remoteFileName, compress, encrypt))
        {
            return remoteFileName;
        }

        longFileName = Guid.NewGuid() + Path.GetExtension(localFileName);
        _logger.Debug("{fileName} path is too long to be copied, it will be renamed instead to {longFile}.", file.FileNamePath(), longFileName);
        return Path.Combine(newVersionDate, "_LONGFILES_", longFileName);
    }

    private bool CopyFileToStorage(IStorageProvider storage, string localFileName, string remoteFileName, bool compress, bool encrypt)
    {
        if (compress)
        {
            return storage.CopyFileToStorageCompressed(localFileName, remoteFileName);
        }

        if (encrypt)
        {
            return storage.CopyFileToStorageEncrypted(localFileName, remoteFileName, Password);
        }

        return storage.CopyFileToStorage(localFileName, remoteFileName);
    }

    private static void DeleteVssTempFile(string localFileName, bool useVss)
    {
        if (!useVss || !localFileName.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        TryDeleteFile(localFileName);
    }

    private static void TryDeleteFile(string fileName)
    {
        try
        {
            File.Delete(fileName);
        }
        catch
        {
            // not critical
        }
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
