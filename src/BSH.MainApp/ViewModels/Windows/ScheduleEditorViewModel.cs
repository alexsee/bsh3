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
    private readonly IOrchestrationService orchestrationService;

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
    [NotifyPropertyChangedFor(nameof(ShowDatePicker))]
    [NotifyPropertyChangedFor(nameof(ShowWeeklyDayPicker))]
    [NotifyPropertyChangedFor(nameof(ShowMonthlyDayPicker))]
    [NotifyPropertyChangedFor(nameof(ShowScheduleDayOrDatePicker))]
    [NotifyPropertyChangedFor(nameof(TimeHeader))]
    [NotifyPropertyChangedFor(nameof(AddScheduleHelpText))]
    private ScheduleEntryKind selectedScheduleKind = ScheduleEntryKind.Daily;

    [ObservableProperty]
    private DateTimeOffset startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan startTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private DayOfWeek selectedWeeklyDay = DayOfWeek.Monday;

    [ObservableProperty]
    private int selectedMonthlyDay = 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteScheduleCommand))]
    private ScheduleEditorEntryViewModel? selectedEntry;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRetentionIntervalControls))]
    [NotifyPropertyChangedFor(nameof(ShowAutomaticHourlyThreshold))]
    private ScheduleRetentionMode retentionMode = ScheduleRetentionMode.None;

    [ObservableProperty]
    private ScheduleRetentionIntervalUnit retentionIntervalUnit = ScheduleRetentionIntervalUnit.Day;

    [ObservableProperty]
    private int retentionInterval = 1;

    [ObservableProperty]
    private int automaticHourlyBackupThreshold = 24;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowFullBackupDays))]
    private bool enableScheduledFullBackups;

    [ObservableProperty]
    private int scheduledFullBackupDays = 1;

    [ObservableProperty]
    private bool performMissedBackupsLater;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyScheduleState))]
    private bool hasEntries;

    public bool ShowDatePicker => SelectedScheduleKind == ScheduleEntryKind.Once;

    public bool ShowWeeklyDayPicker => SelectedScheduleKind == ScheduleEntryKind.Weekly;

    public bool ShowMonthlyDayPicker => SelectedScheduleKind == ScheduleEntryKind.Monthly;

    public bool ShowScheduleDayOrDatePicker =>
        ShowDatePicker || ShowWeeklyDayPicker || ShowMonthlyDayPicker;

    public bool ShowRetentionIntervalControls => RetentionMode == ScheduleRetentionMode.Interval;

    public bool ShowAutomaticHourlyThreshold => RetentionMode == ScheduleRetentionMode.Automatic;

    public bool ShowFullBackupDays => EnableScheduledFullBackups;

    public bool ShowEmptyScheduleState => !HasEntries;

    public string TimeHeader => SelectedScheduleKind == ScheduleEntryKind.Hourly
        ? "Minute"
        : "Time";

    public string AddScheduleHelpText => SelectedScheduleKind switch
    {
        ScheduleEntryKind.Once => "Run once on the selected date and time.",
        ScheduleEntryKind.Hourly => "Run every hour at the selected minute.",
        ScheduleEntryKind.Daily => "Run every day at the selected time.",
        ScheduleEntryKind.Weekly => "Run every week on the selected day and time.",
        ScheduleEntryKind.Monthly => "Run every month on the selected day and time.",
        _ => string.Empty,
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
        settings.AddSchedule(
            SelectedScheduleKind,
            StartDate,
            StartTime,
            SelectedWeeklyDay,
            SelectedMonthlyDay);
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
            await orchestrationService.RefreshAutomationAsync();
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

        HasEntries = Entries.Count > 0;
    }
}
