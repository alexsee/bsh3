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
using Brightbits.BSH.Engine.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Jobs
{
    /// <summary>
    /// Class for all job tasks
    /// </summary>
    public abstract class Job
    {
        private static readonly ILogger _logger = Log.ForContext<Job>();

        protected readonly IStorage storage;

        protected readonly DbClientFactory dbClientFactory;

        protected readonly QueryManager queryManager;

        protected readonly bool silent;

        private readonly List<IJobReport> observers = new List<IJobReport>();

        protected Job(IStorage storage, DbClientFactory dbClientFactory, QueryManager queryManager, bool silent = false)
        {
            this.storage = storage;
            this.dbClientFactory = dbClientFactory;
            this.queryManager = queryManager;

            this.silent = silent;
        }

        public void ReportState(JobState jobState)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.ReportState(jobState);
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        protected void ReportStatus(string title, string text)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.ReportStatus(title, text);
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        protected void ReportProgress(int total, int current)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.ReportProgress(total, current);
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        protected void ReportFileProgress(string file)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.ReportFileProgress(file);
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        protected void ReportExceptions(List<FileExceptionEntry> files)
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.ReportExceptions(files, this.silent);
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        protected RequestOverwriteResult RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
        {
            if (observers.Count > 0)
            {
                return observers[0].RequestOverwrite(localFile, remoteFile);
            }

            return RequestOverwriteResult.Overwrite;
        }

        protected void RequestShowErrorInsufficientDiskSpace()
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.RequestShowErrorInsufficientDiskSpace();
                }
                catch
                {
                    // ignore exception
                }
            }
        }

        public void AddObserver(IJobReport observer)
        {
            try
            {
                observers.Add(observer);
            }
            catch
            {
                // ignore exception
            }
        }

        public void RemoveObserver(IJobReport observer)
        {
            try
            {
                observers.Remove(observer);
            }
            catch
            {
                // ignore exception
            }
        }

        /// <summary>
        /// Updates the database on the storage device. Storage must still be open.
        /// </summary>
        /// <exception cref="DatabaseFileNotUpdatedException"></exception>
        protected void UpdateDatabaseOnStorage()
        {
            try
            {
                storage.UpdateStorageVersion(int.Parse(queryManager.Configuration.OldBackupPrevent));
                storage.UploadDatabaseFile(queryManager.DatabaseFile);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Database file could not be refreshed on storage device.");

                ReportState(JobState.ERROR);

                // standby mode
                Win32Stuff.AllowSystemSleep();
                storage.Dispose();

                throw new DatabaseFileNotUpdatedException();
            }
        }

        /// <summary>
        /// Updates the free disk space on the database.
        /// </summary>
        protected async Task UpdateFreeDiskSpaceAsync()
        {
            try
            {
                queryManager.Configuration.FreeSpace = storage.GetFreeSpace().ToString();

                using (var dbClient = dbClientFactory.CreateDbClient())
                {
                    queryManager.Configuration.BackupSize = (await dbClient.ExecuteScalarAsync("SELECT SUM(FileSize) FROM fileversiontable")).ToString();
                }
            }
            catch (Exception ex)
            {
                // not important
                _logger.Warning(ex, "Could not update free space variable due to exception.");
            }
        }
    }
}