// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class SettingsViewModelTests
{
    [Test]
    public void MigratedWinFormsOffSentinel_LeavesLowDiskReminderDisabled()
    {
        var configuration = new FakeConfigurationManager { RemindSpace = "-1" };
        var viewModel = CreateViewModel(configuration);

        viewModel.OnNavigatedTo(null);

        Assert.That(viewModel.EnableNotificationWhenDiskspaceLow, Is.False);
        Assert.That(viewModel.NotificationWhenDiskspaceLow, Is.GreaterThanOrEqualTo(0));
        Assert.That(configuration.RemindSpace, Is.EqualTo("-1"));
    }

    [Test]
    public void TurningOffLowDiskReminder_StoresWinFormsOffSentinel()
    {
        var configuration = new FakeConfigurationManager { RemindSpace = "10" };
        var viewModel = CreateViewModel(configuration);

        viewModel.OnNavigatedTo(null);
        Assert.That(viewModel.EnableNotificationWhenDiskspaceLow, Is.True);

        viewModel.EnableNotificationWhenDiskspaceLow = false;

        Assert.That(configuration.RemindSpace, Is.EqualTo("-1"));
    }

    [TestCase(TaskType.Auto, TaskType.Manual)]
    [TestCase(TaskType.Schedule, TaskType.Manual)]
    [TestCase(TaskType.Manual, TaskType.Auto)]
    [TestCase(TaskType.Manual, TaskType.Schedule)]
    [TestCase(TaskType.Auto, TaskType.Schedule)]
    public void ChangingTaskTypePersistsAndRefreshesAutomation(TaskType from, TaskType to)
    {
        var configurationManager = new FakeConfigurationManager { TaskType = from };
        var orchestrationService = new RecordingOrchestrationService();
        var viewModel = CreateViewModel(configurationManager, orchestrationService);

        viewModel.TaskType = from;
        viewModel.TaskType = to;

        Assert.That(configurationManager.TaskType, Is.EqualTo(to));
        Assert.That(orchestrationService.RefreshCalls, Is.EqualTo(1));
    }

    [Test]
    public void LoadingTaskTypeDoesNotRefreshAutomation()
    {
        var configurationManager = new FakeConfigurationManager { TaskType = TaskType.Auto };
        var orchestrationService = new RecordingOrchestrationService();
        var viewModel = CreateViewModel(configurationManager, orchestrationService);

        viewModel.TaskType = TaskType.Auto;

        Assert.That(configurationManager.TaskType, Is.EqualTo(TaskType.Auto));
        Assert.That(orchestrationService.RefreshCalls, Is.EqualTo(0));
    }

    private static SettingsViewModel CreateViewModel(FakeConfigurationManager configuration)
    {
        return CreateViewModel(configuration, orchestrationService: null!);
    }

    private static SettingsViewModel CreateViewModel(
        FakeConfigurationManager configurationManager,
        IOrchestrationService orchestrationService)
    {
        return new SettingsViewModel(
            configurationManager,
            presentationService: null!,
            jobService: null!,
            queryManager: null!,
            backupTargetService: null!,
            switchStorageService: null!,
            orchestrationService,
            new StubStartupLaunchAdapter(),
            new StubUpdateService());
    }

    private sealed class StubStartupLaunchAdapter : IStartupLaunchAdapter
    {
        public bool IsEnabled() => false;

        public bool TrySetEnabled(bool enabled) => true;
    }

    private sealed class StubUpdateService : IUpdateService
    {
        public Task InitializeAsync(Action onApplicationExitRequested) => Task.CompletedTask;

        public Task CheckAsync(bool notifyWhenUpToDate) => Task.CompletedTask;

        public Task MaybeCheckOnStartupAsync() => Task.CompletedTask;

        public Task<bool> GetAutoSearchEnabledAsync() => Task.FromResult(true);

        public Task SetAutoSearchEnabledAsync(bool enabled) => Task.CompletedTask;

        public Task<bool> GetDownloadBetaAsync() => Task.FromResult(false);

        public Task SetDownloadBetaAsync(bool enabled) => Task.CompletedTask;

        public Task<string> ResetUniqueUserIdAsync() => Task.FromResult(string.Empty);
    }

    private sealed class RecordingOrchestrationService : IOrchestrationService
    {
        public int RefreshCalls { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync(bool turnOn = false) => Task.CompletedTask;
        public Task StopAsync(bool turnOff = false) => Task.CompletedTask;

        public Task RefreshAutomationAsync()
        {
            RefreshCalls++;
            return Task.CompletedTask;
        }
    }
}
