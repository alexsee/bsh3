// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Runtime.Ports;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels.Windows;
using NUnit.Framework;

namespace BSH.Test;

public class ApplicationUpdateServiceTests
{
    [Test]
    public async Task DownloadBetaPreferenceIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(settings);

        Assert.That(await service.GetDownloadBetaAsync(), Is.False);

        await service.SetDownloadBetaAsync(true);

        Assert.That(await service.GetDownloadBetaAsync(), Is.True);
        Assert.That(await settings.ReadSettingAsync<bool>(ApplicationUpdateService.DownloadBetaSettingKey), Is.True);
    }

    [Test]
    public async Task AutoSearchPreferenceDefaultsToTrueAndIsPersisted()
    {
        var settings = new TestLocalSettingsService();
        var service = CreateService(settings);

        Assert.That(await service.GetAutoSearchEnabledAsync(), Is.True);

        await service.SetAutoSearchEnabledAsync(false);

        Assert.That(await service.GetAutoSearchEnabledAsync(), Is.False);
        Assert.That(await settings.ReadSettingAsync<bool>(ApplicationUpdateService.AutoSearchUpdatesSettingKey), Is.False);
    }

    [Test]
    public async Task CheckAsyncUsesStableFeedByDefault()
    {
        var started = new List<(string Url, bool Notify)>();
        var service = CreateService(startUpdateCheck: url => started.Add((url, true)));

        await service.CheckAsync(notifyWhenUpToDate: true);

        Assert.That(started, Has.Count.EqualTo(1));
        Assert.That(started[0].Url, Is.EqualTo(ApplicationUpdateService.StableUpdateFeedUrl));
    }

    [Test]
    public async Task CheckAsyncUsesBetaFeedWhenEnabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.DownloadBetaSettingKey, true);
        var started = new List<string>();
        var service = CreateService(settings, url => started.Add(url));

        await service.CheckAsync(notifyWhenUpToDate: true);

        Assert.That(started, Is.EqualTo(new[] { ApplicationUpdateService.BetaUpdateFeedUrl }));
    }

    [Test]
    public async Task MaybeCheckOnStartupAsyncSkipsWhenDisabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.AutoSearchUpdatesSettingKey, false);
        var started = new List<string>();
        var service = CreateService(settings, started.Add);

        await service.MaybeCheckOnStartupAsync();

        Assert.That(started, Is.Empty);
    }

    [Test]
    public async Task MaybeCheckOnStartupAsyncChecksWhenEnabled()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.AutoSearchUpdatesSettingKey, true);
        var started = new List<string>();
        var service = CreateService(settings, started.Add);

        await service.MaybeCheckOnStartupAsync();

        Assert.That(started, Is.EqualTo(new[] { ApplicationUpdateService.StableUpdateFeedUrl }));
    }

    [Test]
    public async Task ResetUniqueUserIdAsyncReplacesStoredId()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(ApplicationUpdateService.UniqueUserIdSettingKey, "old-id");
        var service = CreateService(settings);

        var newId = await service.ResetUniqueUserIdAsync();

        Assert.That(newId, Is.Not.EqualTo("old-id"));
        Assert.That(await settings.ReadSettingAsync<string>(ApplicationUpdateService.UniqueUserIdSettingKey), Is.EqualTo(newId));
    }

    [Test]
    public async Task WinUIStoredPasswordAdapterClearsPassword()
    {
        var settings = new TestLocalSettingsService();
        await settings.SaveSettingAsync(WinUIStoredPasswordAdapter.SettingKey, "encrypted-password");
        var adapter = new WinUIStoredPasswordAdapter(settings);

        await adapter.StorePasswordAsync(string.Empty);

        Assert.That(await settings.ReadSettingAsync<string>(WinUIStoredPasswordAdapter.SettingKey), Is.EqualTo(string.Empty));
    }

    [Test]
    public void MainWindowViewModelRoutesUpdateAndPasswordSupportActions()
    {
        var updates = new RecordingUpdateService();
        var passwords = new RecordingStoredPasswordAdapter();
        var viewModel = new MainWindowViewModel(
            presentationService: null,
            navigationService: null,
            updateService: updates,
            storedPasswordAdapter: passwords,
            buildNavigationItems: false);

        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.CheckForUpdates), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ClearStoredPassword), Is.True);
        Assert.That(viewModel.HandleSupportAction(MainWindowViewModel.SupportActionKeys.ResetUniqueUserId), Is.True);

        Assert.That(updates.CheckCalls, Is.EqualTo(1));
        Assert.That(updates.LastNotifyWhenUpToDate, Is.True);
        Assert.That(updates.ResetUniqueUserIdCalls, Is.EqualTo(1));
        Assert.That(passwords.StoreCalls, Is.EqualTo(1));
        Assert.That(passwords.LastStoredPassword, Is.EqualTo(string.Empty));
    }

    private static ApplicationUpdateService CreateService(
        ILocalSettingsService? localSettingsService = null,
        Action<string>? startUpdateCheck = null)
    {
        return new ApplicationUpdateService(
            localSettingsService ?? new TestLocalSettingsService(),
            presentationServiceFactory: () => throw new InvalidOperationException("Presentation is not used by these tests."),
            startUpdateCheck: startUpdateCheck ?? (_ => { }));
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

    private sealed class RecordingUpdateService : IUpdateService
    {
        public int CheckCalls { get; private set; }
        public bool? LastNotifyWhenUpToDate { get; private set; }
        public int ResetUniqueUserIdCalls { get; private set; }

        public Task InitializeAsync(Action onApplicationExitRequested) => Task.CompletedTask;

        public Task CheckAsync(bool notifyWhenUpToDate)
        {
            CheckCalls++;
            LastNotifyWhenUpToDate = notifyWhenUpToDate;
            return Task.CompletedTask;
        }

        public Task MaybeCheckOnStartupAsync() => Task.CompletedTask;
        public Task<bool> GetAutoSearchEnabledAsync() => Task.FromResult(true);
        public Task SetAutoSearchEnabledAsync(bool enabled) => Task.CompletedTask;
        public Task<bool> GetDownloadBetaAsync() => Task.FromResult(false);
        public Task SetDownloadBetaAsync(bool enabled) => Task.CompletedTask;

        public Task<string> ResetUniqueUserIdAsync()
        {
            ResetUniqueUserIdCalls++;
            return Task.FromResult("new-id");
        }
    }

    private sealed class RecordingStoredPasswordAdapter : IStoredPasswordAdapter
    {
        public int StoreCalls { get; private set; }
        public string? LastStoredPassword { get; private set; }

        public Task<string> GetPasswordAsync() => Task.FromResult(string.Empty);

        public Task StorePasswordAsync(string password)
        {
            StoreCalls++;
            LastStoredPassword = password;
            return Task.CompletedTask;
        }
    }
}
