// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;
public interface IStatusService
{
    JobState JobState
    {
        get;
        set;
    }
    RequestOverwriteResult LastFileOverwriteChoice
    {
        get;
    }
    string LastFileProgress
    {
        get;
        set;
    }
    Collection<FileExceptionEntry> LastFilesException
    {
        get;
        set;
    }
    int LastProgressCurrent
    {
        get;
        set;
    }
    int LastProgressTotal
    {
        get;
        set;
    }
    string LastStatusText
    {
        get;
        set;
    }
    string LastStatusTitle
    {
        get;
        set;
    }
    SystemStatus SystemStatus
    {
        get;
        set;
    }

    void AddObserver(IStatusReport jobReport, bool triggerLastState = false);
    bool IsTaskRunning();
    void RemoveObserver(IStatusReport jobReport);
    void ReportAction(ActionType action, bool silent);
    void ReportExceptions(Collection<FileExceptionEntry> files, bool silent);
    void ReportFileProgress(string file);
    void ReportProgress(int total, int current);
    void ReportState(JobState jobState);
    void ReportStatus(string title, string text);
    Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile);
    void RequestShowErrorInsufficientDiskSpace();
    void SetSystemStatus(SystemStatus status);
    void ShowExceptionDialog();

    void Initialize();
}