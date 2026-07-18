// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Resources = BSH.Main.Properties.Resources;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;
using Serilog;

namespace Brightbits.BSH.Main;

/// <summary>
/// WinForms implementation of <see cref="IJobSessionPresenter"/> using existing controllers
/// for status, presentation, and user interaction.
/// </summary>
public class WinFormsJobSessionPresenter : IJobSessionPresenter
{
    private readonly ILogger _logger = Log.ForContext<WinFormsJobSessionPresenter>();

    private CancellationToken cancellationToken;
    private bool statusWindowShown;

    public Task ShowStatusWindowAsync()
    {
        PresentationController.Current.ShowStatusWindow();
        statusWindowShown = true;
        return Task.CompletedTask;
    }

    public Task CompleteAsync(bool triggerShutdown = false, bool triggerHibernate = false, bool honorCompletionActions = true)
    {
        var action = TaskCompleteAction.NoAction;
        if (statusWindowShown)
        {
            action = PresentationController.Current.CloseStatusWindow();
            statusWindowShown = false;
        }

        var executeShutdown = triggerShutdown || (honorCompletionActions && action == TaskCompleteAction.ShutdownPC);
        var executeHibernate = triggerHibernate || (honorCompletionActions && action == TaskCompleteAction.HibernatePC);

        if (executeShutdown)
        {
            _logger.Debug("Computer will be shutdown after task has finished.");
            Process.Start("shutdown.exe", "-s -t 60 -c \"" + Resources.TASK_BSH_SHUTDOWN_PC + "\"");
        }
        else if (executeHibernate)
        {
            _logger.Debug("Computer will be hibernated after task has finished.");
            Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState");
        }

        return Task.CompletedTask;
    }

    public Task ShowErrorTaskRunningAsync()
    {
        MessageBox.Show(Resources.MSG_TASK_RUNNING_TEXT, Resources.MSG_TASK_RUNNING_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return Task.CompletedTask;
    }

    public Task ShowErrorDeviceNotReadyAsync()
    {
        MessageBox.Show(Resources.MSG_BACKUP_DEVICE_NOT_READY_TEXT, Resources.MSG_BACKUP_DEVICE_NOT_READY_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return Task.CompletedTask;
    }

    public Task ShowErrorPasswordRequiredAsync()
    {
        return Task.CompletedTask;
    }

    public Task<JobSessionPasswordRequest> RequestPasswordAsync()
    {
        var request = PresentationController.Current.RequestPassword();
        return Task.FromResult(new JobSessionPasswordRequest(request.password, request.persist));
    }

    public Task ShowErrorPasswordWrongAsync()
    {
        MessageBox.Show(Resources.MSG_PASSWORD_WRONG_TEXT, Resources.MSG_PASSWORD_WRONG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        return Task.CompletedTask;
    }

    public Task CancelAsync()
    {
        BackupLogic.BackupController.Cancel();
        return Task.CompletedTask;
    }

    public CancellationToken GetCancellationToken()
    {
        return cancellationToken;
    }

    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        this.cancellationToken = cancellationToken;
    }

    public FileOverwrite ResolveBatchOverwriteChoice(FileOverwrite currentOverwrite)
    {
        return StatusController.Current.LastFileOverwriteChoice switch
        {
            RequestOverwriteResult.OverwriteAll => FileOverwrite.Overwrite,
            RequestOverwriteResult.NoOverwriteAll => FileOverwrite.DontCopy,
            _ => currentOverwrite
        };
    }

    public void ReportAction(ActionType action, bool silent)
    {
        StatusController.Current.ReportAction(action, silent);
    }

    public void ReportState(JobState jobState)
    {
        StatusController.Current.ReportState(jobState);
    }

    public void ReportStatus(string title, string text)
    {
        StatusController.Current.ReportStatus(title, text);
    }

    public void ReportProgress(int total, int current)
    {
        StatusController.Current.ReportProgress(total, current);
    }

    public void ReportFileProgress(string file)
    {
        StatusController.Current.ReportFileProgress(file);
    }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
    {
        StatusController.Current.ReportExceptions(files, silent);
    }

    public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        return StatusController.Current.RequestOverwrite(localFile, remoteFile);
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        await Task.Run(() => PresentationController.ShowErrorInsufficientDiskSpace());
    }
}
