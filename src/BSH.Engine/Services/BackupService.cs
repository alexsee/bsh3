// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Serilog;

namespace Brightbits.BSH.Engine.Services;

public class BackupService : IBackupService
{
    private static readonly ILogger _logger = Log.ForContext<BackupService>();

    private readonly IConfigurationManager configurationManager;

    private readonly IQueryManager queryManager;

    private readonly IStorageFactory storageFactory;

    private readonly IDbClientFactory dbClientFactory;
    private readonly IVssClient vssClient;
    private readonly IFileCollectorServiceFactory fileCollectorServiceFactory;
    private readonly IVersionQueryRepository versionQueryRepository;
    private readonly IBackupMutationRepository backupMutationRepository;

    private Task currentTask;

    private string password;

    private DateTime lastMediaCheckDate;

    private bool lastMediaCheckResult;

    public BackupService(
        IConfigurationManager configurationManager,
        IQueryManager queryManager,
        IDbClientFactory dbClientFactory,
        IStorageFactory storageFactory,
        IVssClient vssClient,
        IFileCollectorServiceFactory fileCollectorServiceFactory,
        IVersionQueryRepository versionQueryRepository,
        IBackupMutationRepository backupMutationRepository)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);
        ArgumentNullException.ThrowIfNull(queryManager);
        ArgumentNullException.ThrowIfNull(dbClientFactory);
        ArgumentNullException.ThrowIfNull(storageFactory);
        ArgumentNullException.ThrowIfNull(vssClient);
        ArgumentNullException.ThrowIfNull(fileCollectorServiceFactory);
        ArgumentNullException.ThrowIfNull(versionQueryRepository);
        ArgumentNullException.ThrowIfNull(backupMutationRepository);

        this.configurationManager = configurationManager;
        this.queryManager = queryManager;
        this.dbClientFactory = dbClientFactory;
        this.storageFactory = storageFactory;
        this.vssClient = vssClient;
        this.fileCollectorServiceFactory = fileCollectorServiceFactory;
        this.versionQueryRepository = versionQueryRepository;
        this.backupMutationRepository = backupMutationRepository;
    }

    /// <summary>
    /// Checks if the current storage device/system is available.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CheckMedia(bool quickCheck = false)
    {
        using (var storage = storageFactory.GetCurrentStorageProvider())
        {
            // have we checked before?
            if (lastMediaCheckResult && storage.Kind != StorageProviderKind.Ftp && DateTime.Now.Subtract(lastMediaCheckDate).TotalSeconds < 30)
            {
                lastMediaCheckDate = DateTime.Now;
                return lastMediaCheckResult;
            }

            // check
            lastMediaCheckDate = DateTime.Now;
            lastMediaCheckResult = await storage.CheckMedium(quickCheck);
        }

        return lastMediaCheckResult;
    }

    /// <summary>
    /// Sets the current password.
    /// </summary>
    /// <param name="password"></param>
    public void SetPassword(string password)
    {
        this.password = password;
    }

    /// <summary>
    /// Returns the current password.
    /// </summary>
    /// <returns></returns>
    public string GetPassword()
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
    public Task StartBackup(
        string title,
        string description,
        IJobReport jobReport,
        CancellationToken cancellationToken,
        bool fullBackup = false,
        string sources = "",
        bool silent = false)
    {
        var backupJob = new BackupJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            fileCollectorServiceFactory,
            this.vssClient,
            versionQueryRepository,
            backupMutationRepository,
            silent)
        {
            Title = title,
            Description = description,
            FullBackup = fullBackup,
            Sources = sources,
            SourceFolder = configurationManager.SourceFolder,
            Password = password
        };

        return StartJob(jobReport, ActionType.Backup, backupJob, () => backupJob.BackupAsync(cancellationToken), cancellationToken, silent, requirePassword: true);
    }

    /// <summary>
    /// Starts a restore job with the given parameters.
    /// </summary>
    public Task StartRestore(
        string version,
        string file,
        string destination,
        IJobReport jobReport,
        CancellationToken cancellationToken,
        FileOverwrite overwrite = FileOverwrite.Ask,
        bool silent = false)
    {
        var restoreJob = new RestoreJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository)
        {
            Version = int.Parse(version),
            File = file,
            Destination = destination,
            FileOverwrite = overwrite,
            Password = password
        };

        return StartJob(jobReport, ActionType.Restore, restoreJob, () => restoreJob.RestoreAsync(cancellationToken), cancellationToken, silent, requirePassword: true);
    }

    /// <summary>
    /// Starts a deletion job for the given version.
    /// </summary>
    public Task StartDelete(
        string version,
        IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false)
    {
        var deleteJob = new DeleteJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository,
            silent)
        {
            Version = version
        };

        return StartJob(jobReport, ActionType.Delete, deleteJob, deleteJob.DeleteAsync, cancellationToken, silent);
    }

    /// <summary>
    /// Starts a new single deletion job given the file or path filter.
    /// When <paramref name="versionIds"/> is provided, only those backup versions are targeted.
    /// </summary>
    public Task StartDeleteSingle(
        string fileFilter,
        string pathFilter,
        IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false,
        IReadOnlyList<int> versionIds = null)
    {
        var deleteJob = new DeleteSingleJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository);

        return StartJob(jobReport, ActionType.Delete, deleteJob, () => deleteJob.DeleteSingleAsync(fileFilter, pathFilter, versionIds), cancellationToken, silent);
    }

    /// <summary>
    /// Starts a new edit job.
    /// </summary>
    public Task StartEdit(
        IJobReport jobReport,
        CancellationToken cancellationToken,
        bool silent = false)
    {
        var editJob = new EditJob(storageFactory.GetCurrentStorageProvider(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository)
        {
            Password = password
        };

        return StartJob(jobReport, ActionType.Modify, editJob, editJob.EditAsync, cancellationToken, silent, requirePassword: true);
    }

    private Task StartJob(
        IJobReport jobReport,
        ActionType actionType,
        Job job,
        Func<Task> runAsync,
        CancellationToken cancellationToken,
        bool silent,
        bool requirePassword = false)
    {
        ArgumentNullException.ThrowIfNull(jobReport);
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(runAsync);

        if (requirePassword && configurationManager.Encrypt == 1 && (password == null || password.Length == 0))
        {
            throw new PasswordRequiredException();
        }

        if (currentTask != null && currentTask.Status == TaskStatus.Running)
        {
            return Task.FromResult<object>(null);
        }

        job.AddObserver(jobReport);
        jobReport.ReportAction(actionType, silent);

        currentTask = Task.Run(runAsync, cancellationToken);
        currentTask.ContinueWith(t =>
        {
            _logger.Error(t.Exception, "Exception during {action} job.", actionType);
            job.ReportState(JobState.ERROR);
        }, TaskContinuationOptions.OnlyOnFaulted);

        return currentTask;
    }

    /// <summary>
    /// Sets the stable state of a backup.
    /// </summary>
    public async Task SetStableAsync(string version, bool stable)
    {
        await backupMutationRepository.SetVersionStableAsync(version, stable);
    }

    /// <summary>
    /// Edits the details of a version.
    /// </summary>
    public async Task UpdateVersionAsync(string version, VersionDetails versionDetails)
    {
        ArgumentNullException.ThrowIfNull(versionDetails);
        if (!long.TryParse(version, out var versionId))
        {
            throw new ArgumentException("Invalid version ID", nameof(version));
        }

        await backupMutationRepository.UpdateVersionDetailsAsync(versionId, versionDetails.Title, versionDetails.Description);
    }
}
