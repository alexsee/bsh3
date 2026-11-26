// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime.Ports;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using NUnit.Framework;
using Windows.System.Power;
using WinFormsBackupController = Brightbits.BSH.Main.BackupController;
using WinFormsWaitForMediaService = Brightbits.BSH.Main.WaitForMediaService;

namespace BSH.Test;

public class WinUiOrchestrationParityTests
{
    [Test]
    public async Task StartAsyncPausesAutomationWhenBatteryRuleApplies()
    {
        var configurationManager = new FakeConfigurationManager
        {
            IsConfigured = "1",
            DbStatus = "0",
            DeativateAutoBackupsWhenAkku = "1",
            TaskType = TaskType.Auto
        };
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            configurationManager,
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService { IsRunningOnBattery = true },
            new TestJobService());

        await service.StartAsync();

        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.PAUSED_DUE_TO_BATTERY));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task StartAsyncMonitorsPowerAndStopsAutomationWhenPowerIsDisconnected()
    {
        var powerStatusService = new TestPowerStatusService();
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                DeativateAutoBackupsWhenAkku = "1",
                TaskType = TaskType.Auto
            },
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            powerStatusService,
            new TestJobService());

        await service.StartAsync();
        powerStatusService.SetIsRunningOnBattery(true);

        Assert.That(powerStatusService.StartMonitoringCalls, Is.GreaterThanOrEqualTo(1));
        Assert.That(powerStatusService.StopMonitoringCalls, Is.EqualTo(0));
        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.PAUSED_DUE_TO_BATTERY));
    }

    [Test]
    public async Task RefreshAutomationAsyncDoesNotRestartSchedulesWhileBatteryPaused()
    {
        var configurationManager = new FakeConfigurationManager
        {
            IsConfigured = "1",
            DbStatus = "0",
            DeativateAutoBackupsWhenAkku = "1",
            TaskType = TaskType.Schedule
        };
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            configurationManager,
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService { IsRunningOnBattery = true },
            new TestJobService());

        await service.RefreshAutomationAsync();

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(0));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.PAUSED_DUE_TO_BATTERY));
    }

    [Test]
    public async Task RefreshAutomationAsyncRestartsSchedulesWhenNotOnBattery()
    {
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                DeativateAutoBackupsWhenAkku = "1",
                TaskType = TaskType.Schedule
            },
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService { IsRunningOnBattery = false },
            new TestJobService());

        await service.RefreshAutomationAsync();

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(1));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.ACTIVATED));
    }

    [Test]
    public async Task RefreshAutomationAsyncStopsThenStartsWhenTaskTypeIsManual()
    {
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                TaskType = TaskType.Manual
            },
            new TestStatusService(),
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService(),
            new TestJobService());

        await service.RefreshAutomationAsync();

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task RefreshAutomationAsyncDoesNothingWhenSystemIsDeactivated()
    {
        var statusService = new TestStatusService { SystemStatus = SystemStatus.DEACTIVATED };
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "1",
                DeativateAutoBackupsWhenAkku = "1",
                TaskType = TaskType.Schedule
            },
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService { IsRunningOnBattery = false },
            new TestJobService());

        await service.RefreshAutomationAsync();

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(0));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(0));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.DEACTIVATED));
    }

    [Test]
    public async Task StartAsyncMonitorsPowerAndRestartsAutomationWhenPowerIsReconnected()
    {
        var powerStatusService = new TestPowerStatusService
        {
            IsRunningOnBattery = true
        };
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var notifications = new TestNotificationService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                DeativateAutoBackupsWhenAkku = "1",
                TaskType = TaskType.Auto,
                RemindSpace = "10",
                FreeSpace = (9L * 1024L * 1024L).ToString()
            },
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            notifications,
            powerStatusService,
            new TestJobService());

        await service.StartAsync();
        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));

        powerStatusService.SetIsRunningOnBattery(false);

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(1));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.ACTIVATED));
        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task PowerChangeDoesNothingWhenSystemIsDeactivated()
    {
        var configurationManager = new FakeConfigurationManager
        {
            IsConfigured = "1",
            DbStatus = "0",
            DeativateAutoBackupsWhenAkku = "1",
            TaskType = TaskType.Auto
        };
        var powerStatusService = new TestPowerStatusService();
        var statusService = new TestStatusService();
        var scheduledBackupService = new TestScheduledBackupService();
        var service = new OrchestrationService(
            configurationManager,
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            powerStatusService,
            new TestJobService());

        await service.StartAsync();
        configurationManager.DbStatus = "1";
        statusService.SetSystemStatus(SystemStatus.DEACTIVATED);
        powerStatusService.SetIsRunningOnBattery(true);

        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(0));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.DEACTIVATED));
    }

    [Test]
    public async Task StartAsyncShowsLowDiskSpaceNotificationWhenConfigured()
    {
        var configurationManager = new FakeConfigurationManager
        {
            IsConfigured = "1",
            DbStatus = "0",
            RemindSpace = "10",
            FreeSpace = (9L * 1024L * 1024L).ToString()
        };
        var notifications = new TestNotificationService();
        var service = new OrchestrationService(
            configurationManager,
            new TestStatusService(),
            new TestScheduledBackupService(),
            new TestQueryManager(),
            notifications,
            new TestPowerStatusService(),
            new TestJobService());

        await service.StartAsync();

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(notifications.Payloads[0], Does.Contain("launch=\"action=settings\""));
    }

    [Test]
    public async Task StartAsyncShowsOutdatedBackupNotificationWhenConfigured()
    {
        var queryManager = new TestQueryManager
        {
            LastBackup = new VersionDetails { CreationDate = DateTime.Now.AddDays(-3) },
            Versions = new List<VersionDetails> { new() }
        };
        var notifications = new TestNotificationService();
        var service = new OrchestrationService(
            new FakeConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                RemindAfterDays = "1"
            },
            new TestStatusService(),
            new TestScheduledBackupService(),
            queryManager,
            notifications,
            new TestPowerStatusService(),
            new TestJobService());

        await service.StartAsync();

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(notifications.Payloads[0], Does.Contain("launch=\"action=overview\""));
    }

    [Test]
    public async Task StopAsyncCancelsRunningJob()
    {
        var jobService = new TestJobService();
        var scheduledBackupService = new TestScheduledBackupService();
        var statusService = new TestStatusService();
        var service = new OrchestrationService(
            new FakeConfigurationManager(),
            statusService,
            scheduledBackupService,
            new TestQueryManager(),
            new TestNotificationService(),
            new TestPowerStatusService(),
            jobService);

        await service.StopAsync();

        Assert.That(jobService.CancelCalls, Is.EqualTo(1));
        Assert.That(scheduledBackupService.StopCalls, Is.EqualTo(1));
        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.DEACTIVATED));
    }

    [TestCase(JobState.FINISHED, "INFO_BACKUP_SUCCESSFUL_TITLE", "action=overview")]
    [TestCase(JobState.ERROR, "INFO_BACKUP_UNSUCCESSFUL_TITLE", "action=overview")]
    public void StatusServiceShowsBackupCompletionNotifications(JobState state, string expectedTitleKey, string expectedLaunch)
    {
        var notifications = new TestNotificationService();
        var configurationManager = new FakeConfigurationManager
        {
            InfoBackupDone = "1"
        };
        var statusService = new StatusService(
            configurationManager,
            new FakePresentationService(),
            notifications);

        statusService.ReportAction(ActionType.Backup, silent: true);
        statusService.ReportState(state);

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(expectedTitleKey, Is.Not.Null.And.Not.Empty);
        Assert.That(notifications.Payloads[0], Does.Contain($"launch=\"{expectedLaunch}\""));
    }

    [TestCase("1", true, "1", true)]
    [TestCase("1", true, "0", false)]
    [TestCase("1", false, "0", true)]
    [TestCase("0", true, "1", false)]
    [TestCase("0", false, "1", false)]
    public async Task JobServicePromptsForMissingMediaAccordingToConfiguration(
        string medium, bool silent, string showWaitOnMediaAutoBackups, bool shouldPrompt)
    {
        var waitForMediaService = new TestWaitForMediaService { Result = true };
        using var jobService = new JobService(
            new TestBackupService(),
            new FakeConfigurationManager
            {
                Medium = medium,
                ShowWaitOnMediaAutoBackups = showWaitOnMediaAutoBackups
            },
            new TestStatusService(),
            new FakePresentationService(),
            new CompletionActionService(),
            new TestStoredPasswordAdapter(),
            () => waitForMediaService);

        var result = await jobService.CheckMediaAsync(ActionType.Backup, silent);

        Assert.That(result, Is.EqualTo(shouldPrompt));
        Assert.That(waitForMediaService.CancellationTokenSource is not null, Is.EqualTo(shouldPrompt));
        if (shouldPrompt)
        {
            jobService.Cancel();
            Assert.That(waitForMediaService.CancellationTokenSource.IsCancellationRequested, Is.True);
        }
    }

    [TestCase("1", false, true)]
    [TestCase("1", true, false)]
    [TestCase("0", false, false)]
    public async Task WinFormsPromptsForMissingMediaOnlyDuringInteractiveRemovableMediaChecks(
        string medium, bool silent, bool shouldPrompt)
    {
        var waitForMediaService = new TestWaitForMediaService { Result = true };
        ActionType? promptedAction = null;
        using var backupController = new WinFormsBackupController(
            new TestBackupService(),
            new FakeConfigurationManager { Medium = medium, ShowWaitOnMediaAutoBackups = "1" },
            () => false,
            (action, cancellationTokenSource) =>
            {
                promptedAction = action;
                return waitForMediaService.ExecuteAsync(cancellationTokenSource);
            });

        var result = await backupController.CheckMediaAsync(ActionType.Restore, silent);

        Assert.That(result, Is.EqualTo(shouldPrompt));
        Assert.That(promptedAction, Is.EqualTo(shouldPrompt ? ActionType.Restore : (ActionType?)null));
        Assert.That(waitForMediaService.CancellationTokenSource is not null, Is.EqualTo(shouldPrompt));
    }

    [Test]
    public async Task WinFormsMediaPollingRetriesUntilMediaIsAvailable()
    {
        using var cancellation = new CancellationTokenSource();
        var mediaChecks = new Queue<bool>([false, true]);
        var backupService = new TestBackupService { OnCheckMedia = () => Task.FromResult(mediaChecks.Dequeue()) };
        var waitForMedia = new WinFormsWaitForMediaService(backupService, cancellation);

        Assert.That(await waitForMedia.PollForMediaAsync(0), Is.True);
        Assert.That(mediaChecks, Is.Empty);
    }

    [Test]
    public async Task WinFormsMediaPollingReturnsFalseWhenMediaCheckFails()
    {
        using var cancellation = new CancellationTokenSource();
        var backupService = new TestBackupService
        {
            OnCheckMedia = () => Task.FromException<bool>(new InvalidOperationException())
        };
        var waitForMedia = new WinFormsWaitForMediaService(backupService, cancellation);

        Assert.That(await waitForMedia.PollForMediaAsync(0), Is.False);
    }

    [Test]
    public async Task WinFormsMediaPollingReturnsFalseWhenCanceledDuringMediaCheck()
    {
        using var cancellation = new CancellationTokenSource();
        var backupService = new TestBackupService
        {
            OnCheckMedia = () =>
            {
                cancellation.Cancel();
                return Task.FromResult(true);
            }
        };
        var waitForMedia = new WinFormsWaitForMediaService(backupService, cancellation);

        Assert.That(await waitForMedia.PollForMediaAsync(0), Is.False);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void WinFormsMediaWaitDisposesDialogWhenAlreadyCanceled()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var waitForMedia = new WinFormsWaitForMediaService(new TestBackupService(), cancellation);
        var openForms = System.Windows.Forms.Application.OpenForms.Count;

        var result = waitForMedia.ExecuteAsync();

        Assert.That(result.IsCompletedSuccessfully, Is.True);
        Assert.That(result.GetAwaiter().GetResult(), Is.False);
        Assert.That(System.Windows.Forms.Application.OpenForms.Count, Is.EqualTo(openForms));
    }

    [TestCase(BatteryStatus.Discharging, PowerSupplyStatus.NotPresent, true)]
    [TestCase(BatteryStatus.Idle, PowerSupplyStatus.NotPresent, true)]
    [TestCase(BatteryStatus.Charging, PowerSupplyStatus.Adequate, false)]
    [TestCase(BatteryStatus.NotPresent, PowerSupplyStatus.NotPresent, false)]
    public void PowerStatusServiceDetectsBatteryMode(BatteryStatus batteryStatus, PowerSupplyStatus powerSupplyStatus, bool expected)
    {
        Assert.That(PowerStatusService.DetermineIsRunningOnBattery(batteryStatus, powerSupplyStatus), Is.EqualTo(expected));
    }

    private sealed class TestStatusService : IStatusService
    {
        public JobState JobState { get; set; }
        public RequestOverwriteResult LastFileOverwriteChoice => RequestOverwriteResult.None;
        public string LastFileProgress { get; set; } = "";
        public Collection<FileExceptionEntry> LastFilesException { get; set; } = new();
        public int LastProgressCurrent { get; set; }
        public int LastProgressTotal { get; set; }
        public string LastStatusText { get; set; } = "";
        public string LastStatusTitle { get; set; } = "";
        public SystemStatus SystemStatus { get; set; }

        public void AddObserver(IStatusReport jobReport, bool triggerLastState = false) { }
        public bool IsTaskRunning() => JobState == JobState.RUNNING;
        public void RemoveObserver(IStatusReport jobReport) { }
        public void ReportAction(ActionType action, bool silent) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }
        public void ReportFileProgress(string file) { }
        public void ReportProgress(int total, int current) { }
        public void ReportState(JobState jobState) => JobState = jobState;
        public void ReportStatus(string title, string text) { }
        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public void SetSystemStatus(SystemStatus status) => SystemStatus = status;
        public void ShowExceptionDialog() { }
        public void Initialize() { }
    }

    private sealed class TestScheduledBackupService : IScheduledBackupService
    {
        public int StartCalls { get; private set; }
        public int StopCalls { get; private set; }

        public DateTime GetNextBackupDate() => DateTime.MaxValue;
        public Task<bool> HasScheduleEntriesAsync() => Task.FromResult(false);
        public Task StartAsync()
        {
            StartCalls++;
            return Task.CompletedTask;
        }
        public void Stop() => StopCalls++;
    }

    private sealed class TestJobService : FakeJobService
    {
        public int CancelCalls { get; private set; }

        public override void Cancel() => CancelCalls++;
    }

    private sealed class TestPowerStatusService : IPowerStatusService
    {
        private EventHandler? powerStatusChanged;

        public bool IsRunningOnBattery { get; set; }
        public int StartMonitoringCalls { get; private set; }
        public int StopMonitoringCalls { get; private set; }

        public void StartPowerStatusMonitoring(EventHandler powerStatusChanged)
        {
            this.powerStatusChanged -= powerStatusChanged;
            this.powerStatusChanged += powerStatusChanged;
            StartMonitoringCalls++;
        }

        public void StopPowerStatusMonitoring(EventHandler powerStatusChanged)
        {
            this.powerStatusChanged -= powerStatusChanged;
            StopMonitoringCalls++;
        }

        public void SetIsRunningOnBattery(bool isRunningOnBattery)
        {
            if (IsRunningOnBattery == isRunningOnBattery)
            {
                return;
            }

            IsRunningOnBattery = isRunningOnBattery;
            powerStatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class TestNotificationService : IAppNotificationService
    {
        public List<string> Payloads { get; } = new();

        public void Initialize() { }
        public void Activate(string? arguments) { }
        public bool Show(string payload)
        {
            Payloads.Add(payload);
            return true;
        }
        public void Unregister() { }
    }

    private sealed class TestQueryManager : FakeQueryManager
    {
        public VersionDetails? LastBackup { get; set; }
        public List<VersionDetails> Versions { get; set; } = new();

        public override Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public override Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public override string GetFileNameFromDrive(FileTableRow file) => "";
        public override Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public override Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(new FileDetails());
        public override Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public override Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(LastBackup);
        public override Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public override Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public override Task<int> GetNumberOfVersionsAsync() => Task.FromResult(Versions.Count);
        public override List<VersionDetails> GetVersions(bool desc = true) => Versions;
    }

    private sealed class TestWaitForMediaService : IWaitForMediaService
    {
        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public bool Result { get; init; }

        public Task<bool> ExecuteAsync(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
            return Task.FromResult(Result);
        }
    }

    private sealed class TestStoredPasswordAdapter : IStoredPasswordAdapter
    {
        public Task<string> GetPasswordAsync() => Task.FromResult("");
        public Task StorePasswordAsync(string password) => Task.CompletedTask;
    }

    private sealed class TestBackupService : FakeBackupService
    {
        public Func<Task<bool>> OnCheckMedia { get; init; } = () => Task.FromResult(false);

        public override Task<bool> CheckMedia(bool quickCheck = false) => OnCheckMedia();
    }
}
