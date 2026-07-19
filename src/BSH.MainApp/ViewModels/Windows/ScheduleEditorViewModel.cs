// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Globalization;
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
    private readonly IOrchestrationService orchestrationService;

    private ScheduleSettings settings = new();

    public TaskCompletionSource<bool> TaskCompletionSource
    {
        get;
    } = new();

    public IReadOnlyList<DayOfWeek> WeeklyDays
    {
        get;
    } =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    ];

    public ObservableCollection<ScheduleEditorEntryViewModel> Entries
    {
        get;
    } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDatePicker))]
    [NotifyPropertyChangedFor(nameof(ShowHourlyMinutePicker))]
    [NotifyPropertyChangedFor(nameof(ShowDailyTimePicker))]
    [NotifyPropertyChangedFor(nameof(ShowWeeklyDayPicker))]
    [NotifyPropertyChangedFor(nameof(ShowMonthlyDayPicker))]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private ScheduleEntryKind selectedScheduleKind = ScheduleEntryKind.Daily;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private DateTimeOffset startDate = DateTimeOffset.Now;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private int selectedHourlyMinute = DateTime.Now.Minute;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private DayOfWeek selectedWeeklyDay = DateTime.Now.DayOfWeek;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NewScheduleSummary))]
    private int selectedMonthlyDay = DateTime.Now.Day;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowAutomaticRetentionSettings))]
    [NotifyPropertyChangedFor(nameof(ShowIntervalRetentionSettings))]
    [NotifyPropertyChangedFor(nameof(RetentionHelpText))]
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

    public bool ShowDatePicker => SelectedScheduleKind == ScheduleEntryKind.Once;

    public bool ShowHourlyMinutePicker => SelectedScheduleKind == ScheduleEntryKind.Hourly;

    public bool ShowDailyTimePicker => SelectedScheduleKind == ScheduleEntryKind.Daily;

    public bool ShowWeeklyDayPicker => SelectedScheduleKind == ScheduleEntryKind.Weekly;

    public bool ShowMonthlyDayPicker => SelectedScheduleKind == ScheduleEntryKind.Monthly;

    public bool HasEntries => Entries.Count > 0;

    public bool HasNoEntries => !HasEntries;

    public bool ShowAutomaticRetentionSettings => RetentionMode == ScheduleRetentionMode.Automatic;

    public bool ShowIntervalRetentionSettings => RetentionMode == ScheduleRetentionMode.Interval;

    public string NewScheduleSummary => SelectedScheduleKind switch
    {
        ScheduleEntryKind.Once => $"Once on {StartDate:d} at {FormatTime(StartTime)}",
        ScheduleEntryKind.Hourly => $"Every hour at {SelectedHourlyMinute:00} minutes past the hour",
        ScheduleEntryKind.Daily => $"Every day at {FormatTime(StartTime)}",
        ScheduleEntryKind.Weekly => $"Every {SelectedWeeklyDay} at {FormatTime(StartTime)}",
        ScheduleEntryKind.Monthly => $"On day {SelectedMonthlyDay} of every month at {FormatTime(StartTime)}",
        _ => string.Empty,
    };

    public string RetentionHelpText => RetentionMode switch
    {
        ScheduleRetentionMode.Automatic => "Keep recent backups in detail, then reduce them to daily and weekly versions over time.",
        ScheduleRetentionMode.Interval => "Delete non-protected backups after a fixed amount of time.",
        _ => "Keep scheduled backups until you delete them.",
    };

    public ScheduleEditorViewModel(
        ScheduleSettingsService scheduleSettingsService,
        IConfigurationManager configurationManager,
        IOrchestrationService orchestrationService)
    {
        this.scheduleSettingsService = scheduleSettingsService;
        this.configurationManager = configurationManager;
        this.orchestrationService = orchestrationService;
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
        var selectedTime = SelectedScheduleKind == ScheduleEntryKind.Hourly
            ? TimeSpan.FromMinutes(SelectedHourlyMinute)
            : StartTime;

        settings.AddSchedule(
            SelectedScheduleKind,
            StartDate,
            selectedTime,
            SelectedWeeklyDay,
            SelectedMonthlyDay);
        LoadEntries();
    }

    [RelayCommand]
    private void DeleteSchedule(ScheduleEditorEntryViewModel? entry)
    {
        if (entry == null)
        {
            return;
        }

        settings.DeleteSchedule(entry.Entry);
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
            await orchestrationService.RefreshAutomationAsync();
        }

        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
    }

    private void LoadEntries()
    {
        Entries.Clear();
        foreach (var entry in settings.Entries.OrderBy(x => x.Type).ThenBy(x => x.Date))
        {
            Entries.Add(new ScheduleEditorEntryViewModel(entry));
        }

        OnPropertyChanged(nameof(HasEntries));
        OnPropertyChanged(nameof(HasNoEntries));
    }

    private static string FormatTime(TimeSpan time)
    {
        return DateTime.Today.Add(time).ToString("t", CultureInfo.CurrentCulture);
    }
}
