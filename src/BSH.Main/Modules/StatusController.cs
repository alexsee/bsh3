// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Humanizer;
using Serilog;
using Resources = BSH.Main.Properties.Resources;

namespace Brightbits.BSH.Main;

public class StatusController : IJobReport
{
    private static readonly ILogger Logger = Log.ForContext<StatusController>();
    private static StatusController _statusController;

    public static StatusController Current
    {
        get
        {
            if (_statusController == null)
            {
                _statusController = new StatusController();
            }

            return _statusController;
        }
    }

    private readonly List<IStatusReport> observers = new List<IStatusReport>();
    private readonly object observersLock = new object();

    private RequestOverwriteResult lastFileOverwriteChoice = RequestOverwriteResult.None;

    private ActionType lastActionType = ActionType.Check;

    public JobState JobState
    {
        get; set;
    }

    public SystemStatus SystemStatus
    {
        get; set;
    }

    public string LastStatusTitle
    {
        get; set;
    }

    public string LastStatusText
    {
        get; set;
    }

    public int LastProgressTotal
    {
        get; set;
    }

    public int LastProgressCurrent
    {
        get; set;
    }

    public string LastFileProgress
    {
        get; set;
    }

    public Collection<FileExceptionEntry> LastFilesException
    {
        get; set;
    }

    private StatusController()
    {
    }

    public RequestOverwriteResult LastFileOverwriteChoice
    {
        get
        {
            return lastFileOverwriteChoice;
        }
    }

    public bool IsTaskRunning()
    {
        return JobState == JobState.RUNNING;
    }

    public void SetSystemStatus(SystemStatus status)
    {
        SystemStatus = status;
        NotifyObservers(x => x.ReportSystemStatus(status));
    }

    public void ReportAction(ActionType action, bool silent)
    {
        lastFileOverwriteChoice = RequestOverwriteResult.None;
        lastActionType = action;
        NotifyObservers(x => x.ReportAction(action, silent));
    }

    public void ReportState(JobState jobState)
    {
        JobState = jobState;
        NotifyObservers(x => x.ReportState(jobState));

        // finished successfully
        if (jobState == JobState.FINISHED && lastActionType == ActionType.Backup && BackupLogic.ConfigurationManager.InfoBackupDone == "1")
        {
            NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_SUCCESSFUL_TITLE, Resources.INFO_BACKUP_SUCCESSFUL_TEXT, ToolTipIcon.Info);
        }

        if (jobState == JobState.ERROR && lastActionType == ActionType.Backup && BackupLogic.ConfigurationManager.InfoBackupDone == "1")
        {
            NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_UNSUCCESSFUL_TITLE, Resources.INFO_BACKUP_UNSUCCESSFUL_TEXT, ToolTipIcon.Warning);
        }
    }

    public void ReportStatus(string title, string text)
    {
        LastStatusTitle = title;
        LastStatusText = text;
        NotifyObservers(x => x.ReportStatus(title, text));
    }

    public void ReportProgress(int total, int current)
    {
        LastProgressTotal = total;
        LastProgressCurrent = current;
        NotifyObservers(x => x.ReportProgress(total, current));
    }

    public void ReportFileProgress(string file)
    {
        LastFileProgress = file;
        NotifyObservers(x => x.ReportFileProgress(file));
    }

    public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
    {
        LastFilesException = files;
        if (files.Count == 0 || silent)
        {
            return;
        }

        ShowExceptionDialog();
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

        // cancel
        if (dlgFilesOverwrite.ShowDialog() == DialogResult.Cancel)
        {
            BackupLogic.BackupController.Cancel();
            return RequestOverwriteResult.NoOverwrite;
        }

        // overwrite
        if (dlgFilesOverwrite.DialogResult == DialogResult.OK)
        {
            if (dlgFilesOverwrite.chkAllConflicts.Checked)
            {
                lastFileOverwriteChoice = RequestOverwriteResult.OverwriteAll;
                return RequestOverwriteResult.OverwriteAll;
            }

            return RequestOverwriteResult.Overwrite;
        }

        // ignore
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

    public void AddObserver(IStatusReport jobReport, bool triggerLastState = false)
    {
        ArgumentNullException.ThrowIfNull(jobReport);

        lock (observersLock)
        {
            observers.Add(jobReport);
        }

        if (triggerLastState)
        {
            try
            {
                jobReport.ReportSystemStatus(SystemStatus);
                jobReport.ReportState(JobState);

                if (JobState == JobState.RUNNING)
                {
                    jobReport.ReportStatus(LastStatusTitle, LastStatusText);
                    jobReport.ReportProgress(LastProgressTotal, LastProgressCurrent);
                    jobReport.ReportFileProgress(LastFileProgress);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Status observer replay failed.");
            }
        }
    }

    public void RemoveObserver(IStatusReport jobReport)
    {
        ArgumentNullException.ThrowIfNull(jobReport);

        lock (observersLock)
        {
            observers.Remove(jobReport);
        }
    }

    private void NotifyObservers(Action<IStatusReport> notify)
    {
        IStatusReport[] snapshot;
        lock (observersLock)
        {
            snapshot = observers.ToArray();
        }

        foreach (var observer in snapshot)
        {
            try
            {
                notify(observer);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Status observer notification failed.");
            }
        }
    }

    public void ShowExceptionDialog()
    {
        if (LastFilesException.Count == 0)
        {
            return;
        }

        // files with exceptions
        using var dlgFileNotCopied = new frmFileNotCopied();
        foreach (var entry in LastFilesException)
        {
            var innerException = entry.Exception.Message.ToString();

            // show inner exception if file not processed exception
            if (entry.Exception.GetType() == typeof(FileNotProcessedException) &&
                entry.Exception.InnerException != null)
            {
                innerException = entry.Exception.InnerException.Message.ToString();
            }

            var newEntry = new ListViewItem();
            newEntry.Text = entry.File.FileNamePath();
            newEntry.SubItems.Add(innerException);
            newEntry.Tag = entry;
            dlgFileNotCopied.lvFiles.Items.Add(newEntry);
        }

        dlgFileNotCopied.ShowDialog();
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        PresentationController.ShowErrorInsufficientDiskSpace();
    }
}