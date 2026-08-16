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
using CommunityToolkit.WinUI;
using Serilog;

namespace BSH.MainApp.Services;

public class StatusService : IJobReport, IStatusService
{
    private static readonly ILogger Logger = Log.ForContext<StatusService>();

    private readonly List<IStatusReport> observers = new();
    private readonly object observersLock = new();
    private readonly IConfigurationManager configurationManager;
    private readonly IPresentationService presentationService;
    private readonly IAppNotificationService? appNotificationService;
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
    } = string.Empty;

    public string LastStatusText
    {
        get; set;
    } = string.Empty;

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
    } = string.Empty;

    public Collection<FileExceptionEntry> LastFilesException
    {
        get; set;
    } = [];

    public RequestOverwriteResult LastFileOverwriteChoice => lastFileOverwriteChoice;

    public StatusService(
        IConfigurationManager configurationManager,
        IPresentationService presentationService,
        IAppNotificationService? appNotificationService = null)
    {
        this.configurationManager = configurationManager;
        this.presentationService = presentationService;
        this.appNotificationService = appNotificationService;
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

        if (lastActionType != ActionType.Backup || configurationManager.InfoBackupDone != "1")
        {
            return;
        }

        if (jobState == JobState.FINISHED)
        {
            ShowNotification(
                "INFO_BACKUP_SUCCESSFUL_TITLE".GetLocalized(),
                "INFO_BACKUP_SUCCESSFUL_TEXT".GetLocalized(),
                ToastNotificationActivation.ActionOverview);
        }
        else if (jobState == JobState.ERROR)
        {
            ShowNotification(
                "INFO_BACKUP_UNSUCCESSFUL_TITLE".GetLocalized(),
                "INFO_BACKUP_UNSUCCESSFUL_TEXT".GetLocalized(),
                ToastNotificationActivation.ActionBackupResult);
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

        var result = await this.presentationService.RequestOverwriteAsync(localFile, remoteFile);
        if (result == RequestOverwriteResult.OverwriteAll || result == RequestOverwriteResult.NoOverwriteAll)
        {
            lastFileOverwriteChoice = result;
        }
        return result;
    }

    public void AddObserver(IStatusReport jobReport, bool triggerLastState = false)
    {
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
        if (LastFilesException == null || LastFilesException.Count == 0)
        {
            return;
        }

        _ = presentationService.ShowFileExceptionsAsync(LastFilesException);
    }

    public async Task RequestShowErrorInsufficientDiskSpaceAsync()
    {
        await this.presentationService.ShowErrorInsufficientDiskSpaceAsync();
    }

    private void ShowNotification(string title, string text, string action)
    {
        appNotificationService?.Show(ToastNotificationPayload.Create(title, text, action));
    }
}
