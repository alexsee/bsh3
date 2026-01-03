// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class AddScheduleViewModel : ObservableObject
{
    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    [ObservableProperty]
    private int selectedInterval = 0;

    [ObservableProperty]
    private DateTime startTime = DateTime.Now;

    [ObservableProperty]
    private bool showTimeSpinner = false;

    public AddScheduleViewModel()
    {
        UpdateTimeFormat();
    }

    partial void OnSelectedIntervalChanged(int value)
    {
        UpdateTimeFormat();
    }

    private void UpdateTimeFormat()
    {
        // Update UI based on selected interval
        // Show time spinner for: Hourly (1), Daily (2), Monthly (4)
        ShowTimeSpinner = SelectedInterval is 1 or 2 or 4;
    }

    [RelayCommand]
    private void Save()
    {
        TaskCompletionSource.SetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.SetResult(false);
    }
}
