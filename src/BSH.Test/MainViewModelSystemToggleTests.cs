// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using Microsoft.UI.Dispatching;
using NUnit.Framework;

namespace BSH.Test;

public class MainViewModelSystemToggleTests
{
    private DispatcherQueueController dispatcher;

    [SetUp]
    public void CreateDispatcher()
    {
        dispatcher = DispatcherQueueController.CreateOnDedicatedThread();
    }

    [TearDown]
    public async Task ShutDownDispatcher()
    {
        await dispatcher.ShutdownQueueAsync();
    }

    [Test]
    public async Task OverviewReflectsTheSavedSystemStateWithoutStartingIt()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "0" };
        var orchestration = new ScriptedOrchestration(configuration);
        var viewModel = Create(configuration, orchestration, new ToggleStatusService());

        Assert.That(viewModel.IsSystemEnabled, Is.True);
        Assert.That(viewModel.CanToggleSystem, Is.True);
        Assert.That(orchestration.Starts, Is.EqualTo(0));

        viewModel.ReportState(JobState.RUNNING);
        await WaitUntil(() => viewModel.CanToggleSystem == false);
        viewModel.ReportState(JobState.NOT_STARTED);
        await WaitUntil(() => viewModel.CanToggleSystem);
    }

    [Test]
    public async Task ASecondClickDoesNotStartAnotherOrchestrationCall()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "1" };
        var orchestration = new ScriptedOrchestration(configuration) { HoldStart = true };
        var viewModel = Create(configuration, orchestration, new ToggleStatusService());

        viewModel.IsSystemEnabled = true;

        Assert.That(orchestration.Starts, Is.EqualTo(1));
        Assert.That(viewModel.CanToggleSystem, Is.False);

        viewModel.IsSystemEnabled = false;
        Assert.That(orchestration.Stops, Is.EqualTo(0));

        orchestration.StartGate.TrySetResult();
        await WaitUntil(() => viewModel.CanToggleSystem && viewModel.IsSystemEnabled);
        Assert.That(orchestration.Starts, Is.EqualTo(1));
        Assert.That(orchestration.Stops, Is.EqualTo(0));
    }

    [Test]
    public void TurningTheSystemOffPersistsTheStoppedState()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "0" };
        var orchestration = new ScriptedOrchestration(configuration);
        var viewModel = Create(configuration, orchestration, new ToggleStatusService());

        viewModel.IsSystemEnabled = false;

        Assert.That(orchestration.Stops, Is.EqualTo(1));
        Assert.That(configuration.DbStatus, Is.EqualTo("1"));
        Assert.That(viewModel.IsSystemEnabled, Is.False);
        Assert.That(viewModel.CanToggleSystem, Is.True);
    }

    [Test]
    public void AFailedStartLeavesTheToggleUsable()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "1" };
        var orchestration = new ScriptedOrchestration(configuration)
        {
            StartError = new InvalidOperationException("start failed")
        };
        var viewModel = Create(configuration, orchestration, new ToggleStatusService());

        viewModel.IsSystemEnabled = true;

        Assert.That(orchestration.Starts, Is.EqualTo(1));
        Assert.That(viewModel.IsSystemEnabled, Is.False);
        Assert.That(viewModel.CanToggleSystem, Is.True);
    }

    [Test]
    public async Task OverviewUpdatesSurviveACallbackFailure()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "0" };
        var viewModel = Create(configuration, new ScriptedOrchestration(configuration), new ToggleStatusService());
        System.ComponentModel.PropertyChangedEventHandler failUpdate = (_, _) => throw new InvalidOperationException("ui");
        viewModel.PropertyChanged += failUpdate;

        viewModel.ReportState(JobState.RUNNING);
        await DrainDispatcherAsync();
        Assert.That(viewModel.ProgressGridVisibility, Is.False);

        viewModel.PropertyChanged -= failUpdate;
        viewModel.ReportState(JobState.RUNNING);
        await DrainDispatcherAsync();
        Assert.That(viewModel.ProgressGridVisibility, Is.True);
        Assert.That(viewModel.NextBackupGridVisibility, Is.False);
    }

    [Test]
    public async Task RunningJob_HidesActionsUntilItEnds()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "0" };
        var status = new ToggleStatusService { SystemStatus = SystemStatus.ACTIVATED };
        var viewModel = Create(configuration, new ScriptedOrchestration(configuration), status);
        Assert.That(viewModel.HasActionItems, Is.True, "no backup yet offers Back up now");

        status.JobState = JobState.RUNNING;
        viewModel.ReportState(JobState.RUNNING);
        await DrainDispatcherAsync();
        Assert.That(viewModel.HasActionItems, Is.False);
        Assert.That(viewModel.ActionItems, Is.Empty);

        status.JobState = JobState.CANCELED;
        viewModel.ReportState(JobState.CANCELED);
        await WaitUntil(() => viewModel.HasActionItems);
    }

    private async Task DrainDispatcherAsync()
    {
        var drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.That(dispatcher.DispatcherQueue.TryEnqueue(() => drained.SetResult()), Is.True);
        await drained.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task WrongStateBackupDevice_ShowsLocationWarning()
    {
        var configuration = new FakeConfigurationManager { DbStatus = "0" };
        var viewModel = Create(configuration, new ScriptedOrchestration(configuration),
            new ToggleStatusService { SystemStatus = SystemStatus.ACTIVATED }, new WrongStateBackupService());

        viewModel.OnNavigatedTo(null);

        await WaitUntil(() => viewModel.LocationConnectionSeverity == OverviewSeverity.Error);
        await DrainDispatcherAsync();
        Assert.That(viewModel.ActionItems.Any(item => item.Kind == OverviewIssueKind.BackupLocationUnavailable), Is.True);
    }

    private MainViewModel Create(
        IConfigurationManager configuration,
        IOrchestrationService orchestration,
        IStatusService status,
        IBackupService backupService = null)
    {
        return new MainViewModel(
            presentationService: null,
            statusService: status,
            queryManager: null,
            jobService: null,
            scheduledBackupService: null,
            configurationManager: configuration,
            orchestrationService: orchestration,
            dispatcherQueue: dispatcher.DispatcherQueue,
            backupService: backupService);
    }

    private sealed class WrongStateBackupService : IBackupService
    {
        public Task<bool> CheckMedia(bool quickCheck = false) => throw new DeviceContainsWrongStateException();
        public string GetPassword() => throw new NotSupportedException();
        public bool HasPassword() => throw new NotSupportedException();
        public void SetPassword(string password) => throw new NotSupportedException();
        public Task SetStableAsync(string version, bool stable) => throw new NotSupportedException();
        public Task UpdateVersionAsync(string version, VersionDetails details) => throw new NotSupportedException();
        public void UpdateDatabaseFile(string databaseFile) => throw new NotSupportedException();
        public Task StartBackup(string title, string description, IJobReport report, CancellationToken token, bool fullBackup = false, string sources = "", bool silent = false) => throw new NotSupportedException();
        public Task StartDelete(string version, IJobReport report, CancellationToken token, bool silent = false) => throw new NotSupportedException();
        public Task StartDeleteSingle(string fileFilter, string pathFilter, IJobReport report, CancellationToken token, bool silent = false, IReadOnlyList<int> versionIds = null) => throw new NotSupportedException();
        public Task StartEdit(IJobReport report, CancellationToken token, bool silent = false) => throw new NotSupportedException();
        public Task StartRestore(string version, string file, string destination, IJobReport report, CancellationToken token, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => throw new NotSupportedException();
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.That(condition(), Is.True);
    }

    private sealed class ScriptedOrchestration : IOrchestrationService
    {
        private readonly FakeConfigurationManager configuration;

        public ScriptedOrchestration(FakeConfigurationManager configuration)
        {
            this.configuration = configuration;
        }

        public int Starts { get; private set; }

        public int Stops { get; private set; }

        public bool HoldStart { get; set; }

        public Exception StartError { get; set; }

        public TaskCompletionSource StartGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task StartAsync(bool turnOn = false)
        {
            Starts++;
            if (StartError != null)
            {
                throw StartError;
            }

            if (HoldStart)
            {
                await StartGate.Task;
            }

            configuration.DbStatus = "0";
        }

        public Task StopAsync(bool turnOff = false)
        {
            Stops++;
            configuration.DbStatus = "1";
            return Task.CompletedTask;
        }

        public Task RefreshAutomationAsync() => Task.CompletedTask;
    }

    private sealed class ToggleStatusService : IStatusService
    {
        public JobState JobState { get; set; }

        public RequestOverwriteResult LastFileOverwriteChoice => RequestOverwriteResult.None;

        public string LastFileProgress { get; set; } = "";

        public Collection<FileExceptionEntry> LastFilesException { get; set; } = new Collection<FileExceptionEntry>();

        public int LastProgressCurrent { get; set; }

        public int LastProgressTotal { get; set; }

        public string LastStatusText { get; set; } = "";

        public string LastStatusTitle { get; set; } = "";

        public SystemStatus SystemStatus { get; set; }

        public void AddObserver(IStatusReport jobReport, bool triggerLastState = false)
        {
        }

        public bool IsTaskRunning() => false;

        public void RemoveObserver(IStatusReport jobReport)
        {
        }

        public void ReportAction(ActionType action, bool silent)
        {
        }

        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent)
        {
        }

        public void ReportFileProgress(string file)
        {
        }

        public void ReportProgress(int total, int current)
        {
        }

        public void ReportState(JobState jobState)
        {
        }

        public void ReportStatus(string title, string text)
        {
        }

        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
        {
            return Task.FromResult(RequestOverwriteResult.None);
        }

        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;

        public void SetSystemStatus(SystemStatus status)
        {
            SystemStatus = status;
        }

        public void ShowExceptionDialog()
        {
        }

        public void Initialize()
        {
        }
    }
}
