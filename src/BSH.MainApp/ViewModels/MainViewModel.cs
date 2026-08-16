// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Humanizer;
using Microsoft.UI.Dispatching;
using Serilog;

namespace BSH.MainApp.ViewModels;

public partial class MainViewModel : ObservableObject, INavigationAware, IStatusReport
{
    private static readonly ILogger Logger = Log.ForContext<MainViewModel>();

    private readonly IPresentationService presentationService;
    private readonly IStatusService statusService;
    private readonly IQueryManager queryManager;
    private readonly IJobService jobService;
    private readonly IScheduledBackupService scheduledBackupService;
    private readonly DispatcherQueue dispatcherQueue;
    private readonly IConfigurationManager configurationManager;

    [ObservableProperty]
    private string? lastBackupDate;

    [ObservableProperty]
    private string? nextBackupDate;

    [ObservableProperty]
    private string? backupMode;

    [ObservableProperty]
    private string? availableDiskSpace;

    [ObservableProperty]
    private string? totalFilesBackuped;

    [ObservableProperty]
    private string? totalBackups;

    [ObservableProperty]
    private string? totalFileSize;

    [ObservableProperty]
    private string? scheduleWarningText;

    [ObservableProperty]
    private bool scheduleWarningVisible;

    [ObservableProperty]
    private bool nextBackupGridVisibility = true;

    [ObservableProperty]
    private bool progressGridVisibility = false;

    [ObservableProperty]
    private string? currentProgressStatusTitle;

    [ObservableProperty]
    private string? currentProgressStatusText;

    [ObservableProperty]
    private double currentProgressValue = 0;

    [ObservableProperty]
    private double currentProgressMax = 100;

    [ObservableProperty]
    private string? currentFileText;

    [ObservableProperty]
    private string? currentFilePath;

    [ObservableProperty]
    private string? progressCountText;

    [ObservableProperty]
    private string? systemStatusText;

    public MainViewModel(
        IPresentationService presentationService,
        IStatusService statusService,
        IQueryManager queryManager,
        IJobService jobService,
        IScheduledBackupService scheduledBackupService,
        IConfigurationManager configurationManager,
        DispatcherQueue dispatcherQueue)
    {
        this.presentationService = presentationService;
        this.statusService = statusService;
        this.queryManager = queryManager;
        this.jobService = jobService;
        this.scheduledBackupService = scheduledBackupService;
        this.dispatcherQueue = dispatcherQueue;
        this.configurationManager = configurationManager;
        this.statusService.AddObserver(this, true);
        // Apply immediately so the overview has status text before the first dispatcher tick.
        ApplySystemStatus(this.statusService.SystemStatus);
    }

    public async void OnNavigatedTo(object parameter)
    {
        await UpdateBackupStatsAsync();
    }

    public void OnNavigatedFrom()
    {
        this.statusService.RemoveObserver(this);
    }

    private async Task UpdateBackupStatsAsync()
    {
        // set backup dates
        var lastBackup = await queryManager.GetLastBackupAsync();
        if (lastBackup != null)
        {
            LastBackupDate = lastBackup.CreationDate.HumanizeDate();
        }

        // set configuration
        var hasScheduleWarning = false;

        if (configurationManager.TaskType == TaskType.Auto)
        {
            NextBackupDate = scheduledBackupService.GetNextBackupDate().HumanizeDate();
            BackupMode = "MainView_BackupMode_Automatic".GetLocalized();
        }
        else if (configurationManager.TaskType == TaskType.Schedule)
        {
            NextBackupDate = scheduledBackupService.GetNextBackupDate().HumanizeDate();
            BackupMode = "MainView_BackupMode_Scheduled".GetLocalized();
            hasScheduleWarning = !await scheduledBackupService.HasScheduleEntriesAsync();
        }
        else
        {
            NextBackupDate = "MainView_NonePlanned".GetLocalized();
            BackupMode = "MainView_BackupMode_Manual".GetLocalized();
        }

        if (hasScheduleWarning)
        {
            ScheduleWarningText = "MainView_ScheduleWarning_NoEntries".GetLocalized();
            ScheduleWarningVisible = true;
        }
        else
        {
            ScheduleWarningText = "";
            ScheduleWarningVisible = false;
        }

        AvailableDiskSpace = string.IsNullOrEmpty(configurationManager.FreeSpace)
            ? ""
            : double.Parse(configurationManager.FreeSpace, System.Globalization.CultureInfo.InvariantCulture)
                .Bytes()
                .Humanize();

        TotalFilesBackuped = (await queryManager.GetNumberOfFilesAsync()).ToString("g");
        TotalFileSize = (await queryManager.GetTotalFileSizeAsync()).Bytes().Humanize();
        TotalBackups = (await queryManager.GetNumberOfVersionsAsync()).ToString("g");
    }

    [RelayCommand]
    private async Task StartManualBackup()
    {
        var (result, backup) = await this.presentationService.ShowCreateBackupWindowAsync();
        if (result)
        {
            await jobService.CreateBackupAsync(backup.Title ?? "CreateBackup_Title_Manual".GetLocalized(), backup.Description ?? "", true, backup.IsFullBackup, backup.IsShutdownPc);
        }
    }

    [RelayCommand]
    private void CancelBackup()
    {
        jobService.Cancel();
    }

    public void ReportAction(ActionType action, bool silent)
    {
    }

    public void ReportState(JobState jobState)
    {
        EnqueueUi(async () =>
        {
            if (jobState == JobState.RUNNING)
            {
                CurrentFileText = null;
                CurrentFilePath = null;
                ProgressCountText = null;
                ProgressDisplay.Assign(
                    (int)CurrentProgressMax,
                    (int)CurrentProgressValue,
                    total: 0,
                    current: 0,
                    maximum => CurrentProgressMax = maximum,
                    value => CurrentProgressValue = value);
                NextBackupGridVisibility = false;
                ProgressGridVisibility = true;
                return;
            }

            if (jobState == JobState.FINISHED)
            {
                try
                {
                    await UpdateBackupStatsAsync();
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Could not refresh overview statistics after backup.");
                }
            }

            NextBackupGridVisibility = true;
            ProgressGridVisibility = false;
        });
    }

    public void ReportStatus(string title, string text)
    {
        EnqueueUi(() =>
        {
            CurrentProgressStatusTitle = title;
            CurrentProgressStatusText = text;
        });
    }

    public void ReportProgress(int total, int current)
    {
        EnqueueUi(() =>
        {
            ProgressDisplay.Assign(
                (int)CurrentProgressMax,
                (int)CurrentProgressValue,
                total,
                current,
                maximum => CurrentProgressMax = maximum,
                value => CurrentProgressValue = value);

            var safeTotal = Math.Max(0, total);
            var safeCurrent = Math.Clamp(current, 0, safeTotal);
            ProgressCountText = $"{safeCurrent} / {safeTotal} {"Status_FilesProcessed".GetLocalized()}";
        });
    }

    public void ReportFileProgress(string file)
    {
        EnqueueUi(() =>
        {
            CurrentFilePath = file ?? string.Empty;
            CurrentFileText = Formatter.ShortenPathMiddleSafe(file, 70, Logger);
        });
    }

    public void ReportSystemStatus(SystemStatus systemStatus)
    {
        EnqueueUi(() => ApplySystemStatus(systemStatus));
    }

    private void EnqueueUi(Action action)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Overview UI update failed.");
            }
        });
    }

    private void EnqueueUi(Func<Task> action)
    {
        dispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Overview UI update failed.");
            }
        });
    }

    private void ApplySystemStatus(SystemStatus systemStatus)
    {
        SystemStatusText = systemStatus switch
        {
            SystemStatus.PAUSED_DUE_TO_BATTERY => "MainView_SystemStatus_BatteryPaused".GetLocalized(),
            SystemStatus.DEACTIVATED => "MainView_SystemStatus_Deactivated".GetLocalized(),
            SystemStatus.NOT_CONFIGURED => "MainView_SystemStatus_NotConfigured".GetLocalized(),
            _ => "MainView_SystemStatus_Running".GetLocalized(),
        };
    }
}
