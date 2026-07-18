// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels.Windows;
using NUnit.Framework;

namespace BSH.Test;

public class AppExtrasServiceTests
{
    [Test]
    public async Task ExitApplicationAsyncStopsOrchestrationBeforeExit()
    {
        var orchestration = new TestOrchestrationService();
        var exitCalls = 0;
        var service = CreateService(orchestrationService: orchestration, exitApplication: () => exitCalls++);

        await service.ExitApplicationAsync();

        Assert.That(orchestration.StopCalls, Is.EqualTo(1));
        Assert.That(exitCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task ClearStoredPasswordAsyncRemovesPersistedPassword()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(AppExtrasService.BackupPasswordSettingKey, "encrypted-password");
        var service = CreateService(localSettingsService: settings);

        await service.ClearStoredPasswordAsync();

        Assert.That(await settings.ReadSettingAsync<string>(AppExtrasService.BackupPasswordSettingKey), Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task ResetUniqueUserIdAsyncReplacesStoredId()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(AppExtrasService.UniqueUserIdSettingKey, "old-id");
        var service = CreateService(localSettingsService: settings);

        var newId = await service.ResetUniqueUserIdAsync();

        Assert.That(newId, Is.Not.EqualTo("old-id"));
        Assert.That(await settings.ReadSettingAsync<string>(AppExtrasService.UniqueUserIdSettingKey), Is.EqualTo(newId));
    }

    [Test]
    public async Task GetOrCreateUniqueUserIdAsyncCreatesIdWhenMissing()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(localSettingsService: settings);

        var first = await service.GetOrCreateUniqueUserIdAsync();
        var second = await service.GetOrCreateUniqueUserIdAsync();

        Assert.That(first, Is.Not.Null.And.Not.Empty);
        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public async Task DownloadBetaPreferenceIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(localSettingsService: settings);

        Assert.That(await service.GetDownloadBetaAsync(), Is.False);

        await service.SetDownloadBetaAsync(true);

        Assert.That(await service.GetDownloadBetaAsync(), Is.True);
        Assert.That(await settings.ReadSettingAsync<bool>(AppExtrasService.DownloadBetaSettingKey), Is.True);
    }

    [Test]
    public async Task AutoSearchUpdatesPreferenceDefaultsToTrueAndIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(localSettingsService: settings);

        Assert.That(await service.GetAutoSearchUpdatesAsync(), Is.True);

        await service.SetAutoSearchUpdatesAsync(false);

        Assert.That(await service.GetAutoSearchUpdatesAsync(), Is.False);
        Assert.That(await settings.ReadSettingAsync<bool>(AppExtrasService.AutoSearchUpdatesSettingKey), Is.False);
    }

    [Test]
    public async Task CheckForUpdatesAsyncUsesStableFeedByDefault()
    {
        var updater = new TestUpdateCheckAdapter();
        var service = CreateService(updateCheckAdapter: updater);

        await service.CheckForUpdatesAsync();

        Assert.That(updater.StartedChecks, Has.Count.EqualTo(1));
        Assert.That(updater.StartedChecks[0].Url, Is.EqualTo(AppExtrasService.StableUpdateFeedUrl));
        Assert.That(updater.StartedChecks[0].NotifyWhenUpToDate, Is.True);
    }

    [Test]
    public async Task CheckForUpdatesAsyncUsesBetaFeedWhenEnabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(AppExtrasService.DownloadBetaSettingKey, true);
        var updater = new TestUpdateCheckAdapter();
        var service = CreateService(localSettingsService: settings, updateCheckAdapter: updater);

        await service.CheckForUpdatesAsync();

        Assert.That(updater.StartedChecks, Has.Exactly(1).EqualTo((AppExtrasService.BetaUpdateFeedUrl, true)));
    }

    [Test]
    public async Task MaybeCheckForUpdatesOnStartupAsyncSkipsWhenDisabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(AppExtrasService.AutoSearchUpdatesSettingKey, false);
        var updater = new TestUpdateCheckAdapter();
        var service = CreateService(localSettingsService: settings, updateCheckAdapter: updater);

        await service.MaybeCheckForUpdatesOnStartupAsync();

        Assert.That(updater.StartedChecks, Is.Empty);
    }

    [Test]
    public async Task MaybeCheckForUpdatesOnStartupAsyncChecksSilentlyWhenEnabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(AppExtrasService.AutoSearchUpdatesSettingKey, true);
        var updater = new TestUpdateCheckAdapter();
        var service = CreateService(localSettingsService: settings, updateCheckAdapter: updater);

        await service.MaybeCheckForUpdatesOnStartupAsync();

        Assert.That(updater.StartedChecks, Has.Count.EqualTo(1));
        Assert.That(updater.StartedChecks[0].NotifyWhenUpToDate, Is.False);
    }

    [Test]
    public void TrySetLaunchAtStartupEnablesAndDisablesThroughAdapter()
    {
        var startup = new TestStartupLaunchAdapter();
        var service = CreateService(startupLaunchAdapter: startup);

        Assert.That(service.IsLaunchAtStartupEnabled(), Is.False);

        Assert.That(service.TrySetLaunchAtStartup(true), Is.True);
        Assert.That(service.IsLaunchAtStartupEnabled(), Is.True);
        Assert.That(startup.EnabledCommand, Does.Contain("-delayedstart"));

        Assert.That(service.TrySetLaunchAtStartup(false), Is.True);
        Assert.That(service.IsLaunchAtStartupEnabled(), Is.False);
    }

    [Test]
    public void TrySetLaunchAtStartupReturnsFalseWhenAdapterThrows()
    {
        var startup = new TestStartupLaunchAdapter { ThrowOnWrite = true };
        var service = CreateService(startupLaunchAdapter: startup);

        Assert.That(service.TrySetLaunchAtStartup(true), Is.False);
    }

    [Test]
    public async Task MainWindowViewModelRoutesAppExtraSupportActions()
    {
        var extras = new RecordingAppExtrasService();
        var viewModel = new MainWindowViewModel(
            presentationService: null,
            navigationService: null,
            appExtrasService: extras,
            buildNavigationItems: false);

        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.CheckForUpdates), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ClearStoredPassword), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ResetUniqueUserId), Is.True);

        Assert.That(extras.CheckForUpdatesCalls, Is.EqualTo(1));
        Assert.That(extras.ClearStoredPasswordCalls, Is.EqualTo(1));
        Assert.That(extras.ResetUniqueUserIdCalls, Is.EqualTo(1));
        await Task.CompletedTask;
    }

    private static AppExtrasService CreateService(
        ILocalSettingsService? localSettingsService = null,
        IOrchestrationService? orchestrationService = null,
        IStartupLaunchAdapter? startupLaunchAdapter = null,
        IUpdateCheckAdapter? updateCheckAdapter = null,
        Action? exitApplication = null)
    {
        return new AppExtrasService(
            localSettingsService ?? new TestLocalSettingsService(),
            orchestrationService ?? new TestOrchestrationService(),
            startupLaunchAdapter ?? new TestStartupLaunchAdapter(),
            updateCheckAdapter ?? new TestUpdateCheckAdapter(),
            () => Environment.ProcessPath ?? "BSH.MainApp.exe",
            exitApplication ?? (() => { }));
    }

    private sealed class TestLocalSettingsService : ILocalSettingsService
    {
        private readonly Dictionary<string, object?> values = new();

        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (!values.TryGetValue(key, out var value) || value is null)
            {
                return Task.FromResult<T?>(default);
            }

            if (value is T typed)
            {
                return Task.FromResult<T?>(typed);
            }

            return Task.FromResult<T?>((T)Convert.ChangeType(value, typeof(T)));
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            values[key] = value;
            return Task.CompletedTask;
        }
    }

    private sealed class TestOrchestrationService : IOrchestrationService
    {
        public int StopCalls { get; private set; }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task StartAsync(bool turnOn = false) => Task.CompletedTask;
        public Task RefreshAutomationAsync() => Task.CompletedTask;

        public Task StopAsync(bool turnOff = false)
        {
            StopCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestStartupLaunchAdapter : IStartupLaunchAdapter
    {
        public bool ThrowOnWrite { get; set; }
        public string? EnabledCommand { get; private set; }

        public bool IsEnabled(string valueName) => !string.IsNullOrEmpty(EnabledCommand);

        public void SetEnabled(string valueName, string command)
        {
            if (ThrowOnWrite)
            {
                throw new UnauthorizedAccessException();
            }

            EnabledCommand = command;
        }

        public void Disable(string valueName)
        {
            if (ThrowOnWrite)
            {
                throw new UnauthorizedAccessException();
            }

            EnabledCommand = null;
        }
    }

    private sealed class TestUpdateCheckAdapter : IUpdateCheckAdapter
    {
        public List<(string Url, bool NotifyWhenUpToDate)> StartedChecks { get; } = [];

        public List<string> StartedUrls => StartedChecks.ConvertAll(check => check.Url);

        public void Start(string feedUrl, bool notifyWhenUpToDate = false) =>
            StartedChecks.Add((feedUrl, notifyWhenUpToDate));
    }

    private sealed class RecordingAppExtrasService : IAppExtrasService
    {
        public int CheckForUpdatesCalls { get; private set; }
        public int ClearStoredPasswordCalls { get; private set; }
        public int ResetUniqueUserIdCalls { get; private set; }

        public bool IsLaunchAtStartupEnabled() => false;
        public bool TrySetLaunchAtStartup(bool enabled) => true;
        public Task<bool> GetDownloadBetaAsync() => Task.FromResult(false);
        public Task SetDownloadBetaAsync(bool enabled) => Task.CompletedTask;
        public Task<bool> GetAutoSearchUpdatesAsync() => Task.FromResult(true);
        public Task SetAutoSearchUpdatesAsync(bool enabled) => Task.CompletedTask;
        public Task CheckForUpdatesAsync()
        {
            CheckForUpdatesCalls++;
            return Task.CompletedTask;
        }

        public Task MaybeCheckForUpdatesOnStartupAsync() => Task.CompletedTask;
        public Task ClearStoredPasswordAsync()
        {
            ClearStoredPasswordCalls++;
            return Task.CompletedTask;
        }

        public Task<string> ResetUniqueUserIdAsync()
        {
            ResetUniqueUserIdCalls++;
            return Task.FromResult("new-id");
        }

        public Task<string> GetOrCreateUniqueUserIdAsync() => Task.FromResult("id");
        public Task ExitApplicationAsync() => Task.CompletedTask;
    }
}
