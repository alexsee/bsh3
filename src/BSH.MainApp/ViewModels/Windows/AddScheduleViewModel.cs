// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels.Windows;

public partial class AddScheduleViewModel : ModalViewModel
{
    public override string Title => "Add Backup Schedule";

    public async override Task InitializeAsync()
    {
        UpdateTimeFormat();
    }

    public async override Task SaveConfigurationAsync()
    {
        await Task.CompletedTask;
    }

    [ObservableProperty]
    private int selectedInterval = 0;

    [ObservableProperty]
    private DateTime startTime = DateTime.Now;

    [ObservableProperty]
    private bool showTimeSpinner = false;

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
}
