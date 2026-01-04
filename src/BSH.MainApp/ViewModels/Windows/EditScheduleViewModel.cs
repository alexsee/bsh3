// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Data;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using BSH.MainApp.Windows;
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

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

    public override string Title => "Edit Backup Schedule";

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

    public EditScheduleViewModel(IConfigurationManager configurationManager, IDbClientFactory dbClientFactory)
    {
        this.configurationManager = configurationManager;
        this.dbClientFactory = dbClientFactory;
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
        var scheduleDialog = new AddScheduleWindow();
        if (await scheduleDialog.ShowDialogAsync())
        {
            var viewModel = scheduleDialog.ViewModel;
            AddScheduleEntry(viewModel.SelectedInterval, viewModel.StartTime);
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

    private async Task SaveSchedulesAsync()
    {
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
                var dateString = entry.StartTime.ToString("dd.MM.yyyy HH:mm:ss");

                var query = $"INSERT INTO schedule (timType, timDate) VALUES ({dbIntervalType}, '{dateString}')";
                await dbClient.ExecuteNonQueryAsync(CommandType.Text, query, null);
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
