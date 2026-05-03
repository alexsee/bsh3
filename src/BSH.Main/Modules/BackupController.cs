// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Security;
using Serilog;
using Resources = BSH.Main.Properties.Resources;

namespace Brightbits.BSH.Main;

/// <summary>
/// The BackupController is responsible for orchestrating all backup tasks related to
/// the engine. It maintains an internal state of the tasks and allows external services
/// to start and cancel tasks. External services can also subscribe to the state and 
/// status of the tasks being executed.
/// </summary>
public class BackupController : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<BackupController>();

    private readonly IBackupService backupService;

    private readonly IConfigurationManager configurationManager;

    private readonly JobRuntime jobRuntime;
    private readonly JobSessionRunner jobSessionRunner;
    private readonly IJobSessionPresenter presenter;
    private readonly IStoredPasswordAdapter storedPasswordAdapter;

    private CancellationToken cancellationToken;

    /// <summary>
    /// Initializes a new instance of the BackupController given the BackupService and
    /// ConfigurationManager instance.
    /// </summary>
    /// <param name="backupService"></param>
    /// <param name="configurationManager"></param>
    /// <param name="isTaskRunning"></param>
    /// <param name="waitForMediaAsync"></param>
    /// <param name="requestPasswordAsync"></param>
    public BackupController(
        IBackupService backupService,
        IConfigurationManager configurationManager,
        Func<bool> isTaskRunning,
        Func<ActionType, bool, CancellationTokenSource, Task<bool>> waitForMediaAsync,
        Func<Task<bool>> requestPasswordAsync)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(configurationManager);
        ArgumentNullException.ThrowIfNull(isTaskRunning);
        ArgumentNullException.ThrowIfNull(waitForMediaAsync);
        ArgumentNullException.ThrowIfNull(requestPasswordAsync);

        this.backupService = backupService;
        this.configurationManager = configurationManager;
        this.jobRuntime = new JobRuntime(
            backupService,
            isTaskRunning,
            () => this.configurationManager.Medium == "1",
            waitForMediaAsync,
            requestPasswordAsync);
        this.presenter = new WinFormsJobSessionPresenter(backupService, configurationManager);
        this.storedPasswordAdapter = new WinFormsStoredPasswordAdapter();
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
        cancellationToken = jobRuntime.GetNewCancellationToken();
        return cancellationToken;
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the backup task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the backup task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the backup task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
            return false;
        }

        await presenter.CompleteAsync(triggerShutdown: shutdownPC);

        if (shutdownApp)
        {
            NotificationController.Current.Shutdown();
            BackupLogic.StopSystem();
            try
            {
                Application.Exit();
                Environment.Exit(0);
            }
            catch
            {
                Environment.Exit(0);
            }
        }

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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the restore task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the restore task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the restore task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the restore task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the restore task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the restore task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the delete task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the delete task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the delete task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the delete task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
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

        if (!result.Started)
        {
            switch (result.Failure)
            {
                case JobSessionStartFailure.TaskRunning:
                    _logger.Error("Another task is running, so the modify task will not be started.");
                    break;
                case JobSessionStartFailure.DeviceNotReady:
                    _logger.Error("Device is not ready, so the modify task will not be started.");
                    break;
                case JobSessionStartFailure.PasswordRequired:
                    _logger.Error("Password request was cancelled, so the modify task will not be started.");
                    break;
            }

            await presenter.CompleteAsync(honorCompletionActions: false);
            return;
        }

        await presenter.CompleteAsync();
    }

    /// <summary>
    /// Requests the password from the user by either showing a corresponding password window
    /// or returning the last used password if stored temporarily.
    /// </summary>
    /// <returns>Returns true, if the password was provided, otherwise false.</returns>
    public bool RequestPassword()
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

        var request = presenter.RequestPasswordAsync().GetAwaiter().GetResult();
        while (!string.IsNullOrEmpty(request.Password))
        {
            if ((Hash.GetMD5Hash(request.Password) ?? string.Empty) == (configurationManager.EncryptPassMD5 ?? string.Empty))
            {
                backupService.SetPassword(request.Password);

                if (request.Persist)
                {
                    storedPasswordAdapter.StorePassword(request.Password);
                }

                return true;
            }

            _logger.Debug("Password given by user is not correct. Request retry.");

            presenter.ShowErrorPasswordWrongAsync().GetAwaiter().GetResult();
            request = presenter.RequestPasswordAsync().GetAwaiter().GetResult();
        }

        return false;
    }

    public void Dispose()
    {
        jobRuntime.Dispose();
        GC.SuppressFinalize(this);
    }
}
