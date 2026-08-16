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
using Serilog;

namespace BSH.MainApp.ViewModels.Windows;

public partial class StatusViewModel : ObservableObject, IStatusReport
{
    private static readonly ILogger Logger = Log.ForContext<StatusViewModel>();

    private readonly DispatcherQueue? dispatcherQueue;
    private int isActive = 1;

    [ObservableProperty]
    private string statusTitle = "";

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    private string currentFileText = "";

    [ObservableProperty]
    private string currentFilePath = "";

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

    public void Detach()
    {
        Interlocked.Exchange(ref isActive, 0);
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
            CurrentFilePath = file ?? string.Empty;
            CurrentFileText = Formatter.ShortenPathMiddleSafe(file, 75, Logger);
        });
    }

    public void ReportProgress(int total, int current)
    {
        UpdateOnUiThread(() =>
        {
            ProgressDisplay.Assign(
                TotalProgress,
                CurrentProgress,
                total,
                current,
                maximum => TotalProgress = maximum,
                value => CurrentProgress = value);
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

    private bool IsActive => Volatile.Read(ref isActive) == 1;

    private void UpdateOnUiThread(Action action)
    {
        if (!IsActive)
        {
            return;
        }

        if (dispatcherQueue == null)
        {
            action();
            return;
        }

        dispatcherQueue.TryEnqueue(() =>
        {
            if (!IsActive)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Status window UI update failed.");
            }
        });
    }
}
