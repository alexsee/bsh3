// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services;
using BSH.MainApp.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class ScheduleEditorViewModel : ObservableObject
{
    private readonly ScheduleSettingsService scheduleSettingsService;
    private readonly IConfigurationManager configurationManager;
    private readonly IScheduledBackupService scheduledBackupService;

    private ScheduleSettings settings = new();

    public TaskCompletionSource<bool> TaskCompletionSource
    {
        get;
    } = new();

    public IReadOnlyList<ScheduleEntryKind> ScheduleKinds
    {
        get;
    } =
    [
        ScheduleEntryKind.Once,
        ScheduleEntryKind.Hourly,
        ScheduleEntryKind.Daily,
        ScheduleEntryKind.Weekly,
        ScheduleEntryKind.Monthly
    ];

    public IReadOnlyList<ScheduleRetentionMode> RetentionModes
    {
        get;
    } =
    [
        ScheduleRetentionMode.None,
        ScheduleRetentionMode.Automatic,
        ScheduleRetentionMode.Interval
    ];

    public IReadOnlyList<ScheduleRetentionIntervalUnit> RetentionIntervalUnits
    {
        get;
    } =
    [
        ScheduleRetentionIntervalUnit.Hour,
        ScheduleRetentionIntervalUnit.Day,
        ScheduleRetentionIntervalUnit.Week
    ];

    public ObservableCollection<ScheduleEditorEntryViewModel> Entries
    {
        get;
    } = new();

    [ObservableProperty]
    private ScheduleEntryKind selectedScheduleKind = ScheduleEntryKind.Daily;

    [ObservableProperty]
    private DateTimeOffset startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteScheduleCommand))]
    private ScheduleEditorEntryViewModel? selectedEntry;

    [ObservableProperty]
    private ScheduleRetentionMode retentionMode = ScheduleRetentionMode.None;

    [ObservableProperty]
    private ScheduleRetentionIntervalUnit retentionIntervalUnit = ScheduleRetentionIntervalUnit.Day;

    [ObservableProperty]
    private int retentionInterval = 1;

    [ObservableProperty]
    private int automaticHourlyBackupThreshold = 24;

    [ObservableProperty]
    private bool enableScheduledFullBackups;

    [ObservableProperty]
    private int scheduledFullBackupDays = 1;

    [ObservableProperty]
    private bool performMissedBackupsLater;

    public ScheduleEditorViewModel(
        ScheduleSettingsService scheduleSettingsService,
        IConfigurationManager configurationManager,
        IScheduledBackupService scheduledBackupService)
    {
        this.scheduleSettingsService = scheduleSettingsService;
        this.configurationManager = configurationManager;
        this.scheduledBackupService = scheduledBackupService;
    }

    public async Task InitializeAsync()
    {
        settings = await scheduleSettingsService.LoadAsync();
        LoadEntries();

        RetentionMode = settings.RetentionMode;
        RetentionIntervalUnit = settings.RetentionIntervalUnit;
        RetentionInterval = settings.RetentionInterval;
        AutomaticHourlyBackupThreshold = settings.AutomaticHourlyBackupThreshold;
        EnableScheduledFullBackups = settings.EnableScheduledFullBackups;
        ScheduledFullBackupDays = settings.ScheduledFullBackupDays;
        PerformMissedBackupsLater = settings.PerformMissedBackupsLater;
    }

    [RelayCommand]
    private void AddSchedule()
    {
        var date = StartDate.Date.Add(StartTime);
        settings.AddSchedule(SelectedScheduleKind, date);
        LoadEntries();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSchedule))]
    private void DeleteSchedule()
    {
        if (SelectedEntry == null)
        {
            return;
        }

        settings.DeleteSchedule(SelectedEntry.Entry);
        SelectedEntry = null;
        LoadEntries();
    }

    [RelayCommand]
    private async Task Save()
    {
        settings.RetentionMode = RetentionMode;
        settings.RetentionIntervalUnit = RetentionIntervalUnit;
        settings.RetentionInterval = RetentionInterval;
        settings.AutomaticHourlyBackupThreshold = AutomaticHourlyBackupThreshold;
        settings.EnableScheduledFullBackups = EnableScheduledFullBackups;
        settings.ScheduledFullBackupDays = ScheduledFullBackupDays;
        settings.PerformMissedBackupsLater = PerformMissedBackupsLater;

        await scheduleSettingsService.SaveAsync(settings);

        if (configurationManager.TaskType == TaskType.Schedule)
        {
            scheduledBackupService.Stop();
            await scheduledBackupService.StartAsync();
        }

        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
    }

    private bool CanDeleteSchedule() => SelectedEntry != null;

    private void LoadEntries()
    {
        Entries.Clear();
        foreach (var entry in settings.Entries.OrderBy(x => x.Type).ThenBy(x => x.Date))
        {
            Entries.Add(new ScheduleEditorEntryViewModel(entry));
        }
    }
}
