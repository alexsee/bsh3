// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels.Windows;

public partial class AddScheduleViewModel : ModalViewModel
{
    public override string Title => "Add Backup Schedule";

    public override int Width => 800;

    public override int Height => 450;

    public async override Task InitializeAsync()
    {
        UpdateTimeFormat();
    }

    [ObservableProperty]
    private int selectedInterval = 0;

    [ObservableProperty]
    private TimeSpan startTime = TimeSpan.FromDays(1);

    [ObservableProperty]
    private DateTimeOffset startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private bool showTimeSpinner = false;

    [ObservableProperty]
    private bool showOnceOptions = false;

    [ObservableProperty]
    private bool showHourlyOptions = false;

    [ObservableProperty]
    private bool showDailyOptions = false;

    [ObservableProperty]
    private bool showWeeklyOptions = false;

    [ObservableProperty]
    private bool showMonthlyOptions = false;

    [ObservableProperty]
    private bool sunday = false;

    [ObservableProperty]
    private bool monday = true;

    [ObservableProperty]
    private bool tuesday = false;

    [ObservableProperty]
    private bool wednesday = false;

    [ObservableProperty]
    private bool thursday = false;

    [ObservableProperty]
    private bool friday = false;

    [ObservableProperty]
    private bool saturday = false;

    [ObservableProperty]
    private int dayOfMonth = 1;

    partial void OnSelectedIntervalChanged(int value)
    {
        UpdateTimeFormat();
    }

    private void UpdateTimeFormat()
    {
        // Update UI based on selected interval
        // 0: Once, 1: Hourly, 2: Daily, 3: Weekly, 4: Monthly
        ShowOnceOptions = SelectedInterval == 0;
        ShowHourlyOptions = SelectedInterval == 1;
        ShowDailyOptions = SelectedInterval == 2;
        ShowWeeklyOptions = SelectedInterval == 3;
        ShowMonthlyOptions = SelectedInterval == 4;
    }
}
