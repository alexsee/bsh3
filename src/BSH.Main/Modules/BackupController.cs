﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
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
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Security;
using BSH.Main.Properties;
using Serilog;

namespace Brightbits.BSH.Main;

/// <summary>
/// The BackupController is responsible for orchestrating all backup tasks related to
/// the engine. It maintains an internal state of the tasks and allows external services
/// to start and cancel tasks. External services can also subscribe to the state and 
/// status of the tasks being executed.
/// </summary>
public class BackupController
{
    private readonly ILogger _logger = Log.ForContext<BackupController>();

    private readonly IBackupService backupService;

    private readonly IConfigurationManager configurationManager;

    private IJobReport jobReportCallback;

    private CancellationTokenSource cancellationTokenSource;

    private CancellationToken cancellationToken;

    /// <summary>
    /// Initializes a new instance of the BackupController given the BackupService and
    /// ConfigurationManager instance.
    /// </summary>
    /// <param name="backupService"></param>
    /// <param name="configurationManager"></param>
    public BackupController(IBackupService backupService, IConfigurationManager configurationManager)
    {
        this.backupService = backupService;
        this.configurationManager = configurationManager;

        jobReportCallback = StatusController.Current;

        GetNewCancellationToken();
    }

    /// <summary>
    /// Creates a new CancellationToken and assigns it to the current internal state.
    /// </summary>
    /// <returns>Returns the new CancellationToken.</returns>
    public CancellationToken GetNewCancellationToken()
    {
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;
        return cancellationToken;
    }

    /// <summary>
    /// Cancels the current executed task.
    /// </summary>
    public void Cancel()
    {
        _logger.Debug("Cancellation of current task requested by user.");

        if (cancellationTokenSource == null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Checks if the currently selected backup media is available.
    /// </summary>
    /// <param name="action">Specifies the action that should be checked.</param>
    /// <param name="silent">Specifies if no user visualization should be shown.</param>
    /// <returns>Returns true, if the backup media is available, otherwise false.</returns>
    public async Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
    {
        _logger.Debug("Media check requested by task {action}.", action);

        // check if media is available
        if (await backupService.CheckMedia())
        {
            return true;
        }

        // should we wait for media
        if (configurationManager.Medium == "1" && !silent)
        {
            var waitForMediaService = new WaitForMediaService(backupService, silent, action, cancellationTokenSource);
            return await waitForMediaService.ExecuteAsync();
        }

        return false;
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
        GetNewCancellationToken();

        // other task running?
        if (StatusController.Current.IsTaskRunning())
        {
            throw new TaskRunningException();
        }

        // show dialog?
        if (statusDialog)
        {
            PresentationController.Current.ShowStatusWindow();
        }

        // check media
        if (!await CheckMediaAsync(action, !statusDialog))
        {
            throw new DeviceNotReadyException();
        }

        // check password
        if (!RequestPassword())
        {
            throw new PasswordRequiredException();
        }
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
                MessageBox.Show(Resources.MSG_TASK_RUNNING_TEXT, Resources.MSG_TASK_RUNNING_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            HandleFinishedStatusDialog(statusDialog);
            return false;
        }
        catch (Exception ex) when (ex is DeviceNotReadyException || ex is DeviceContainsWrongStateException)
        {
            _logger.Error(ex, "Device is not ready, so the backup task will not be started.");

            if (statusDialog)
            {
                MessageBox.Show(Resources.MSG_BACKUP_DEVICE_NOT_READY_TEXT, Resources.MSG_BACKUP_DEVICE_NOT_READY_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            HandleFinishedStatusDialog(statusDialog);
            return false;
        }
        catch (PasswordRequiredException ex)
        {
            _logger.Error(ex, "Password request was cancelled, so the backup task will not be started.");
            HandleFinishedStatusDialog(statusDialog);
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
    private void HandleFinishedStatusDialog(bool statusDialog, bool triggerAction = false)
    {
        // finish job
        if (statusDialog)
        {
            var action = PresentationController.Current.CloseStatusWindow();
            if (triggerAction && action == TaskCompleteAction.ShutdownPC)
            {
                _logger.Debug("Computer will be shutdown after task has finished.");

                Process.Start("shutdown.exe", "-s -t 60 -c \"" + Resources.TASK_BSH_SHUTDOWN_PC + "\"");
            }
            else if (triggerAction && action == TaskCompleteAction.HibernatePC)
            {
                _logger.Debug("Computer will be hibernated after task has finished.");

                Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState");
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
    public async Task CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "")
    {
        _logger.Debug("Backup task is started with title: {title}, description: {description}, statusDialog: {statusDialog}, fullBackup: {fullBackup}",
            title, description, statusDialog, fullBackup);

        // check job requirements
        if (!await PrepareJobAndHandleExceptions(ActionType.Backup, statusDialog))
        {
            return;
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

        HandleFinishedStatusDialog(statusDialog);

        // shutdown pc?
        if (shutdownPC)
        {
            Process.Start("shutdown.exe", "-s -t 60 -c \"" + Resources.TASK_BSH_SHUTDOWN_PC + "\"");
        }

        // shutdown app?
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
        HandleFinishedStatusDialog(statusDialog);
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

        jobReportCallback.ReportAction(ActionType.Restore, !statusDialog);
        jobReportCallback.ReportState(JobState.RUNNING);

        IJobReport forwardJobReport = new ForwardJobReport(jobReportCallback);

        foreach (var file in files)
        {
            try
            {
                jobReportCallback.ReportProgress(files.Count, files.IndexOf(file) + 1);

                // restore file
                var task = backupService.StartRestore(version, file, destination, ref forwardJobReport, cancellationToken, fileOverwrite, !statusDialog);
                await task.ConfigureAwait(true);
            }
            catch
            {
                // exception already handled
            }
            finally
            {
                // persist overwrite
                if (StatusController.Current.LastFileOverwriteChoice == RequestOverwriteResult.OverwriteAll || StatusController.Current.LastFileOverwriteChoice == RequestOverwriteResult.NoOverwriteAll)
                {
                    fileOverwrite = StatusController.Current.LastFileOverwriteChoice == RequestOverwriteResult.OverwriteAll ? FileOverwrite.Overwrite : FileOverwrite.DontCopy;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        ((ForwardJobReport)forwardJobReport).ForwardExceptions(!statusDialog);
        jobReportCallback.ReportState(JobState.FINISHED);

        // finish
        HandleFinishedStatusDialog(statusDialog);
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
        HandleFinishedStatusDialog(statusDialog);
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
        jobReportCallback.ReportAction(ActionType.Delete, !statusDialog);
        jobReportCallback.ReportState(JobState.RUNNING);

        IJobReport forwardJobReport = new ForwardJobReport(jobReportCallback);

        foreach (var version in versions)
        {
            try
            {
                var task = backupService.StartDelete(version, ref forwardJobReport, cancellationToken, !statusDialog);
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

        ((ForwardJobReport)forwardJobReport).ForwardExceptions(!statusDialog);
        jobReportCallback.ReportState(JobState.FINISHED);

        // finish
        HandleFinishedStatusDialog(statusDialog);
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
        HandleFinishedStatusDialog(statusDialog);
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

        // password stored
        if (!string.IsNullOrEmpty(Settings.Default.BackupPwd))
        {
            var storedPassword = Crypto.DecryptString(Settings.Default.BackupPwd);
            if (storedPassword.Length > 0)
            {
                backupService.SetPassword(storedPassword);
                return true;
            }
        }

        // request password from user
        var request = PresentationController.Current.RequestPassword();
        while (!string.IsNullOrEmpty(request.password))
        {
            if ((Hash.GetMD5Hash(request.password) ?? "") == (configurationManager.EncryptPassMD5 ?? ""))
            {
                backupService.SetPassword(request.password);

                // persist password?
                if (request.persist)
                {
                    Settings.Default.BackupPwd = Crypto.EncryptString(request.password);
                    Settings.Default.Save();
                }

                return true;
            }

            // report back to user
            _logger.Debug("Password given by user is not correct. Request retry.");

            MessageBox.Show(Resources.MSG_PASSWORD_WRONG_TEXT, Resources.MSG_PASSWORD_WRONG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            request = PresentationController.Current.RequestPassword();
        }

        return false;
    }
}