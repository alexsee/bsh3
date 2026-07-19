// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Service.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;

using Brightbits.BSH.Engine.Types;
namespace BSH.MainApp.ViewModels.Windows;

public partial class StatusViewModel : ObservableObject, IStatusReport
{
    private readonly DispatcherQueue? dispatcherQueue;

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

    [ObservableProperty]
    private int selectedCompletionActionIndex = 0;

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
        UpdateOnUiThread(() =>
        {
            CurrentFileText = file;
        });
    }

    public void ReportProgress(int total, int current)
    {
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
