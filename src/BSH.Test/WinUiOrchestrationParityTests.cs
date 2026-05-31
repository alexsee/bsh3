// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using Windows.System.Power;
using Windows.UI.Popups;

namespace BSH.Test;

public class WinUiOrchestrationParityTests
{
    [Test]
    public async Task StartAsyncPausesAutomationWhenBatteryRuleApplies()
    {
        var configurationManager = new TestConfigurationManager
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
            new TestPowerStatusService { IsRunningOnBattery = true });

        await service.StartAsync();

        Assert.That(statusService.SystemStatus, Is.EqualTo(SystemStatus.PAUSED_DUE_TO_BATTERY));
        Assert.That(scheduledBackupService.StartCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task StartAsyncShowsLowDiskSpaceNotificationWhenConfigured()
    {
        var configurationManager = new TestConfigurationManager
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
            new TestPowerStatusService());

        await service.StartAsync();

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(notifications.Payloads[0], Does.Contain("Not enough disk space"));
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
            new TestConfigurationManager
            {
                IsConfigured = "1",
                DbStatus = "0",
                RemindAfterDays = "1"
            },
            new TestStatusService(),
            new TestScheduledBackupService(),
            queryManager,
            notifications,
            new TestPowerStatusService());

        await service.StartAsync();

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(notifications.Payloads[0], Does.Contain("Backup outdated"));
    }

    [TestCase(JobState.FINISHED, "Backup successful")]
    [TestCase(JobState.ERROR, "Backup with errors finished")]
    public void StatusServiceShowsBackupCompletionNotifications(JobState state, string expectedTitle)
    {
        var notifications = new TestNotificationService();
        var configurationManager = new TestConfigurationManager
        {
            InfoBackupDone = "1"
        };
        var statusService = new StatusService(
            configurationManager,
            new TestPresentationService(),
            notifications);

        statusService.ReportAction(ActionType.Backup, silent: true);
        statusService.ReportState(state);

        Assert.That(notifications.Payloads, Has.Count.EqualTo(1));
        Assert.That(notifications.Payloads[0], Does.Contain(expectedTitle));
    }

    [Test]
    public async Task JobRuntimeCanPromptForMediaDuringSilentConfiguredBackups()
    {
        var waitWasRequested = false;
        var runtime = new JobRuntime(
            new TestBackupService { IsMediaAvailable = false },
            () => false,
            silent => silent ? MediaWaitMode.PromptUser : MediaWaitMode.None,
            async (_, waitMode, cancellationTokenSource) =>
            {
                Assert.That(waitMode, Is.EqualTo(MediaWaitMode.PromptUser));
                waitWasRequested = true;
                cancellationTokenSource.Cancel();
                await Task.Yield();
                return false;
            },
            () => Task.FromResult(true));

        var result = await runtime.CheckMediaAsync(ActionType.Backup, silent: true);

        Assert.That(result, Is.False);
        Assert.That(waitWasRequested, Is.True);
        Assert.That(runtime.IsCancellationRequested, Is.True);
    }

    [TestCase(BatteryStatus.Discharging, PowerSupplyStatus.NotPresent, true)]
    [TestCase(BatteryStatus.Idle, PowerSupplyStatus.NotPresent, true)]
    [TestCase(BatteryStatus.Charging, PowerSupplyStatus.Adequate, false)]
    [TestCase(BatteryStatus.NotPresent, PowerSupplyStatus.NotPresent, false)]
    public void PowerStatusServiceDetectsBatteryMode(BatteryStatus batteryStatus, PowerSupplyStatus powerSupplyStatus, bool expected)
    {
        Assert.That(PowerStatusService.DetermineIsRunningOnBattery(batteryStatus, powerSupplyStatus), Is.EqualTo(expected));
    }

    private sealed class TestConfigurationManager : IConfigurationManager
    {
        public string AutoBackup { get; set; } = "";
        public string BackupFolder { get; set; } = "";
        public string BackupSize { get; set; } = "";
        public int Compression { get; set; }
        public string DbStatus { get; set; } = "";
        public string DBVersion { get; set; } = "";
        public string DeativateAutoBackupsWhenAkku { get; set; } = "";
        public string DoPastBackups { get; set; } = "";
        public int Encrypt { get; set; }
        public string EncryptPassMD5 { get; set; } = "";
        public string ExcludeCompression { get; set; } = "";
        public string ExcludeFile { get; set; } = "";
        public string ExcludeFileBigger { get; set; } = "";
        public string ExcludeFileTypes { get; set; } = "";
        public string ExcludeFolder { get; set; } = "";
        public string ExcludeMask { get; set; } = "";
        public string FreeSpace { get; set; } = "";
        public string FtpCoding { get; set; } = "";
        public string FtpEncryptionMode { get; set; } = "";
        public string FtpFolder { get; set; } = "";
        public string FtpHost { get; set; } = "";
        public string FtpPass { get; set; } = "";
        public string FtpPort { get; set; } = "";
        public string FtpSslProtocols { get; set; } = "";
        public string FtpUser { get; set; } = "";
        public string InfoBackupDone { get; set; } = "";
        public string IntervallAutoHourBackups { get; set; } = "";
        public string IntervallDelete { get; set; } = "";
        public string IsConfigured { get; set; } = "";
        public string LastBackupDone { get; set; } = "";
        public string LastVersionDate { get; set; } = "";
        public string MediaVolumeSerial { get; set; } = "";
        public string Medium { get; set; } = "";
        public MediaType MediumType { get; set; }
        public string OldBackupPrevent { get; set; } = "";
        public string RemindAfterDays { get; set; } = "";
        public string RemindSpace { get; set; } = "";
        public string ScheduleFullBackup { get; set; } = "";
        public string ShowLocalizedPath { get; set; } = "";
        public string ShowWaitOnMediaAutoBackups { get; set; } = "";
        public string SourceFolder { get; set; } = "";
        public TaskType TaskType { get; set; }
        public string UNCPassword { get; set; } = "";
        public string UNCUsername { get; set; } = "";

        public Task InitializeAsync() => Task.CompletedTask;
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
        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync()
        {
            StartCalls++;
            return Task.CompletedTask;
        }
        public void Stop() => StopCalls++;
    }

    private sealed class TestPowerStatusService : IPowerStatusService
    {
        public bool IsRunningOnBattery { get; set; }
    }

    private sealed class TestNotificationService : IAppNotificationService
    {
        public List<string> Payloads { get; } = new();

        public void Initialize() { }
        public NameValueCollection ParseArguments(string arguments) => new();
        public bool Show(string payload)
        {
            Payloads.Add(payload);
            return true;
        }
        public void Unregister() { }
    }

    private sealed class TestQueryManager : IQueryManager
    {
        public VersionDetails? LastBackup { get; set; }
        public List<VersionDetails> Versions { get; set; } = new();

        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public string GetFileNameFromDrive(FileTableRow file) => "";
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(new FileDetails());
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(LastBackup);
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(Versions.Count);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult<VersionDetails>(null);
        public List<VersionDetails> GetVersions(bool desc = true) => Versions;
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }

    private sealed class TestBackupService : IBackupService
    {
        public bool IsMediaAvailable { get; set; }

        public Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(IsMediaAvailable);
        public string GetPassword() => "";
        public bool HasPassword() => false;
        public void SetPassword(string password) { }
        public Task SetStableAsync(string version, bool stable) => Task.CompletedTask;
        public Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;
        public Task StartBackup(string title, string description, ref IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false) => Task.CompletedTask;
        public Task StartDelete(string version, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartDeleteSingle(string fileFilter, string pathFilter, ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartEdit(ref IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartRestore(string version, string file, string destination, ref IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => Task.CompletedTask;
        public void UpdateDatabaseFile(string databaseFile) { }
    }

    private sealed class TestPresentationService : IPresentationService
    {
        public Task CloseBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task CloseMainWindowAsync() => Task.CompletedTask;
        public Task<TaskCompleteAction> CloseStatusWindowAsync() => Task.FromResult(TaskCompleteAction.NoAction);
        public Task OpenCurrentEventLogAsync() => Task.CompletedTask;
        public Task OpenHelpSupportAsync() => Task.CompletedTask;
        public Task<(string? password, bool persist)> RequestPasswordAsync() => Task.FromResult<(string?, bool)>((null, false));
        public Task<RequestOverwriteResult> RequestOverwriteAsync(FileTableRow localFile, FileTableRow remoteFile) => Task.FromResult(RequestOverwriteResult.None);
        public Task ResetConfigurationAsync() => Task.CompletedTask;
        public Task ShowAboutWindowAsync() => Task.CompletedTask;
        public Task ShowBackupBrowserWindowAsync() => Task.CompletedTask;
        public Task<(bool, BSH.MainApp.ViewModels.Windows.NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new BSH.MainApp.ViewModels.Windows.NewBackupViewModel()));
        public Task<(bool, BSH.MainApp.ViewModels.Windows.EditBackupViewModel)> ShowEditBackupWindowAsync(BSH.MainApp.ViewModels.Windows.EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.None);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
    }
}
