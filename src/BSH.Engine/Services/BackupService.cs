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

using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Storage;
using Serilog;

namespace Brightbits.BSH.Engine.Services;

public class BackupService : IBackupService
{
    private static readonly ILogger _logger = Log.ForContext<BackupService>();

    private readonly IConfigurationManager configurationManager;

    private readonly IQueryManager queryManager;

    private readonly IStorageFactory storageFactory;

    private readonly IDbClientFactory dbClientFactory;

    private Task currentTask;

    private SecureString password;

    private DateTime lastMediaCheckDate;

    private bool lastMediaCheckResult;

    public BackupService(IConfigurationManager configurationManager, IQueryManager queryManager, IDbClientFactory dbClientFactory, IStorageFactory storageFactory)
    {
        this.configurationManager = configurationManager;
        this.queryManager = queryManager;
        this.dbClientFactory = dbClientFactory;
        this.storageFactory = storageFactory;
    }

    /// <summary>
    /// Checks if the current storage device/system is available.
    /// </summary>
    /// <returns></returns>
    public bool CheckMedia(bool quickCheck = false)
    {
        using (var storage = storageFactory.GetCurrentStorageProvider())
        {
            // have we checked before?
            if (lastMediaCheckResult && storage.GetType() != typeof(FTPStorage) && DateTime.Now.Subtract(lastMediaCheckDate).TotalSeconds < 30)
            {
                lastMediaCheckDate = DateTime.Now;
                return lastMediaCheckResult;
            }

            // check
            lastMediaCheckDate = DateTime.Now;
            lastMediaCheckResult = storage.CheckMedium(quickCheck);
        }

        return lastMediaCheckResult;
    }

    /// <summary>
    /// Sets the current password.
    /// </summary>
    /// <param name="password"></param>
    public void SetPassword(SecureString password)
    {
        this.password = password;
    }

    /// <summary>
    /// Returns the current password.
    /// </summary>
    /// <returns></returns>
    public SecureString GetPassword()
    {
        return this.password;
    }

    /// <summary>
    /// Returns if a password has been set.
    /// </summary>
    /// <returns></returns>
    public bool HasPassword()
    {
        return this.password != null && this.password.Length > 0;
    }

    /// <summary>
    /// Updates the database file on the storage device/system.
    /// </summary>
    /// <param name="databaseFile"></param>
    public void UpdateDatabaseFile(string databaseFile)
    {
        DbClientFactory.ClosePool();

        using var storage = storageFactory.GetCurrentStorageProvider();
        storage.Open();
        storage.UploadDatabaseFile(databaseFile);
    }

    /// <summary>
    /// Starts a backup job with the given parameters.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="jobReport"></param>
    /// <param name="fullBackup"></param>
    /// <param name="sources"></param>
    public Task StartBackup(
        string title,
        string description,
        ref IJobReport jobReport,
        CancellationToken cancellationToken,
        bool fullBackup = false,
        string sources = "",
        bool silent = false)
    {
        // check if password is set
        if (configurationManager.Encrypt == 1 && (password == null || password.Length == 0))
        {
            throw new PasswordRequiredException();
        }

        // check if we have a running task
        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        // create new backup job
        var backupJob = new BackupJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            new FileCollectorServiceFactory(),
            silent)
        {
            Title = title,
            Description = description,
            FullBackup = fullBackup,
            Sources = sources,
            SourceFolder = configurationManager.SourceFolder,
            Password = password
        };

        backupJob.AddObserver(jobReport);

        // start backup
        jobReport.ReportAction(ActionType.Backup, silent);

        currentTask = Task.Factory.StartNew(async () => await backupJob.BackupAsync(cancellationToken));

        // error handling
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during backup job.");
            backupJob.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Starts a restore job with the given parameters.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="destination"></param>
    /// <param name="jobReport"></param>
    /// <param name="overwrite"></param>
    public Task StartRestore(
        string version,
        string file,
        string destination,
        ref IJobReport jobReport,
        CancellationToken cancellationToken,
        FileOverwrite overwrite = FileOverwrite.Ask,
        bool silent = false)
    {
        // check if we have a running task
        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        // check if password is set
        if (configurationManager.Encrypt == 1 && (password == null || password.Length == 0))
        {
            throw new PasswordRequiredException();
        }

        // create new restore job
        var restoreJob = new RestoreJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager)
        {
            Version = int.Parse(version),
            File = file,
            Destination = destination,
            FileOverwrite = overwrite,
            Password = password
        };

        restoreJob.AddObserver(jobReport);

        // run restore
        jobReport.ReportAction(ActionType.Restore, silent);

        currentTask = Task.Factory.StartNew(() => restoreJob.RestoreAsync(cancellationToken));

        // error handling
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during restore job.");
            restoreJob.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Starts a deletion job for the given version.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="jobReport"></param>
    public Task StartDelete(
        string version,
        ref IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false)
    {
        // check if we have a running task
        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        // create deletion job
        var deleteJob = new DeleteJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            silent)
        {
            Version = version
        };

        deleteJob.AddObserver(jobReport);

        // run delete
        jobReport.ReportAction(ActionType.Delete, silent);

        currentTask = Task.Factory.StartNew(async () => await deleteJob.DeleteAsync());

        // error handling
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during delete job.");
            deleteJob.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Starts a new single deletion job given the file or path filter.
    /// </summary>
    /// <param name="fileFilter"></param>
    /// <param name="pathFilter"></param>
    /// <param name="jobReport"></param>
    public Task StartDeleteSingle(
        string fileFilter,
        string pathFilter,
        ref IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false)
    {
        // check if we have a running task
        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        // create deletion job
        var deleteJob = new DeleteSingleJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager);

        deleteJob.AddObserver(jobReport);

        // run delete
        jobReport.ReportAction(ActionType.Delete, silent);

        currentTask = Task.Factory.StartNew(async () => await deleteJob.DeleteSingleAsync(fileFilter, pathFilter));

        // error handling
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during delete job.");
            deleteJob.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Starts a new edit job.
    /// </summary>
    /// <param name="jobReport"></param>
    public Task StartEdit(
        ref IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false)
    {
        // check if we have a running task
        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        // check if password is set
        if (configurationManager.Encrypt == 1 && (password == null || password.Length == 0))
        {
            throw new PasswordRequiredException();
        }

        // create edit job
        var editJob = new EditJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager)
        {
            Password = password
        };

        editJob.AddObserver(jobReport);

        // run edit
        jobReport.ReportAction(ActionType.Modify, silent);

        currentTask = Task.Factory.StartNew(() => editJob.EditAsync());

        // error handling
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during edit job.");
            editJob.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Sets the stable state of a backup.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="stable"></param>
    public async Task SetStableAsync(string version, bool stable)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        await dbClient.ExecuteNonQueryAsync($"UPDATE versiontable SET versionStable = {(stable ? 1 : 0)} WHERE versionID = {version}");
    }
}
