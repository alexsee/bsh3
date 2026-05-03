// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
using Serilog;

namespace BSH.MainApp.Services;

public class JobService : IJobService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<JobService>();

    private readonly IBackupService backupService;

    private readonly IConfigurationManager configurationManager;
    private readonly IStatusService statusService;
    private readonly IPresentationService presentationService;
    private readonly Func<IWaitForMediaService> waitForMediaServiceFactory;
    private readonly JobRuntime jobRuntime;
    private readonly JobSessionRunner jobSessionRunner;
    private readonly WinUIJobSessionPresenter presenter;
    private readonly WinUIStoredPasswordAdapter storedPasswordAdapter;

    public bool IsCancellationRequested => jobRuntime.IsCancellationRequested;

    /// <summary>
    /// Initializes a new instance of the JobService given the BackupService and
    /// ConfigurationManager instance.
    /// </summary>
    /// <param name="backupService"></param>
    /// <param name="configurationManager"></param>
    public JobService(
        IBackupService backupService,
        IConfigurationManager configurationManager,
        IStatusService statusService,
        IPresentationService presentationService,
        ILocalSettingsService localSettingsService,
        Func<IWaitForMediaService> waitForMediaServiceFactory)
    {
        ArgumentNullException.ThrowIfNull(waitForMediaServiceFactory);

        this.backupService = backupService;
        this.configurationManager = configurationManager;
        this.statusService = statusService;
        this.presentationService = presentationService;
        this.waitForMediaServiceFactory = waitForMediaServiceFactory;

        this.jobRuntime = new JobRuntime(
            backupService,
            () => this.statusService.IsTaskRunning(),
            () => this.configurationManager.Medium == "1",
            async (_, silent, cancellationTokenSource) =>
            {
                var waitForMediaService = this.waitForMediaServiceFactory();
                return await waitForMediaService.ExecuteAsync(silent, cancellationTokenSource);
            },
            this.RequestPassword);
        this.presenter = new WinUIJobSessionPresenter(this.presentationService, this.statusService, this.Cancel);
        this.storedPasswordAdapter = new WinUIStoredPasswordAdapter(localSettingsService);
        this.jobSessionRunner = new JobSessionRunner(
            backupService,
            this.jobRuntime,
            () => this.configurationManager.Encrypt == 1,
            () => this.configurationManager.EncryptPassMD5,
            this.storedPasswordAdapter);

        GetNewCancellationToken();
    }

    /// <summary>
    /// Creates a new CancellationToken and assigns it to the current internal state.
    /// </summary>
    /// <returns>Returns the new CancellationToken.</returns>
    public CancellationToken GetNewCancellationToken()
    {
        return jobRuntime.GetNewCancellationToken();
    }

    /// <summary>
    /// Cancels the current executed task.
    /// </summary>
    public void Cancel()
    {
        _logger.Debug("Cancellation of current task requested by user.");
        jobRuntime.Cancel();
    }

    /// <summary>
    /// Checks if the currently selected backup media is available.
    /// </summary>
    /// <param name="action">Specifies the action that should be checked.</param>
    /// <param name="silent">Specifies if no user visualization should be shown.</param>
    /// <returns>Returns true, if the backup media is available, otherwise false.</returns>
    public async Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
    {
        return await jobRuntime.CheckMediaAsync(action, silent);
    }

    /// <summary>
    /// Runs a new backup task given the corresponding options.
    /// </summary>
    /// <param name="title">Specifies the title of the backup.</param>
    /// <param name="description">Specifies the description of the backup.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <param name="fullBackup">Specifies if the backup should be a full backup.</param>
    /// <param name="shutdownPC">Specifies if the computer should be shut down after completion.</param>
    /// <param name="shutdownApp">Specifies if the application should be closed after completion.</param>
    /// <param name="sourceFolders">Specifies the source folders to consider for the backup.</param>
    /// <returns></returns>
    public async Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "")
    {
        _logger.Debug("Backup task is started with title: {title}, description: {description}, statusDialog: {statusDialog}, fullBackup: {fullBackup}",
            title, description, statusDialog, fullBackup);

        var result = await jobSessionRunner.RunSingleBackupAsync(title, description, presenter, statusDialog, fullBackup, sourceFolders);
        if (!await HandleSessionStartAsync(result, "backup"))
        {
            return false;
        }

        await presenter.CompleteAsync(triggerShutdown: shutdownPC);
        return !result.Canceled;
    }

    /// <summary>
    /// Runs a restore backup task to restore an entire backup.
    /// </summary>
    /// <param name="version">Specifies the version to restore.</param>
    /// <param name="file">Specifies the file to restore.</param>
    /// <param name="destination">Specifies the destination to restore the files to.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true)
    {
        _logger.Debug("Restore task for version {version} and file \"{file}\" to \"{destination}\" started.",
            version, file, destination);

        var result = await jobSessionRunner.RunSingleRestoreAsync(version, file, destination, presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "restore"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Runs a restore backup task with the specifies files and destination.
    /// </summary>
    /// <param name="version">Specifies the version to restore.</param>
    /// <param name="files">Specifies the files to restore.</param>
    /// <param name="destination">Specifies the destination to restore the files to.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true)
    {
        _logger.Debug("Restore task for version {version} and {files} files to \"{destination}\" started.",
            version, files.Count, destination);
        var result = await jobSessionRunner.RunBatchRestoreAsync(version, files, destination, presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "restore"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Runs a delete backup task to delete a single version from the backup.
    /// </summary>
    /// <param name="version">Specifies the version to delete.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task DeleteBackupAsync(string version, bool statusDialog = true)
    {
        _logger.Debug("Delete task started for version {version}.", version);

        var result = await jobSessionRunner.RunSingleDeleteAsync(version, presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "delete"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Runs a delete backup task to delete multiple version from the backup.
    /// </summary>
    /// <param name="versions">Specifies the versions to delete.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true)
    {
        _logger.Debug("Delete task started for {versions} versions.", versions.Count);
        var result = await jobSessionRunner.RunBatchDeleteAsync(versions, presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "delete"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Runs a new delete single file task which deletes files in all backups given the 
    /// corresponding filters.
    /// </summary>
    /// <param name="fileFilter">Specifies the files to delete from all backups.</param>
    /// <param name="folderFilter">Specifies the folders to delete from all backups.</param>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true)
    {
        _logger.Debug("Delete task started for file and folder filter.");

        var result = await jobSessionRunner.RunSingleDeleteSingleAsync(fileFilter, folderFilter, presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "delete"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Runs a modify backup task to edit the backup.
    /// </summary>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task ModifyBackupAsync(bool statusDialog = true)
    {
        _logger.Debug("Modify task started.");

        var result = await jobSessionRunner.RunSingleModifyAsync(presenter, statusDialog);
        if (!await HandleSessionStartAsync(result, "modify"))
        {
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Requests the password from the user by either showing a corresponding password window
    /// or returning the last used password if stored temporarily.
    /// </summary>
    /// <returns>Returns true, if the password was provided, otherwise false.</returns>
    public async Task<bool> RequestPassword()
    {
        _logger.Debug("Password requested by user.");

        if (backupService.HasPassword())
        {
            return true;
        }

        if (configurationManager.Encrypt != 1)
        {
            return true;
        }

        var storedPassword = storedPasswordAdapter.GetPassword();
        if (!string.IsNullOrEmpty(storedPassword))
        {
            backupService.SetPassword(storedPassword);
            return true;
        }

        var request = await presentationService.RequestPasswordAsync();
        while (!string.IsNullOrEmpty(request.password))
        {
            if ((Hash.GetMD5Hash(request.password) ?? string.Empty) == (configurationManager.EncryptPassMD5 ?? string.Empty))
            {
                backupService.SetPassword(request.password);

                if (request.persist)
                {
                    storedPasswordAdapter.StorePassword(request.password);
                }

                return true;
            }

            _logger.Debug("Password given by user is not correct. Request retry.");

            await presentationService.ShowMessageBoxAsync(
                "MSG_PASSWORD_WRONG_TITLE".GetLocalized(),
                "MSG_PASSWORD_WRONG_TEXT".GetLocalized(),
                null
            );
            request = await presentationService.RequestPasswordAsync();
        }

        return false;
    }

    public void Dispose()
    {
        jobRuntime.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<bool> HandleSessionStartAsync(SingleBackupSessionResult result, string operationName)
    {
        if (result.Started)
        {
            return true;
        }

        LogSingleOperationStartFailure(result.Failure, operationName);
        await presenter.CompleteAsync(honorCompletionActions: false);
        return false;
    }

    private void LogSingleOperationStartFailure(JobSessionStartFailure failure, string operationName)
    {
        switch (failure)
        {
            case JobSessionStartFailure.TaskRunning:
                _logger.Error("Another task is running, so the {operationName} task will not be started.", operationName);
                break;
            case JobSessionStartFailure.DeviceNotReady:
                _logger.Error("Device is not ready, so the {operationName} task will not be started.", operationName);
                break;
            case JobSessionStartFailure.PasswordRequired:
                _logger.Error("Password request was cancelled, so the {operationName} task will not be started.", operationName);
                break;
        }
    }
}
