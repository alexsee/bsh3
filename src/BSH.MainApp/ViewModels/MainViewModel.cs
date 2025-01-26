// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Jobs;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Helpers;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Microsoft.UI.Dispatching;

namespace BSH.MainApp.ViewModels;

public partial class MainViewModel : ObservableObject, INavigationAware, IStatusReport
{
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
        this.statusService.AddObserver(this, true);
        this.configurationManager = configurationManager;
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
        if (configurationManager.TaskType == TaskType.Auto)
        {
            NextBackupDate = scheduledBackupService.GetNextBackupDate().HumanizeDate();
            BackupMode = "MainView_BackupMode_Automatic".GetLocalized();
        }
        else if (configurationManager.TaskType == TaskType.Schedule)
        {
            NextBackupDate = scheduledBackupService.GetNextBackupDate().HumanizeDate();
            BackupMode = "MainView_BackupMode_Scheduled".GetLocalized();
        }
        else
        {
            NextBackupDate = "None planned";
            BackupMode = "MainView_BackupMode_Manual".GetLocalized();
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
            await jobService.CreateBackupAsync(backup.Title ?? "Manual backup", backup.Description ?? "", true, backup.IsFullBackup, backup.IsShutdownPc);
        }
    }

    public void ReportAction(ActionType action, bool silent)
    {
    }

    public void ReportState(JobState jobState)
    {
        dispatcherQueue.TryEnqueue(async () =>
        {
            if (jobState == JobState.RUNNING)
            {
                NextBackupGridVisibility = false;
                ProgressGridVisibility = true;
                return;
            }

            if (jobState == JobState.FINISHED)
            {
                await UpdateBackupStatsAsync();
            }

            NextBackupGridVisibility = true;
            ProgressGridVisibility = false;
        });
    }

    public void ReportStatus(string title, string text)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            CurrentProgressStatusTitle = title;
            CurrentProgressStatusText = text;
        });
    }

    public void ReportProgress(int total, int current)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            CurrentProgressMax = total;
            CurrentProgressValue = current;
        });
    }

    public void ReportFileProgress(string file)
    {
    }

    public void ReportSystemStatus(SystemStatus systemStatus)
    {
    }
}
