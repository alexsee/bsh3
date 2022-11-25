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
using Brightbits.BSH.Engine.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Jobs
{
    /// <summary>
    /// Class for edit tasks
    /// </summary>
    public class EditJob : Job
    {
        private static readonly ILogger _logger = Log.ForContext<DeleteJob>();

        public SecureString Password { get; set; }

        public List<FileExceptionEntry> FileErrorList { get; set; }

        public EditJob(IStorage storage, DbClientFactory dbClientFactory, QueryManager queryManager) : base(storage, dbClientFactory, queryManager)
        {
            FileErrorList = new List<FileExceptionEntry>();
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
            if (!storage.CheckMedium())
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
                    int i = 0;
                    while (reader.Read())
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
                            EditFileFromDevice(
                                dbClient,
                                remoteFilePath,
                                reader.GetInt32("fileType"),
                                reader.GetInt32("fileversionid"));
                        }
                        catch (Exception ex)
                        {
                            var fileExceptionEntry = new FileExceptionEntry()
                            {
                                Exception = ex,
                                File = new FileTableRow()
                                {
                                    FilePath = reader.GetString("filePath"),
                                    FileName = reader.GetString("fileName")
                                }
                            };

                            FileErrorList.Add(fileExceptionEntry);

                            _logger.Error(ex.InnerException, "File {fileName} could not be edited.", remoteFilePath, new { fileExceptionEntry });
                        }
                    }

                    reader.Close();
                }

                dbClient.CommitTransaction();
            }

            // set new metadata
            queryManager.Configuration.Encrypt = 0;
            queryManager.Configuration.EncryptPassMD5 = "";

            // close all database connections
            dbClientFactory.ClosePool();

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
        private void EditFileFromDevice(DbClient dbClient, string remoteFile, int fileType, int fileVersionId)
        {
            if (fileType == 5)
            {
                // decrypt on device
                storage.DecryptOnStorage(remoteFile, Password);

                // modify entry
                dbClient.ExecuteNonQuery($"UPDATE fileversiontable SET fileType = 3 WHERE fileversionid = {fileVersionId}");
            }
            else if (fileType == 6)
            {
                // decrypt on device
                storage.DecryptOnStorage(remoteFile, Password);

                // modify entry
                dbClient.ExecuteNonQuery($"UPDATE fileversiontable SET fileType = 1 WHERE fileversionid = {fileVersionId}");
            }
        }
    }
}
