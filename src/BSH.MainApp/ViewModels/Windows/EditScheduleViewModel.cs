// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Data;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using BSH.MainApp.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels.Windows;

public partial class ScheduleEntry : ObservableObject
{
    [ObservableProperty]
    private int intervalType;

    [ObservableProperty]
    private DateTime startTime;

    [ObservableProperty]
    private string displayText = string.Empty;

    public string IntervalName => intervalType switch
    {
        0 => "Once",
        1 => "Hourly",
        2 => "Daily",
        3 => "Weekly",
        4 => "Monthly",
        _ => "Unknown"
    };

    public string TimeDisplay => intervalType switch
    {
        0 => startTime.ToString(),
        1 => $"at {startTime.Minute} minutes",
        2 => startTime.ToShortTimeString(),
        3 => startTime.ToString("dddd, HH:mm"),
        4 => $"Day {startTime.Day} at {startTime:HH:mm}",
        _ => string.Empty
    };
}

public enum DeleteInterval
{
    DontDelete = 0,
    Manual = 1,
    Auto = 2
}

public partial class EditScheduleViewModel : ModalViewModel
{
    private readonly IConfigurationManager configurationManager;
    private readonly IDbClientFactory dbClientFactory;
    private readonly IPresentationService presentationService;

    public override string Title => "Edit Backup Schedule";

    public override int Width => 900;

    public override int Height => 600;


    [ObservableProperty]
    private ObservableCollection<ScheduleEntry> scheduleList = new();

    [ObservableProperty]
    private ScheduleEntry? selectedSchedule;

    [ObservableProperty]
    private bool deleteButtonEnabled;

    [ObservableProperty]
    private DeleteInterval deleteMode = Windows.DeleteInterval.DontDelete;

    [ObservableProperty]
    private int deleteInterval = 1;

    [ObservableProperty]
    private int deleteIntervalType = 0; // 0=hour, 1=day, 2=week

    [ObservableProperty]
    private bool enableAutoDelete;

    [ObservableProperty]
    private int autoDeleteHourBackups = 48;

    [ObservableProperty]
    private bool enableFullBackup;

    [ObservableProperty]
    private int fullBackupDay = 1;

    public EditScheduleViewModel(IConfigurationManager configurationManager, IDbClientFactory dbClientFactory, IPresentationService presentationService)
    {
        this.configurationManager = configurationManager;
        this.dbClientFactory = dbClientFactory;
        this.presentationService = presentationService;
    }

    public async override Task InitializeAsync()
    {
        await LoadSchedulesAsync();
        LoadConfiguration();
    }

    partial void OnSelectedScheduleChanged(ScheduleEntry? value)
    {
        DeleteButtonEnabled = value != null;
    }

    [RelayCommand]
    private async Task AddSchedule()
    {
        var (viewModel, result) = await this.presentationService.ShowAddScheduleWindowAsync();
        if (result)
        {
            var finalDateTime = ConvertAddScheduleViewModelToDateTime(viewModel);
            AddScheduleEntry(viewModel.SelectedInterval, finalDateTime);
        }
    }

    [RelayCommand]
    private void DeleteSchedule()
    {
        if (SelectedSchedule != null)
        {
            ScheduleList.Remove(SelectedSchedule);
            SelectedSchedule = null;
        }
    }

    public void AddScheduleEntry(int intervalType, DateTime startTime)
    {
        var entry = new ScheduleEntry
        {
            IntervalType = intervalType,
            StartTime = startTime
        };
        ScheduleList.Add(entry);
    }

    private DateTime ConvertAddScheduleViewModelToDateTime(AddScheduleViewModel viewModel)
    {
        var baseDate = viewModel.StartDate.Date;
        var baseTime = viewModel.StartTime;

        return viewModel.SelectedInterval switch
        {
            0 => baseDate.Add(baseTime), // Once: use StartDate + StartTime as-is
            1 => baseDate.Add(baseTime), // Daily: use StartDate + StartTime (time is what matters)
            2 => baseDate.Add(baseTime), // Hourly: use StartDate + StartTime (minute is what matters)
            3 => GetNextWeeklyDate(baseDate, baseTime, viewModel), // Weekly: find next occurrence based on day flags
            4 => GetMonthlyDate(DateTime.Now.Date, baseTime, viewModel.DayOfMonth), // Monthly: use DayOfMonth
            _ => baseDate.Add(baseTime)
        };
    }

    private DateTime GetNextWeeklyDate(DateTime baseDate, TimeSpan time, AddScheduleViewModel viewModel)
    {
        // Get selected days of week (0=Sunday, 1=Monday, etc.)
        var selectedDays = new List<DayOfWeek>();

        if (viewModel.Sunday)
        {
            selectedDays.Add(DayOfWeek.Sunday);
        }

        if (viewModel.Monday)
        {
            selectedDays.Add(DayOfWeek.Monday);
        }

        if (viewModel.Tuesday)
        {
            selectedDays.Add(DayOfWeek.Tuesday);
        }

        if (viewModel.Wednesday)
        {
            selectedDays.Add(DayOfWeek.Wednesday);
        }

        if (viewModel.Thursday)
        {
            selectedDays.Add(DayOfWeek.Thursday);
        }

        if (viewModel.Friday)
        {
            selectedDays.Add(DayOfWeek.Friday);
        }

        if (viewModel.Saturday)
        {
            selectedDays.Add(DayOfWeek.Saturday);
        }

        // If no days selected, default to current day
        if (selectedDays.Count == 0)
        {
            selectedDays.Add(baseDate.DayOfWeek);
        }

        // Find the next occurrence of one of the selected days
        var currentDate = baseDate;
        for (int i = 0; i < 7; i++)
        {
            if (selectedDays.Contains(currentDate.DayOfWeek))
            {
                return currentDate.Add(time);
            }
            currentDate = currentDate.AddDays(1);
        }

        // Fallback to first selected day
        return baseDate.Add(time);
    }

    private DateTime GetMonthlyDate(DateTime baseDate, TimeSpan time, int dayOfMonth)
    {
        // Clamp dayOfMonth to valid range for the month
        var daysInMonth = DateTime.DaysInMonth(baseDate.Year, baseDate.Month);
        var validDay = Math.Min(dayOfMonth, daysInMonth);

        var monthlyDate = new DateTime(baseDate.Year, baseDate.Month, validDay);

        // If the calculated date is in the past, move to next month
        return monthlyDate.Add(time);
    }

    private async Task LoadSchedulesAsync()
    {
        try
        {
            ScheduleList.Clear();

            using var dbClient = dbClientFactory.CreateDbClient();
            using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM schedule", null);

            while (await reader.ReadAsync())
            {
                try
                {
                    var intervalType = reader.GetInt32("timType");
                    var dateString = reader["timDate"].ToString();

                    if (string.IsNullOrEmpty(dateString) || !DateTime.TryParse(dateString, out var parsedDate))
                    {
                        continue;
                    }

                    // Convert database timType (1-5) to ViewModel intervalType (0-4)
                    var vmIntervalType = intervalType - 1;

                    var entry = new ScheduleEntry
                    {
                        IntervalType = vmIntervalType,
                        StartTime = parsedDate
                    };

                    ScheduleList.Add(entry);
                }
                catch
                {
                    // Ignore malformed schedule entries
                }
            }

            await reader.CloseAsync();
        }
        catch
        {
            // If there's an error loading, continue with empty schedule list
        }
    }

    public async override Task SaveConfigurationAsync()
    {
        SaveConfiguration();

        try
        {
            using var dbClient = dbClientFactory.CreateDbClient();

            // Delete all existing schedules
            await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM schedule", null);

            // Insert all current schedules
            foreach (var entry in ScheduleList)
            {
                // Convert ViewModel intervalType (0-4) to database timType (1-5)
                var dbIntervalType = entry.IntervalType + 1;
                var dateString = entry.StartTime.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                var query = $"INSERT INTO schedule (timType, timDate) VALUES (@type, @date)";
                await dbClient.ExecuteNonQueryAsync(CommandType.Text, query, [("type", dbIntervalType), ("type", dateString)]);
            }
        }
        catch
        {
            // Log error but don't throw - allow window to close
        }
    }

    private void LoadConfiguration()
    {
        try
        {
            var intervallDelete = configurationManager.IntervallDelete ?? "";

            if (string.IsNullOrEmpty(intervallDelete))
            {
                DeleteMode = Windows.DeleteInterval.DontDelete;
            }
            else if (intervallDelete == "auto")
            {
                DeleteMode = Windows.DeleteInterval.Auto;
                AutoDeleteHourBackups = int.TryParse(configurationManager.IntervallAutoHourBackups, out var hours)
                    ? hours
                    : 24;
            }
            else
            {
                DeleteMode = Windows.DeleteInterval.Manual;
                var parts = intervallDelete.Split('|');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var interval))
                {
                    DeleteInterval = interval;

                    DeleteIntervalType = parts[0] switch
                    {
                        "hour" => 0,
                        "day" => 1,
                        "week" => 2,
                        _ => 0
                    };
                }
            }

            var scheduleFullBackup = configurationManager.ScheduleFullBackup ?? "";
            if (!string.IsNullOrEmpty(scheduleFullBackup))
            {
                EnableFullBackup = true;
                var parts = scheduleFullBackup.Split('|');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var day))
                {
                    FullBackupDay = day;
                }
            }
            else
            {
                EnableFullBackup = false;
                FullBackupDay = 1;
            }
        }
        catch
        {
            // Use defaults if loading fails
        }
    }

    private void SaveConfiguration()
    {
        try
        {
            // Save delete interval settings
            switch (DeleteMode)
            {
                case Windows.DeleteInterval.DontDelete:
                    configurationManager.IntervallDelete = "";
                    break;

                case Windows.DeleteInterval.Manual:
                    var intervalTypeStr = DeleteIntervalType switch
                    {
                        0 => "hour",
                        1 => "day",
                        2 => "week",
                        _ => "day"
                    };
                    configurationManager.IntervallDelete = $"{intervalTypeStr}|{DeleteInterval}";
                    break;

                case Windows.DeleteInterval.Auto:
                    configurationManager.IntervallDelete = "auto";
                    configurationManager.IntervallAutoHourBackups = AutoDeleteHourBackups.ToString();
                    break;
            }

            // Save full backup settings
            if (EnableFullBackup)
            {
                configurationManager.ScheduleFullBackup = $"day|{FullBackupDay}";
            }
            else
            {
                configurationManager.ScheduleFullBackup = "";
            }
        }
        catch
        {
            // Log error but don't throw
        }
    }
}
