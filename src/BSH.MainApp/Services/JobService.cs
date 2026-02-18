// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Security;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
using BSH.MainApp.Models;
using Serilog;

namespace BSH.MainApp.Services;

public class JobService : IJobService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<JobService>();

    private readonly IBackupService backupService;

    private readonly IConfigurationManager configurationManager;
    private readonly IStatusService statusService;
    private readonly IPresentationService presentationService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly Func<IWaitForMediaService> waitForMediaServiceFactory;
    private readonly JobRuntime jobRuntime;

    private IJobReport jobReportCallback;

    private CancellationToken cancellationToken;

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
        this.localSettingsService = localSettingsService;
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

        jobReportCallback = (IJobReport)statusService;

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
    /// Prepares a backup job by setting internal states, informs all observers, and
    /// shows (if applicable) the corresponding user interfaces to the user.
    /// </summary>
    /// <param name="action">Specifies the action that is executed.</param>
    /// <param name="statusDialog">Specifies if the status window should be shown.</param>
    /// <returns></returns>
    /// <exception cref="TaskRunningException"></exception>
    /// <exception cref="DeviceNotReadyException"></exception>
    /// <exception cref="PasswordRequiredException"></exception>
    private async Task PrepareJob(ActionType action, bool statusDialog)
    {
        // show dialog?
        if (statusDialog)
        {
            await presentationService.ShowStatusWindowAsync();
        }

        cancellationToken = await jobRuntime.PrepareAsync(action, statusDialog);
    }

    /// <summary>
    /// Prepares a backup job by setting internal states, informs all observers, and
    /// shows (if applicable) the corresponding user interfaces to the user. This method
    /// also handles potential exceptions and returns false.
    /// </summary>
    /// <param name="action">Specifies the action that is executed.</param>
    /// <param name="statusDialog">Specifies if the status window should be shown.</param>
    /// <returns></returns>
    private async Task<bool> PrepareJobAndHandleExceptions(ActionType action, bool statusDialog)
    {
        try
        {
            await PrepareJob(action, statusDialog);
        }
        catch (TaskRunningException ex)
        {
            _logger.Error(ex, "Another task is running, so the backup task will not be started.");

            if (statusDialog)
            {
                await presentationService.ShowMessageBoxAsync("MSG_TASK_RUNNING_TITLE".GetLocalized(), "MSG_TASK_RUNNING_TEXT".GetLocalized(), null);
            }

            await HandleFinishedStatusDialog(statusDialog);
            return false;
        }
        catch (Exception ex) when (ex is DeviceNotReadyException || ex is DeviceContainsWrongStateException)
        {
            _logger.Error(ex, "Device is not ready, so the backup task will not be started.");

            if (statusDialog)
            {
                await presentationService.ShowMessageBoxAsync("MSG_BACKUP_DEVICE_NOT_READY_TITLE".GetLocalized(), "MSG_BACKUP_DEVICE_NOT_READY_TEXT".GetLocalized(), null);
            }

            await HandleFinishedStatusDialog(statusDialog);
            return false;
        }
        catch (PasswordRequiredException ex)
        {
            _logger.Error(ex, "Password request was cancelled, so the backup task will not be started.");
            await HandleFinishedStatusDialog(statusDialog);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures that the corresponding finishing tasks of the status windows are handled according
    /// to the user settings. If the user specifies to shutdown the computer, the computer will be
    /// shut down. If the user specifies to hibernate the computer, the computer will be put into
    /// this mode as well.
    /// </summary>
    /// <param name="statusDialog"></param>
    /// <param name="triggerAction"></param>
    private async Task HandleFinishedStatusDialog(bool statusDialog, bool triggerAction = false)
    {
        // finish job
        if (statusDialog)
        {
            var action = await presentationService.CloseStatusWindowAsync();
            if (triggerAction && action == TaskCompleteAction.ShutdownPC)
            {
                _logger.Debug("Computer will be shutdown after task has finished.");

                //Process.Start("shutdown.exe", "-s -t 60 -c \"" + Resources.TASK_BSH_SHUTDOWN_PC + "\"");
            }
            else if (triggerAction && action == TaskCompleteAction.HibernatePC)
            {
                _logger.Debug("Computer will be hibernated after task has finished.");

                //Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState");
            }
        }
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Backup, statusDialog))
        {
            return false;
        }

        // run backup job
        try
        {
            var task = backupService.StartBackup(title, description, ref jobReportCallback, cancellationToken, fullBackup, sourceFolders, !statusDialog);
            await task.ConfigureAwait(true);
        }
        catch
        {
            // exception already handled
        }

        await HandleFinishedStatusDialog(statusDialog);
        return !cancellationToken.IsCancellationRequested;
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Restore, statusDialog))
        {
            return;
        }

        // run restore job
        try
        {
            var task = backupService.StartRestore(version, file, destination, ref jobReportCallback, cancellationToken, FileOverwrite.Ask, !statusDialog);
            await task.ConfigureAwait(true);
        }
        catch
        {
            // exception already handled
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Restore, statusDialog))
        {
            return;
        }

        // run restore job
        var fileOverwrite = FileOverwrite.Ask;

        foreach (var file in files)
        {
            try
            {
                var task = backupService.StartRestore(version, file, destination, ref jobReportCallback, cancellationToken, fileOverwrite, !statusDialog);
                await task.ConfigureAwait(true);
            }
            catch
            {
                // exception already handled
            }
            finally
            {
                // persist overwrite
                if (statusService.LastFileOverwriteChoice == RequestOverwriteResult.OverwriteAll || statusService.LastFileOverwriteChoice == RequestOverwriteResult.NoOverwriteAll)
                {
                    fileOverwrite = statusService.LastFileOverwriteChoice == RequestOverwriteResult.OverwriteAll ? FileOverwrite.Overwrite : FileOverwrite.DontCopy;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Delete, statusDialog))
        {
            return;
        }

        // run delete job
        try
        {
            var task = backupService.StartDelete(version, ref jobReportCallback, cancellationToken, !statusDialog);
            await task.ConfigureAwait(true);
        }
        catch
        {
            // exception already handled
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Delete, statusDialog))
        {
            return;
        }

        // run delete job
        foreach (string version in versions)
        {
            try
            {
                var task = backupService.StartDelete(version, ref jobReportCallback, cancellationToken, !statusDialog);
                await task.ConfigureAwait(true);
            }
            catch
            {
                // exception already handled
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
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

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Delete, statusDialog))
        {
            return;
        }

        // run delete job
        try
        {
            var task = backupService.StartDeleteSingle(fileFilter, folderFilter, ref jobReportCallback, cancellationToken, !statusDialog);
            await task.ConfigureAwait(true);
        }
        catch
        {
            // exception already handled
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
    }

    /// <summary>
    /// Runs a modify backup task to edit the backup.
    /// </summary>
    /// <param name="statusDialog">Specifies if the user should be shown a status user interface.</param>
    /// <returns></returns>
    public async Task ModifyBackupAsync(bool statusDialog = true)
    {
        _logger.Debug("Modify task started.");

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Modify, statusDialog))
        {
            return;
        }

        // run modify job
        try
        {
            var task = backupService.StartEdit(ref jobReportCallback, cancellationToken, statusDialog);
            await task.ConfigureAwait(true);
        }
        catch
        {
            // exception already handled
        }

        // finish
        await HandleFinishedStatusDialog(statusDialog);
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

        // password stored
        var encryptedPassword = await localSettingsService.ReadSettingAsync<string>("BackupPassword");
        if (!string.IsNullOrEmpty(encryptedPassword))
        {
            var storedPassword = Crypto.DecryptString(encryptedPassword);
            if (storedPassword.Length > 0)
            {
                backupService.SetPassword(storedPassword);
                return true;
            }
        }

        // request password from user
        var request = await presentationService.RequestPasswordAsync();
        while (!string.IsNullOrEmpty(request.password))
        {
            if ((Hash.GetMD5Hash(request.password) ?? "") == (configurationManager.EncryptPassMD5 ?? ""))
            {
                backupService.SetPassword(request.password);

                // persist password?
                if (request.persist)
                {
                    await localSettingsService.SaveSettingAsync("BackupPassword", Crypto.EncryptString(request.password));
                }
                return true;
            }

            // report back to user
            _logger.Debug("Password given by user is not correct. Request retry.");

            await this.presentationService.ShowMessageBoxAsync(
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
}
