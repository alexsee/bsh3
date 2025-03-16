// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;

namespace BSH.MainApp.ViewModels.Windows;
public partial class StatusViewModel : ObservableObject, IStatusReport
{
    private readonly DispatcherQueue dispatcherQueue;

    [ObservableProperty]
    private string statusTitle = "";

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    private string currentFileText = "";

    [ObservableProperty]
    private int totalProgress = 100;

    [ObservableProperty]
    private int currentProgress = 0;

    public StatusViewModel()
    {
        this.dispatcherQueue = App.GetService<DispatcherQueue>();
    }

    public void ReportAction(ActionType action, bool silent)
    {
        // not used
    }

    public void ReportState(JobState jobState)
    {
        // not used
    }
    public void ReportSystemStatus(SystemStatus systemStatus)
    {
        // not used
    }

    public void ReportFileProgress(string file)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            CurrentFileText = file;
        });
    }

    public void ReportProgress(int total, int current)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            TotalProgress = total;
            CurrentProgress = current;
        });
    }

    public void ReportStatus(string title, string text)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            StatusTitle = title;
            StatusText = text;
        });
    }

    [RelayCommand]
    public void Cancel()
    {
        App.GetService<IJobService>().Cancel();
    }
}
