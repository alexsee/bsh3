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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Jobs
{
    /// <summary>
    /// Class for single file deletion
    /// </summary>
    public class DeleteSingleJob : Job
    {
        private static readonly ILogger _logger = Log.ForContext<DeleteJob>();

        public List<FileExceptionEntry> FileErrorList { get; set; }

        public DeleteSingleJob(IStorage storage, DbClientFactory dbClientFactory, QueryManager queryManager) : base(storage, dbClientFactory, queryManager)
        {
            FileErrorList = new List<FileExceptionEntry>();
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

                // get file list
                var fileIds = new List<int>();
                var fileVersionIds = new List<int>();

                string selectFileSQL;
                IDataParameter[] selectFileParameters;

                if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
                {
                    selectFileSQL = "SELECT ft.fileID " +
                                    "FROM fileTable AS ft " +
                                    "WHERE fileName = @fileName AND filePath = @filePath";
                    selectFileParameters = new IDataParameter[]
                    {
                        dbClient.CreateParameter("fileName", DbType.String, 0, fileFilter),
                        dbClient.CreateParameter("filePath", DbType.String, 0, pathFilter)
                    };
                }
                else
                {
                    selectFileSQL = "SELECT ft.fileID " +
                                    "FROM fileTable AS ft " +
                                    "WHERE filePath LIKE @filePath";
                    selectFileParameters = new IDataParameter[] {
                        dbClient.CreateParameter("filePath", DbType.String, 0, pathFilter)
                    };
                }

                // obtain files
                using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, selectFileSQL, selectFileParameters))
                {
                    while (reader.Read())
                    {
                        fileIds.Add(reader.GetInt32(0));
                    }
                    reader.Close();
                }

                // report progress
                _logger.Information("{numFiles} files determined for deletion.", fileIds.Count);
                ReportProgress(fileIds.Count, 0);

                foreach (var fileId in fileIds)
                {
                    // obtain all file versions
                    var deleteFileParams = new IDataParameter[]
                    {
                        dbClient.CreateParameter("fileId", DbType.Int32, 0, fileId)
                    };

                    using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text,
                        "SELECT * FROM fileVersionTable AS fvt " +
                                                        "INNER JOIN fileTable AS ft ON " +
                                                        "  ft.fileID = fvt.fileID " +
                                                        "INNER JOIN versionTable AS vt ON " +
                                                        "  fvt.filePackage = vt.versionID " +
                                                        "WHERE fvt.fileID = @fileId",
                        deleteFileParams);
                    int i = 0;
                    while (reader.Read())
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
                            FileErrorList.Add(new FileExceptionEntry()
                            {
                                File = new FileTableRow() { FileName = reader.GetString("fileName"), FilePath = reader.GetString("filePath") },
                                Exception = ex
                            });

                            _logger.Error(ex.InnerException, "File {fileName} could not be deleted.", reader.GetString("fileName"));
                        }

                        // add to deleted id list
                        fileVersionIds.Add(reader.GetInt32(reader.GetOrdinal("fileversionid")));
                    }

                    reader.Close();
                }

                // delete metadata from database
                string subQuerySQL;
                var deleteParams = new List<IDataParameter>();

                if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
                {
                    subQuerySQL = "SELECT ft.fileID FROM fileTable AS ft WHERE fileName = @fileName AND filePath = @filePath";

                    deleteParams.Add(dbClient.CreateParameter("fileName", DbType.String, 0, fileFilter));
                    deleteParams.Add(dbClient.CreateParameter("filePath", DbType.String, 0, pathFilter));
                }
                else
                {
                    subQuerySQL = "SELECT ft.fileID FROM fileTable AS ft WHERE filePath LIKE @filePath";

                    deleteParams.Add(dbClient.CreateParameter("filePath", DbType.String, 0, pathFilter));
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
            if (int.TryParse(queryManager.Configuration.OldBackupPrevent, out int databaseVersion))
            {
                queryManager.Configuration.OldBackupPrevent = (databaseVersion + 1).ToString();
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
}