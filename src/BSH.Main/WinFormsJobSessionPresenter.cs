// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Resources = BSH.Main.Properties.Resources;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;
using Humanizer;
using Serilog;

namespace Brightbits.BSH.Main;

/// <summary>
/// WinForms implementation of <see cref="IJobSessionPresenter"/> using existing controllers
/// for status, presentation, and user interaction.
/// </summary>
public class WinFormsJobSessionPresenter : IJobSessionPresenter
{
    private readonly ILogger _logger = Log.ForContext<WinFormsJobSessionPresenter>();

    private readonly IBackupService backupService;
    private readonly IConfigurationManager configurationManager;
    private CancellationToken cancellationToken;
    private ActionType lastActionType = ActionType.Check;
    private RequestOverwriteResult lastFileOverwriteChoice = RequestOverwriteResult.None;

    public WinFormsJobSessionPresenter(IBackupService backupService, IConfigurationManager configurationManager)
    {
        ArgumentNullException.ThrowIfNull(backupService);
        ArgumentNullException.ThrowIfNull(configurationManager);

        this.backupService = backupService;
        this.configurationManager = configurationManager;
    }

    public Task ShowStatusWindowAsync()
    {
        PresentationController.Current.ShowStatusWindow();
        return Task.CompletedTask;
    }

    public async Task CompleteAsync(bool triggerShutdown = false, bool triggerHibernate = false, bool honorCompletionActions = true)
    {
        var action = PresentationController.Current.CloseStatusWindow();

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
        return lastFileOverwriteChoice switch
        {
            RequestOverwriteResult.OverwriteAll => FileOverwrite.Overwrite,
            RequestOverwriteResult.NoOverwriteAll => FileOverwrite.DontCopy,
            _ => currentOverwrite
        };
    }

    private bool IsTaskRunning() => StatusController.Current.IsTaskRunning();

    public void ReportAction(ActionType action, bool silent)
    {
        lastFileOverwriteChoice = RequestOverwriteResult.None;
        lastActionType = action;
        StatusController.Current.ReportAction(action, silent);
    }

    public void ReportState(JobState jobState)
    {
        StatusController.Current.ReportState(jobState);

        if (jobState == JobState.FINISHED && lastActionType == ActionType.Backup && configurationManager.InfoBackupDone == "1")
        {
            NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_SUCCESSFUL_TITLE, Resources.INFO_BACKUP_SUCCESSFUL_TEXT, ToolTipIcon.Info);
        }

        if (jobState == JobState.ERROR && lastActionType == ActionType.Backup && configurationManager.InfoBackupDone == "1")
        {
            NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_UNSUCCESSFUL_TITLE, Resources.INFO_BACKUP_UNSUCCESSFUL_TEXT, ToolTipIcon.Warning);
        }
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

    public async Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        if (lastFileOverwriteChoice != RequestOverwriteResult.None)
        {
            return lastFileOverwriteChoice;
        }

        using var dlgFilesOverwrite = new frmFileOverrides();
        dlgFilesOverwrite.lblFileName1.Text = remoteFile.FileName;
        dlgFilesOverwrite.lblFileName2.Text = localFile.FileName;
        dlgFilesOverwrite.lblFileDateChanged1.Text = Resources.LBL_CHANGE_DATE + remoteFile.FileDateModified.ToString();
        dlgFilesOverwrite.lblFileDateChanged2.Text = Resources.LBL_CHANGE_DATE + localFile.FileDateModified.ToString();
        dlgFilesOverwrite.lblFileSize1.Text = Resources.LBL_SIZE + remoteFile.FileSize.Bytes().Humanize();
        dlgFilesOverwrite.lblFileSize2.Text = Resources.LBL_SIZE + localFile.FileSize.Bytes().Humanize();
        if (!localFile.FilePath.StartsWith(@"\\"))
        {
            dlgFilesOverwrite.picIco1.Image = Icon.ExtractAssociatedIcon(localFile.FilePath + localFile.FileName).ToBitmap();
        }

        dlgFilesOverwrite.picIco2.Image = dlgFilesOverwrite.picIco1.Image;

        if (dlgFilesOverwrite.ShowDialog() == DialogResult.Cancel)
        {
            await CancelAsync();
            return RequestOverwriteResult.NoOverwrite;
        }

        if (dlgFilesOverwrite.DialogResult == DialogResult.OK)
        {
            if (dlgFilesOverwrite.chkAllConflicts.Checked)
            {
                lastFileOverwriteChoice = RequestOverwriteResult.OverwriteAll;
                return RequestOverwriteResult.OverwriteAll;
            }

            return RequestOverwriteResult.Overwrite;
        }

        if (dlgFilesOverwrite.DialogResult == DialogResult.Ignore)
        {
            if (dlgFilesOverwrite.chkAllConflicts.Checked)
            {
                lastFileOverwriteChoice = RequestOverwriteResult.NoOverwriteAll;
                return RequestOverwriteResult.NoOverwriteAll;
            }

            return RequestOverwriteResult.NoOverwrite;
        }

        return RequestOverwriteResult.NoOverwrite;
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        await Task.Run(() => PresentationController.ShowErrorInsufficientDiskSpace());
    }
}
