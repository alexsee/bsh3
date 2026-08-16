// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
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

    private static SettingsViewModel CreateViewModel(FakeConfigurationManager configuration)
    {
        return new SettingsViewModel(
            configuration,
            presentationService: null!,
            jobService: null!,
            queryManager: null!,
            backupTargetService: null!,
            switchStorageService: null!,
            orchestrationService: null!,
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
}
