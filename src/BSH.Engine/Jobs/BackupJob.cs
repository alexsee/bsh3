// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Properties;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using Brightbits.BSH.Engine.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Jobs
{
    /// <summary>
    /// Class for the backup task
    /// </summary>
    public class BackupJob : Job
    {
        private static readonly ILogger _logger = Log.ForContext<BackupJob>();

        private readonly HashSet<string> junctionFolders = new HashSet<string>();

        public string Title { get; set; }

        public string Description { get; set; }

        public bool FullBackup { get; set; }

        public string Sources { get; set; }

        public string SourceFolder { get; set; }

        public SecureString Password { get; set; }

        public List<FileExceptionEntry> FileErrorList { get; set; }

        public BackupJob(IStorage storage,
            DbClientFactory dbClientFactory,
            QueryManager queryManager,
            bool silent = false) : base(storage, dbClientFactory, queryManager, silent)
        {
            FileErrorList = new List<FileExceptionEntry>();
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

            // connect to database
            using (var dbClient = dbClientFactory.CreateDbClient())
            {
                ///
                /// PHASE 1 : Initialization
                ///

                // get last version
                var lastVersionDate = await dbClient.ExecuteScalarAsync("SELECT versionDate FROM versiontable ORDER BY versionID LIMIT 1");

                // full backup?
                var fullBackup = string.IsNullOrEmpty(lastVersionDate?.ToString()) || FullBackup;

                // check medium
                if (!storage.CheckMedium())
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

                // open storage
                storage.Open();

                // get backup version date
                var newVersionDate = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");

                // save current backup version, so we know in case of an error that something wrong happend
                queryManager.Configuration.LastVersionDate = newVersionDate;

                // begin with transaction
                dbClient.BeginTransaction();

                // variable for new files
                var newFiles = false;

                // store all folders or just the selection
                var selectedFolders = string.IsNullOrEmpty(Sources) ? SourceFolder.Split('|') : Sources.Split('|');
                var folderList = GetSourceFolders(selectedFolders);

                var folderListString = string.Join("|", folderList);

                // create new backup
                var backupParameters = new IDataParameter[]
                {
                    dbClient.CreateParameter("newVersionDate", DbType.String, 0, newVersionDate),
                    dbClient.CreateParameter("title", DbType.String, 0, Title),
                    dbClient.CreateParameter("description", DbType.String, 0, Description),
                    dbClient.CreateParameter("type", DbType.String, 0, fullBackup ? "2" : "1"),
                    dbClient.CreateParameter("sources", DbType.String, 0, folderListString)
                };

                var newVersionId = (long)await dbClient.ExecuteScalarAsync(CommandType.Text,
                    "INSERT INTO versiontable (versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable, versionSources) VALUES (" +
                    "@newVersionDate, @title, @description, @type, '0', '0', @sources); select last_insert_rowid()",
                    backupParameters);

                ///
                /// PHASE 2 : Obtain new files and backup them
                /// 

                // obtain all files
                var files = new List<FileTableRow>();
                var emptyFolder = new List<FolderTableRow>();

                foreach (var folderEntry in folderList)
                {
                    var fileCollector = new FileCollectorService(this.queryManager);
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
                foreach (var folder in emptyFolder)
                {
                    // backup folder
                    var folderParameters = new IDataParameter[]
                    {
                        dbClient.CreateParameter("folder", DbType.String, 0, "\\" + Path.Combine(Path.GetFileName(folder.RootPath), FileCollectorService.GetRelativeFolder(folder.Folder, folder.RootPath)) + "\\")
                    };

                    var folderId = await dbClient.ExecuteScalarAsync(CommandType.Text, "INSERT OR IGNORE INTO foldertable ( folder ) VALUES ( @folder ); SELECT id FROM foldertable WHERE folder = @folder", folderParameters);

                    // add folder link
                    var folderLinkParameters = new IDataParameter[] {
                        dbClient.CreateParameter("folderid", DbType.Int32, 0, folderId),
                        dbClient.CreateParameter("versionID", DbType.Int32, 0, newVersionId)
                    };

                    await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT INTO folderlink ( folderid, versionid ) VALUES ( @folderid, @versionID )", folderLinkParameters);
                }

                // process all files
                bool cancel = false;
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    ReportProgress(files.Count, i);

                    var isModifiedFile = true;

                    try
                    {
                        // refresh status
                        ReportFileProgress(file.FileNamePath());

                        // search for database entry
                        var fileSelectParameters = new IDataParameter[]
                        {
                            dbClient.CreateParameter("fileName", DbType.String, 0, file.FileName),
                            dbClient.CreateParameter("filePath", DbType.String, 0, "\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\")
                        };

                        file.FileId = (await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT fileID FROM filetable WHERE fileName = @fileName AND filePath = @filePath LIMIT 1", fileSelectParameters))?.ToString();

                        if (!long.TryParse(file.FileId, out long fileId))
                        {
                            // file does not have an entry
                            var fileInsertParameters = new IDataParameter[] {
                                dbClient.CreateParameter("fileName", DbType.String, 0, file.FileName),
                                dbClient.CreateParameter("filePath", DbType.String, 0, "\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\")
                            };

                            file.FileId = (await dbClient.ExecuteScalarAsync(CommandType.Text, "INSERT INTO filetable ( fileName, filePath ) VALUES ( @fileName, @filePath ); SELECT MAX(fileID) FROM filetable", fileInsertParameters))?.ToString();
                        }
                        else
                        {
                            // file was found, so we already have a version of the file
                            if (!FullBackup)
                            {
                                var fileSelectParameters2 = new IDataParameter[] {
                                    dbClient.CreateParameter("fileID", DbType.Int32, 0, fileId),
                                    dbClient.CreateParameter("fileSize", DbType.Double, 0, file.FileSize),
                                    dbClient.CreateParameter("fileDateModified", DbType.Date, 0, file.FileDateModified)
                                };

                                file.FilePackage = (await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT fileversionID FROM fileversiontable WHERE" +
                                    " fileID = @fileID AND fileStatus = 1 AND fileSize = @fileSize AND datetime(fileDateModified) = datetime(@fileDateModified) ORDER BY fileversionID DESC LIMIT 1", fileSelectParameters2))?.ToString();

                                if (long.TryParse(file.FilePackage, out long filePackage))
                                {
                                    // file is the same, so only create a link
                                    var fileInsertParameters2 = new IDataParameter[] {
                                        dbClient.CreateParameter("fileversionID", DbType.Int32, 0, filePackage),
                                        dbClient.CreateParameter("versionID", DbType.Int32, 0, newVersionId)
                                    };

                                    await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT INTO filelink ( fileversionID, versionID ) VALUES ( @fileversionID, @versionID )", fileInsertParameters2);
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
                        FileExceptionEntry fileExceptionEntry = AddFileErrorToList(newVersionDate, newVersionId, file, ex);
                        _logger.Error(ex.InnerException, "File {fileName} could not be backuped.", file.FileNamePath(), new { fileExceptionEntry });

                        if (ex.RequestCancel)
                        {
                            _logger.Error("Backup job is being cancelled due to permanent storage exception.");
                            cancel = true;

                            RequestShowErrorInsufficientDiskSpace();
                        }
                    }
                    catch (Exception ex)
                    {
                        FileExceptionEntry fileExceptionEntry = AddFileErrorToList(newVersionDate, newVersionId, file, ex);
                        _logger.Error(ex.InnerException, "File {fileName} could not be backuped.", file.FileNamePath(), new { fileExceptionEntry });
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

                        ReportState(JobState.FINISHED);
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

                        if (lastVersion != null)
                        {
                            storage.RenameDirectory(lastVersion.CreationDate.ToString("dd-MM-yyyy HH-mm-ss"), newVersionDate);
                            await dbClient.ExecuteNonQueryAsync("UPDATE versiontable SET versionDate = '" + newVersionDate + "' WHERE versionID = " + lastVersion.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // don't do anything
                        _logger.Error(ex, "Backup could not be refreshed. New backup will be ignored.");
                    }
                }

                // save version infos
                queryManager.Configuration.LastBackupDone = newVersionDate;
                queryManager.Configuration.LastVersionDate = "";

                int.TryParse(queryManager.Configuration.OldBackupPrevent, out int databaseVersion);
                queryManager.Configuration.OldBackupPrevent = (databaseVersion + 1).ToString();
            }

            // refresh free diskspace
            await UpdateFreeDiskSpaceAsync();

            // close all database connections
            dbClientFactory.ClosePool();

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

        /// <summary>
        /// Adds the given exception to the file exception list.
        /// </summary>
        /// <param name="versionDate">The version date of the backup.</param>
        /// <param name="versionId">The version id of the backup.</param>
        /// <param name="file">The file that could not be copied.</param>
        /// <param name="ex">The exception that occured.</param>
        /// <returns></returns>
        private FileExceptionEntry AddFileErrorToList(string versionDate, long versionId, FileTableRow file, Exception ex)
        {
            var fileExceptionEntry = new FileExceptionEntry()
            {
                Exception = ex,
                File = file,
                NewVersionDate = versionDate,
                NewVersionId = versionId
            };

            FileErrorList.Add(fileExceptionEntry);
            return fileExceptionEntry;
        }

        /// <summary>
        /// Retrieve all folders that should be backupped.
        /// </summary>
        /// <param name="selectedFolders">List of source folders.</param>
        /// <returns></returns>
        private List<string> GetSourceFolders(string[] selectedFolders)
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
                path = Path.GetFileName(file.FileRoot) + path.Replace(file.FileRoot, "");

                var junctionInsertParameters = new IDataParameter[] {
                    dbClient.CreateParameter("path", DbType.String, 0, path),
                    dbClient.CreateParameter("displayName", DbType.String, 0, displayName)
                };

                await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT OR IGNORE INTO folderjunctiontable VALUES (@path, @displayName)", junctionInsertParameters);
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
        private async Task<bool> CopyFileToDeviceAsync(IStorage storage, FileTableRow file, double newVersionId, string newVersionDate, DbClient dbClient, bool normalCopy = false, bool useVss = false)
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
                    // temporary file path
                    var vssTempFile = Path.Combine(Path.GetTempPath(), file.FileName);

                    if (VolumeShadowCopyService.CopyFile(localFileName, vssTempFile))
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

            if (queryManager.Configuration.Compression == 1)
            {
                var fileExt = Path.GetExtension(file.FileNamePath()).ToLower();

                if (fileExt != "")
                {
                    var exts = queryManager.Configuration.ExcludeCompression.Split('|');

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
            var compress = queryManager.Configuration.Compression == 1 && !normalCopy && !doNotCompress;
            var encrypt = queryManager.Configuration.Encrypt == 1 && !normalCopy && file.FileSize != 0;

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

                if (useVss && localFileName.StartsWith(Path.GetTempPath()))
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
        private async Task AddFileVersionDatabaseEntryAsync(DbClient dbClient, FileTableRow file, double newVersionId, string longFileName, bool compress, bool encrypt)
        {
            // correct path
            if (!file.FilePath.EndsWith("\\"))
            {
                file.FilePath += "\\";
            }

            var fileType = 1;
            if (storage is FileSystemStorage)
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

            // update database
            var fileInsertParameters = new IDataParameter[] {
                    dbClient.CreateParameter("fileID", DbType.Int32, 0, file.FileId),
                    dbClient.CreateParameter("filePackage", DbType.Int32, 0, newVersionId),
                    dbClient.CreateParameter("fileSize", DbType.Double, 0, file.FileSize),
                    dbClient.CreateParameter("fileDateCreated", DbType.Date, 0, file.FileDateCreated),
                    dbClient.CreateParameter("fileDateModified", DbType.Date, 0, file.FileDateModified),
                    dbClient.CreateParameter("fileHash", DbType.String, 0, ""),
                    dbClient.CreateParameter("fileType", DbType.Double, 0, fileType),
                    dbClient.CreateParameter("fileStatus", DbType.Double, 0, 1),
                    dbClient.CreateParameter("longfilename", DbType.String, 0, longFileName)
                };

            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT INTO fileversiontable " +
                "( fileID, filePackage, fileSize, fileDateCreated, fileDateModified, fileHash, fileType, fileStatus, longfilename ) VALUES " +
                "( @fileID, @filePackage, @fileSize, @fileDateCreated, @fileDateModified, @fileHash, @fileType, @fileStatus, @longfilename )", fileInsertParameters);

            // add file link
            var fileLinkInsertParameters = new IDataParameter[] {
                    dbClient.CreateParameter("versionID", DbType.Int32, 0, newVersionId)
                };
            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "INSERT INTO filelink ( fileversionID, versionID ) VALUES ( last_insert_rowid(), @versionID )", fileLinkInsertParameters);

        }
    }
}
