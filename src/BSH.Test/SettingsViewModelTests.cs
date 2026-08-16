// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class SettingsViewModelTests
{
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
            startupLaunchAdapter: null!,
            updateService: null!);
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
