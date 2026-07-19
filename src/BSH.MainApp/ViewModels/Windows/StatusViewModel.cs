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
    private static readonly TimeSpan UiRefreshInterval = TimeSpan.FromMilliseconds(100);

    private readonly DispatcherQueue? dispatcherQueue;
    private DateTime lastProgressRefresh = DateTime.MinValue;
    private DateTime lastFileRefresh = DateTime.MinValue;

    [ObservableProperty]
    private string statusTitle = "";

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    private string currentFileText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyPropertyChangedFor(nameof(ProgressMaximum))]
    [NotifyPropertyChangedFor(nameof(ProgressFilesText))]
    private int totalProgress = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressFilesText))]
    private int currentProgress = 0;

    [ObservableProperty]
    private int selectedCompletionActionIndex = 0;

    public bool IsProgressIndeterminate => TotalProgress <= 0;

    public int ProgressMaximum => TotalProgress > 0 ? TotalProgress : 1;

    public string ProgressFilesText => TotalProgress > 0
        ? $"{CurrentProgress} of {TotalProgress} files"
        : "Preparing…";

    public TaskCompleteAction SelectedCompletionAction => SelectedCompletionActionIndex switch
    {
        1 => TaskCompleteAction.ShutdownPC,
        2 => TaskCompleteAction.HibernatePC,
        _ => TaskCompleteAction.NoAction
    };

    public StatusViewModel()
        : this(App.GetService<DispatcherQueue>())
    {
    }

    public StatusViewModel(DispatcherQueue? dispatcherQueue)
    {
        this.dispatcherQueue = dispatcherQueue;
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
        if (DateTime.Now - lastFileRefresh < UiRefreshInterval)
        {
            return;
        }

        lastFileRefresh = DateTime.Now;
        UpdateOnUiThread(() =>
        {
            CurrentFileText = file;
        });
    }

    public void ReportProgress(int total, int current)
    {
        if (DateTime.Now - lastProgressRefresh < UiRefreshInterval)
        {
            return;
        }

        lastProgressRefresh = DateTime.Now;
        UpdateOnUiThread(() =>
        {
            TotalProgress = total;
            CurrentProgress = current;
        });
    }

    public void ReportStatus(string title, string text)
    {
        UpdateOnUiThread(() =>
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

    partial void OnSelectedCompletionActionIndexChanged(int value)
    {
        OnPropertyChanged(nameof(SelectedCompletionAction));
    }

    private void UpdateOnUiThread(Action action)
    {
        if (dispatcherQueue == null || !dispatcherQueue.TryEnqueue(() => action()))
        {
            action();
        }
    }
}
