// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;
public class StatusService : IJobReport, IStatusService
{
    private readonly List<IStatusReport> observers = new();
    private readonly IConfigurationManager configurationManager;
    private readonly IPresentationService presentationService;
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

    public RequestOverwriteResult LastFileOverwriteChoice => lastFileOverwriteChoice;

    public StatusService(IConfigurationManager configurationManager, IPresentationService presentationService)
    {
        this.configurationManager = configurationManager;
        this.presentationService = presentationService;
    }

    public void Initialize()
    {
        if (configurationManager.IsConfigured == "0")
        {
            SetSystemStatus(SystemStatus.NOT_CONFIGURED);
            return;
        }
    }

    public bool IsTaskRunning()
    {
        return JobState == JobState.RUNNING;
    }

    public void SetSystemStatus(SystemStatus status)
    {
        SystemStatus = status;
        observers.ForEach(x => x.ReportSystemStatus(status));
    }

    public void ReportAction(ActionType action, bool silent)
    {
        lastFileOverwriteChoice = RequestOverwriteResult.None;
        lastActionType = action;
        observers.ForEach(x => x.ReportAction(action, silent));
    }

    public void ReportState(JobState jobState)
    {
        JobState = jobState;
        foreach (IStatusReport x in observers)
        {
            x.ReportState(jobState);
        }

        //// finished successfully
        //if (jobState == JobState.FINISHED && lastActionType == ActionType.Backup && configurationManager.InfoBackupDone == "1")
        //{
        //    NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_SUCCESSFUL_TITLE, Resources.INFO_BACKUP_SUCCESSFUL_TEXT, ToolTipIcon.Info);
        //}

        //if (jobState == JobState.ERROR && lastActionType == ActionType.Backup && configurationManager.InfoBackupDone == "1")
        //{
        //    NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_UNSUCCESSFUL_TITLE, Resources.INFO_BACKUP_UNSUCCESSFUL_TEXT, ToolTipIcon.Warning);
        //}
    }

    public void ReportStatus(string title, string text)
    {
        LastStatusTitle = title;
        LastStatusText = text;
        observers.ForEach(x => x.ReportStatus(title, text));
    }

    public void ReportProgress(int total, int current)
    {
        LastProgressTotal = total;
        LastProgressCurrent = current;
        observers.ForEach(x => x.ReportProgress(total, current));
    }

    public void ReportFileProgress(string file)
    {
        LastFileProgress = file;
        observers.ForEach(x => x.ReportFileProgress(file));
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

    public RequestOverwriteResult RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
    {
        if (lastFileOverwriteChoice != RequestOverwriteResult.None)
        {
            return lastFileOverwriteChoice;
        }

        //using var dlgFilesOverwrite = new frmFileOverrides();
        //dlgFilesOverwrite.lblFileName1.Text = remoteFile.FileName;
        //dlgFilesOverwrite.lblFileName2.Text = localFile.FileName;
        //dlgFilesOverwrite.lblFileDateChanged1.Text = Resources.LBL_CHANGE_DATE + remoteFile.FileDateModified.ToString();
        //dlgFilesOverwrite.lblFileDateChanged2.Text = Resources.LBL_CHANGE_DATE + localFile.FileDateModified.ToString();
        //dlgFilesOverwrite.lblFileSize1.Text = Resources.LBL_SIZE + remoteFile.FileSize.Bytes().Humanize();
        //dlgFilesOverwrite.lblFileSize2.Text = Resources.LBL_SIZE + localFile.FileSize.Bytes().Humanize();
        //if (!localFile.FilePath.StartsWith(@"\\"))
        //{
        //    dlgFilesOverwrite.picIco1.Image = Icon.ExtractAssociatedIcon(localFile.FilePath + localFile.FileName).ToBitmap();
        //}

        //dlgFilesOverwrite.picIco2.Image = dlgFilesOverwrite.picIco1.Image;

        //// cancel
        //if (dlgFilesOverwrite.ShowDialog() == DialogResult.Cancel)
        //{
        //    BackupLogic.BackupController.Cancel();
        //    return RequestOverwriteResult.NoOverwrite;
        //}

        //// overwrite
        //if (dlgFilesOverwrite.DialogResult == DialogResult.OK)
        //{
        //    if (dlgFilesOverwrite.chkAllConflicts.Checked)
        //    {
        //        lastFileOverwriteChoice = RequestOverwriteResult.OverwriteAll;
        //        return RequestOverwriteResult.OverwriteAll;
        //    }

        //    return RequestOverwriteResult.Overwrite;
        //}

        //// ignore
        //if (dlgFilesOverwrite.DialogResult == DialogResult.Ignore)
        //{
        //    if (dlgFilesOverwrite.chkAllConflicts.Checked)
        //    {
        //        lastFileOverwriteChoice = RequestOverwriteResult.NoOverwriteAll;
        //        return RequestOverwriteResult.NoOverwriteAll;
        //    }

        //    return RequestOverwriteResult.NoOverwrite;
        //}

        return RequestOverwriteResult.NoOverwrite;
    }

    public void AddObserver(IStatusReport jobReport, bool triggerLastState = false)
    {
        observers.Add(jobReport);
    }

    public void RemoveObserver(IStatusReport jobReport)
    {
        observers.Remove(jobReport);
    }

    public void ShowExceptionDialog()
    {
        if (LastFilesException.Count == 0)
        {
            return;
        }

        //// files with exceptions
        //using var dlgFileNotCopied = new frmFileNotCopied();
        //foreach (var entry in LastFilesException)
        //{
        //    var innerException = entry.Exception.Message.ToString();

        //    // show inner exception if file not processed exception
        //    if (entry.Exception.GetType() == typeof(FileNotProcessedException) &&
        //        entry.Exception.InnerException != null)
        //    {
        //        innerException = entry.Exception.InnerException.Message.ToString();
        //    }

        //    var newEntry = new ListViewItem();
        //    newEntry.Text = entry.File.FileNamePath();
        //    newEntry.SubItems.Add(innerException);
        //    newEntry.Tag = entry;
        //    dlgFileNotCopied.lvFiles.Items.Add(newEntry);
        //}

        //dlgFileNotCopied.ShowDialog();
    }

    public void RequestShowErrorInsufficientDiskSpace()
    {
        this.presentationService.ShowErrorInsufficientDiskSpace();
    }
}
