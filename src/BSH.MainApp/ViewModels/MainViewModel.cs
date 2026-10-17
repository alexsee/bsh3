// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Globalization;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Helpers;
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
    private const string BrowserPageKey = "BSH.MainApp.ViewModels.BrowserViewModel";

    private static readonly ILogger Logger = Log.ForContext<MainViewModel>();

    private readonly IPresentationService presentationService;
    private readonly IStatusService statusService;
    private readonly IQueryManager queryManager;
    private readonly IJobService jobService;
    private readonly IScheduledBackupService scheduledBackupService;
    private readonly DispatcherQueue dispatcherQueue;
    private readonly IConfigurationManager configurationManager;
    private readonly IOrchestrationService orchestrationService;
    private readonly IBackupService? backupService;
    private readonly INavigationService? navigationService;
    private bool systemToggleInFlight;
    private int refreshGeneration;

    private LastBackupResult? lastResult;
    private VersionDetails? lastVersion;
    private bool hasScheduleEntries;
    private bool? isBackupLocationAvailable;
    private SystemStatus systemStatus;
    private ActionType currentAction = ActionType.Backup;

    [ObservableProperty]
    private OverviewSeverity severity = OverviewSeverity.Protected;

    [ObservableProperty]
    private string? statusTitle;

    [ObservableProperty]
    private string? statusText;

    public ObservableCollection<OverviewActionItem> ActionItems { get; } = [];

    [ObservableProperty]
    private bool hasActionItems;

    [ObservableProperty]
    private bool hasLastBackup;

    [ObservableProperty]
    private string? lastBackupWhen;

    [ObservableProperty]
    private string? lastBackupResultText;

    [ObservableProperty]
    private OverviewSeverity lastBackupResultSeverity = OverviewSeverity.Protected;

    [ObservableProperty]
    private string? lastBackupName;

    [ObservableProperty]
    private bool hasLastBackupName;

    [ObservableProperty]
    private bool hasChangeStatistics;

    [ObservableProperty]
    private string? addedFilesText;

    [ObservableProperty]
    private string? modifiedFilesText;

    [ObservableProperty]
    private string? deletedFilesText;

    [ObservableProperty]
    private string? locationKindText;

    [ObservableProperty]
    private string? locationPath;

    [ObservableProperty]
    private string? locationConnectionText;

    [ObservableProperty]
    private OverviewSeverity locationConnectionSeverity = OverviewSeverity.Protected;

    [ObservableProperty]
    private string? freeSpaceText;

    [ObservableProperty]
    private string? freeSpaceHint;

    [ObservableProperty]
    private bool isFreeSpaceLow;

    [ObservableProperty]
    private string? totalsText;

    [ObservableProperty]
    private string? modeAndNextBackupText;

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
    private bool isSystemEnabled;

    [ObservableProperty]
    private bool canToggleSystem = true;

    public MainViewModel(
        IPresentationService presentationService,
        IStatusService statusService,
        IQueryManager queryManager,
        IJobService jobService,
        IScheduledBackupService scheduledBackupService,
        IConfigurationManager configurationManager,
        IOrchestrationService orchestrationService,
        DispatcherQueue dispatcherQueue,
        IBackupService? backupService = null,
        INavigationService? navigationService = null)
    {
        this.presentationService = presentationService;
        this.statusService = statusService;
        this.queryManager = queryManager;
        this.jobService = jobService;
        this.scheduledBackupService = scheduledBackupService;
        this.dispatcherQueue = dispatcherQueue;
        this.configurationManager = configurationManager;
        this.orchestrationService = orchestrationService;
        this.backupService = backupService;
        this.navigationService = navigationService;
        this.statusService.AddObserver(this, true);
        // Apply immediately so the overview has status text before the first dispatcher tick.
        ApplySystemStatus(this.statusService.SystemStatus);
    }

    public async void OnNavigatedTo(object parameter)
    {
        try
        {
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Could not load the overview.");
        }
    }

    public void OnNavigatedFrom()
    {
        this.statusService.RemoveObserver(this);
    }

    private async Task RefreshAsync()
    {
        var generation = ++refreshGeneration;

        var version = queryManager == null ? null : await queryManager.GetLastBackupAsync();
        var result = LastBackupResult.FromJson(configurationManager.LastBackupResult);
        if (result != null && version != null && result.CompletedAt < version.CreationDate)
        {
            // A newer version was written by something that did not record a result.
            result = null;
        }

        var scheduleEntries = configurationManager.TaskType == TaskType.Schedule
            && scheduledBackupService != null
            && await scheduledBackupService.HasScheduleEntriesAsync();

        VersionChangeStatistics? statistics = null;
        if (version != null && result?.Outcome != BackupOutcome.Failed && queryManager != null)
        {
            statistics = await queryManager.GetVersionChangeStatisticsAsync(version.Id);
        }

        string? totals = null;
        if (queryManager != null)
        {
            var versions = await queryManager.GetNumberOfVersionsAsync();
            var files = await queryManager.GetNumberOfFilesAsync();
            var size = await queryManager.GetTotalFileSizeAsync();
            totals = versions == 0
                ? null
                : Format("Overview_Totals", versions, files, size.Bytes().Humanize());
        }

        if (generation != refreshGeneration)
        {
            return;
        }

        lastVersion = version;
        lastResult = result;
        hasScheduleEntries = scheduleEntries;
        TotalsText = totals;
        ApplyLastBackup(statistics);
        ApplyModeAndNextBackup();
        ApplyLocation();
        ApplyStatus();

        _ = CheckBackupLocationAsync(generation);
    }

    private async Task CheckBackupLocationAsync(int generation)
    {
        if (backupService == null || statusService.JobState == JobState.RUNNING)
        {
            return;
        }

        bool available;
        try
        {
            available = await Task.Run(() => backupService.CheckMedia(quickCheck: true));
        }
        catch (Exception ex) when (ex is DeviceNotReadyException or DeviceContainsWrongStateException)
        {
            available = false;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Could not check the backup location.");
            return;
        }

        EnqueueUi(() =>
        {
            if (generation != refreshGeneration)
            {
                return;
            }

            isBackupLocationAvailable = available;
            ApplyLocation();
            ApplyStatus();
        });
    }

    private OverviewInputs BuildInputs()
    {
        return new OverviewInputs
        {
            SystemStatus = systemStatus,
            TaskType = configurationManager.TaskType,
            LastResult = lastResult,
            LastBackupDate = lastVersion?.CreationDate,
            RemindAfterDays = int.TryParse(configurationManager.RemindAfterDays, out var days) ? days : null,
            HasScheduleEntries = hasScheduleEntries,
            IsBackupLocationAvailable = isBackupLocationAvailable,
            FreeSpaceBytes = GetFreeSpaceBytes(),
            RemindSpaceMegabytes = double.TryParse(configurationManager.RemindSpace, NumberStyles.Float, CultureInfo.InvariantCulture, out var megabytes) ? megabytes : null,
            Now = DateTime.Now,
        };
    }

    private void ApplyStatus()
    {
        var inputs = BuildInputs();
        var issues = OverviewStatusEvaluator.Evaluate(inputs);
        Severity = OverviewStatusEvaluator.GetOverallSeverity(issues);

        ActionItems.Clear();
        if (statusService.JobState == JobState.RUNNING)
        {
            StatusTitle = currentAction switch
            {
                ActionType.Backup => "Overview_Running_Backup_Title".GetLocalized(),
                ActionType.Restore => "Overview_Running_Restore_Title".GetLocalized(),
                _ => "Overview_Running_Other_Title".GetLocalized(),
            };
            StatusText = "Overview_Running_Text".GetLocalized();
            HasActionItems = false;
            return;
        }

        if (issues.Count == 0)
        {
            StatusTitle = "Overview_Protected_Title".GetLocalized();
            StatusText = GetProtectedText();
        }
        else
        {
            StatusTitle = GetIssueTitle(issues[0], inputs);
            StatusText = GetIssueText(issues[0], inputs);
        }

        foreach (var issue in issues.Where(issue => issue.Action != OverviewAction.None))
        {
            ActionItems.Add(new OverviewActionItem(
                issue.Kind,
                issue.Severity,
                GetActionText(issue, inputs),
                GetActionButtonText(issue.Action),
                GetActionCommand(issue.Action)));
        }

        HasActionItems = ActionItems.Count > 0;
    }

    private string GetProtectedText()
    {
        var text = lastVersion == null ? "" : Format("Overview_LastBackupAt", lastVersion.CreationDate.HumanizeDate());
        if (configurationManager.TaskType != TaskType.Manual && scheduledBackupService != null)
        {
            text += " " + Format("Overview_NextBackupAt", scheduledBackupService.GetNextBackupDate().HumanizeDate());
        }

        return text.Trim();
    }

    private string GetIssueTitle(OverviewIssue issue, OverviewInputs inputs)
    {
        return issue.Kind switch
        {
            OverviewIssueKind.BackupFailed => "Overview_BackupFailed_Title".GetLocalized(),
            OverviewIssueKind.BackupLocationUnavailable => "Overview_LocationUnavailable_Title".GetLocalized(),
            OverviewIssueKind.BackupOverdue => Format("Overview_Overdue_Title", OverviewStatusEvaluator.GetDaysSinceLastBackup(inputs)),
            OverviewIssueKind.NoBackupYet => "Overview_NoBackup_Title".GetLocalized(),
            OverviewIssueKind.ScheduleEmpty => "Overview_ScheduleEmpty_Title".GetLocalized(),
            OverviewIssueKind.FilesSkipped => Format("Overview_FilesSkipped_Title", lastResult?.FailedFileCount ?? 0),
            OverviewIssueKind.LowDiskSpace => "Overview_LowSpace_Title".GetLocalized(),
            OverviewIssueKind.TurnedOff => "Overview_TurnedOff_Title".GetLocalized(),
            OverviewIssueKind.PausedOnBattery => "Overview_Battery_Title".GetLocalized(),
            _ => "Overview_NotConfigured_Title".GetLocalized(),
        };
    }

    private string GetIssueText(OverviewIssue issue, OverviewInputs inputs)
    {
        return issue.Kind switch
        {
            OverviewIssueKind.BackupFailed => lastVersion == null
                ? "Overview_BackupFailed_TextNoBackup".GetLocalized()
                : Format("Overview_BackupFailed_Text", lastVersion.CreationDate.HumanizeDate()),
            OverviewIssueKind.BackupLocationUnavailable => "Overview_LocationUnavailable_Text".GetLocalized(),
            OverviewIssueKind.BackupOverdue => Format("Overview_Overdue_Text", inputs.RemindAfterDays ?? 0),
            OverviewIssueKind.NoBackupYet => "Overview_NoBackup_Text".GetLocalized(),
            OverviewIssueKind.ScheduleEmpty => "Overview_ScheduleEmpty_Text".GetLocalized(),
            OverviewIssueKind.FilesSkipped => Format("Overview_FilesSkipped_Text", (lastResult?.CompletedAt ?? DateTime.Now).HumanizeDate()),
            OverviewIssueKind.LowDiskSpace => Format("Overview_LowSpace_Text", inputs.FreeSpaceBytes.Bytes().Humanize()),
            OverviewIssueKind.TurnedOff => "Overview_TurnedOff_Text".GetLocalized(),
            OverviewIssueKind.PausedOnBattery => "Overview_Battery_Text".GetLocalized(),
            _ => "Overview_NotConfigured_Text".GetLocalized(),
        };
    }

    private string GetActionText(OverviewIssue issue, OverviewInputs inputs)
    {
        return issue.Kind switch
        {
            OverviewIssueKind.BackupFailed => GetFailureActionText(),
            OverviewIssueKind.BackupLocationUnavailable => "Overview_LocationUnavailable_Action".GetLocalized(),
            OverviewIssueKind.BackupOverdue => "Overview_Overdue_Action".GetLocalized(),
            OverviewIssueKind.NoBackupYet => "Overview_NoBackup_Action".GetLocalized(),
            OverviewIssueKind.ScheduleEmpty => "Overview_ScheduleEmpty_Action".GetLocalized(),
            OverviewIssueKind.FilesSkipped => Format("Overview_FilesSkipped_Action", lastResult?.FailedFileCount ?? 0),
            OverviewIssueKind.LowDiskSpace => "Overview_LowSpace_Action".GetLocalized(),
            OverviewIssueKind.TurnedOff => "Overview_TurnedOff_Action".GetLocalized(),
            _ => GetIssueText(issue, inputs),
        };
    }

    private string GetFailureActionText()
    {
        if (lastResult == null)
        {
            return "Overview_BackupFailed_Action_Unknown".GetLocalized();
        }

        return lastResult.FailureReason switch
        {
            BackupFailureReason.DeviceNotReady => "Overview_BackupFailed_Action_DeviceNotReady".GetLocalized(),
            BackupFailureReason.InsufficientDiskSpace => "Overview_BackupFailed_Action_DiskSpace".GetLocalized(),
            _ when lastResult.FailedFileCount > 0 => Format("Overview_BackupFailed_Action_Files", lastResult.FailedFileCount),
            _ when !string.IsNullOrWhiteSpace(lastResult.FailureMessage) => Format("Overview_BackupFailed_Action_Other", lastResult.FailureMessage),
            _ => "Overview_BackupFailed_Action_Unknown".GetLocalized(),
        };
    }

    private static string GetActionButtonText(OverviewAction action)
    {
        return action switch
        {
            OverviewAction.ViewFailedFiles => "Overview_Button_ViewFiles".GetLocalized(),
            OverviewAction.ManageBackups => "Overview_Button_ManageBackups".GetLocalized(),
            OverviewAction.ChangeBackupLocation => "Overview_Button_ChangeLocation".GetLocalized(),
            OverviewAction.SetSchedule => "Overview_Button_SetSchedule".GetLocalized(),
            OverviewAction.TurnOn => "Overview_Button_TurnOn".GetLocalized(),
            _ => "Overview_Button_BackUpNow".GetLocalized(),
        };
    }

    private IRelayCommand GetActionCommand(OverviewAction action)
    {
        return action switch
        {
            OverviewAction.ViewFailedFiles => ViewFailedFilesCommand,
            OverviewAction.ManageBackups => BrowseBackupsCommand,
            OverviewAction.ChangeBackupLocation => ChangeBackupLocationCommand,
            OverviewAction.SetSchedule => SetScheduleCommand,
            OverviewAction.TurnOn => TurnOnCommand,
            _ => BackUpNowCommand,
        };
    }

    private void ApplyLastBackup(VersionChangeStatistics? statistics)
    {
        HasLastBackup = lastVersion != null || lastResult != null;

        var when = lastResult?.CompletedAt ?? lastVersion?.CreationDate;
        LastBackupWhen = when?.HumanizeDate();

        var outcome = lastResult?.Outcome ?? BackupOutcome.Succeeded;
        LastBackupResultSeverity = outcome switch
        {
            BackupOutcome.Failed => OverviewSeverity.Error,
            BackupOutcome.CompletedWithErrors => OverviewSeverity.Warning,
            _ => OverviewSeverity.Protected,
        };
        LastBackupResultText = outcome switch
        {
            BackupOutcome.Failed => "Overview_Result_Failed".GetLocalized(),
            BackupOutcome.CompletedWithErrors => Format("Overview_Result_CompletedWithErrors", lastResult!.FailedFileCount),
            _ => "Overview_Result_Succeeded".GetLocalized(),
        };

        var versionBelongsToResult = lastVersion != null && outcome != BackupOutcome.Failed;
        LastBackupName = versionBelongsToResult ? lastVersion!.Title : null;
        HasLastBackupName = !string.IsNullOrWhiteSpace(LastBackupName);

        HasChangeStatistics = versionBelongsToResult && statistics != null;
        AddedFilesText = statistics == null ? null : Format("Overview_Changes_Added", statistics.Added);
        ModifiedFilesText = statistics == null ? null : Format("Overview_Changes_Modified", statistics.Modified);
        DeletedFilesText = statistics == null ? null : Format("Overview_Changes_Deleted", statistics.Deleted);
    }

    private void ApplyModeAndNextBackup()
    {
        var mode = configurationManager.TaskType switch
        {
            TaskType.Auto => "MainView_BackupMode_Automatic".GetLocalized(),
            TaskType.Schedule => "MainView_BackupMode_Scheduled".GetLocalized(),
            _ => "MainView_BackupMode_Manual".GetLocalized(),
        };

        if (configurationManager.TaskType == TaskType.Manual || scheduledBackupService == null)
        {
            ModeAndNextBackupText = mode;
            return;
        }

        ModeAndNextBackupText = Format("Overview_ModeAndNext", mode, scheduledBackupService.GetNextBackupDate().HumanizeDate());
    }

    private void ApplyLocation()
    {
        if (configurationManager.MediumType == MediaType.FileTransferServer)
        {
            LocationKindText = "Overview_Location_Ftp".GetLocalized();
            LocationPath = $"ftp://{configurationManager.FtpHost}{configurationManager.FtpFolder}";
        }
        else
        {
            LocationKindText = "Overview_Location_Local".GetLocalized();
            LocationPath = configurationManager.BackupFolder;
        }

        (LocationConnectionText, LocationConnectionSeverity) = isBackupLocationAvailable switch
        {
            true => ("Overview_Location_Available".GetLocalized(), OverviewSeverity.Protected),
            false => ("Overview_Location_Unavailable".GetLocalized(), OverviewSeverity.Error),
            null => ("Overview_Location_Checking".GetLocalized(), OverviewSeverity.Paused),
        };

        var freeBytes = GetFreeSpaceBytes();
        var inputs = BuildInputs();
        IsFreeSpaceLow = OverviewStatusEvaluator.IsLowOnDiskSpace(inputs);
        FreeSpaceText = freeBytes > 0
            ? Format("Overview_Location_Free", freeBytes.Bytes().Humanize())
            : "Overview_Location_FreeUnknown".GetLocalized();
        FreeSpaceHint = IsFreeSpaceLow
            ? Format("Overview_Location_LowHint", (inputs.RemindSpaceMegabytes!.Value * 1024d * 1024d).Bytes().Humanize())
            : null;
    }

    private double GetFreeSpaceBytes()
    {
        return double.TryParse(configurationManager.FreeSpace, NumberStyles.Float, CultureInfo.InvariantCulture, out var bytes) ? bytes : 0;
    }

    private static string Format(string resourceKey, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, resourceKey.GetLocalized() ?? resourceKey, args);
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
    private async Task BackUpNow()
    {
        await jobService.CreateBackupAsync("CreateBackup_Title_Manual".GetLocalized(), "", true);
    }

    [RelayCommand]
    private async Task ViewFailedFiles()
    {
        if (lastResult != null)
        {
            await presentationService.ShowLastBackupFailedFilesAsync(lastResult);
        }
    }

    [RelayCommand]
    private void BrowseBackups()
    {
        (navigationService ?? App.GetService<INavigationService>()).NavigateTo(BrowserPageKey);
    }

    [RelayCommand]
    private async Task ChangeBackupLocation()
    {
        if (await presentationService.ShowSwitchStorageWindowAsync())
        {
            isBackupLocationAvailable = null;
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task SetSchedule()
    {
        await presentationService.ShowScheduleEditorWindowAsync();
        await RefreshAsync();
    }

    [RelayCommand]
    private void TurnOn()
    {
        IsSystemEnabled = true;
    }

    [RelayCommand]
    private void CancelBackup()
    {
        jobService.Cancel();
    }

    public void ReportAction(ActionType action, bool silent)
    {
        EnqueueUi(() =>
        {
            currentAction = action;
            if (statusService.JobState == JobState.RUNNING)
            {
                ApplyStatus();
            }
        });
    }

    public void ReportState(JobState jobState)
    {
        EnqueueUi(async () =>
        {
            if (!systemToggleInFlight)
            {
                CanToggleSystem = jobState != JobState.RUNNING;
            }
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
                ApplyStatus();
                return;
            }

            NextBackupGridVisibility = true;
            ProgressGridVisibility = false;
            ApplyStatus();

            if (jobState is JobState.FINISHED or JobState.ERROR or JobState.CANCELED)
            {
                try
                {
                    isBackupLocationAvailable = null;
                    await RefreshAsync();
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Could not refresh the overview after a job.");
                }
            }
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
        EnqueueUi(() =>
        {
            action();
            return Task.CompletedTask;
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

    private void ApplySystemStatus(SystemStatus status)
    {
        systemStatus = status;
        SyncSystemEnabledFromConfiguration();
        ApplyStatus();
    }

    partial void OnIsSystemEnabledChanged(bool value)
    {
        _ = ApplySystemEnabledAsync(value);
    }

    private async Task ApplySystemEnabledAsync(bool enabled)
    {
        if (systemToggleInFlight)
        {
            return;
        }

        systemToggleInFlight = true;
        CanToggleSystem = false;
        try
        {
            if (enabled)
            {
                await orchestrationService.StartAsync(turnOn: true);
            }
            else
            {
                await orchestrationService.StopAsync(turnOff: true);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Could not change the backup system state.");
        }
        finally
        {
            systemToggleInFlight = false;
            SyncSystemEnabledFromConfiguration();
        }
    }

    private void SyncSystemEnabledFromConfiguration()
    {
        if (!systemToggleInFlight)
        {
            CanToggleSystem = statusService.JobState != JobState.RUNNING;
        }
        SetProperty(ref isSystemEnabled, configurationManager.DbStatus == "0", nameof(IsSystemEnabled));
    }
}
