// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading.Tasks;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using NUnit.Framework;

namespace BSH.Test;

public class BrowserViewPreferencesServiceTests
{
    [Test]
    public async Task InfoPaneVisibleDefaultsToFalseWhenUnset()
    {
        var service = new BrowserViewPreferencesService(new MemoryLocalSettingsService());

        Assert.That(await service.GetInfoPaneVisibleAsync(), Is.False);
    }

    [Test]
    public async Task InfoPaneVisibleIsPersistedWhenChanged()
    {
        var settings = new MemoryLocalSettingsService();
        var service = new BrowserViewPreferencesService(settings);

        await service.SetInfoPaneVisibleAsync(true);

        Assert.That(await service.GetInfoPaneVisibleAsync(), Is.True);
        Assert.That(await settings.ReadSettingAsync<bool>(BrowserViewPreferencesService.InfoPaneVisibleSettingsKey), Is.True);

        await service.SetInfoPaneVisibleAsync(false);

        Assert.That(await service.GetInfoPaneVisibleAsync(), Is.False);
        Assert.That(await settings.ReadSettingAsync<bool>(BrowserViewPreferencesService.InfoPaneVisibleSettingsKey), Is.False);
    }

    [Test]
    public async Task InfoPaneVisibleIsRestoredFromExistingSettings()
    {
        var settings = new MemoryLocalSettingsService();
        await settings.SaveSettingAsync(BrowserViewPreferencesService.InfoPaneVisibleSettingsKey, true);

        var service = new BrowserViewPreferencesService(settings);

        Assert.That(await service.GetInfoPaneVisibleAsync(), Is.True);
    }

    private sealed class MemoryLocalSettingsService : ILocalSettingsService
    {
        private readonly Dictionary<string, object?> settings = [];

        public Task<T?> ReadSettingAsync<T>(string key)
        {
            if (!settings.TryGetValue(key, out var value))
            {
                return Task.FromResult(default(T));
            }

            return Task.FromResult((T?)value);
        }

        public Task SaveSettingAsync<T>(string key, T value)
        {
            settings[key] = value;
            return Task.CompletedTask;
        }
    }
}
