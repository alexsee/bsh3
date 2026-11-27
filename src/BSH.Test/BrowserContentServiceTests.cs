// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSH.Test;

public class BrowserContentServiceTests
{
    [Test]
    public async Task GetFavoritesAsync_WhenVersionSourcesAreEmpty_FallsBackToConfiguredSourceFolders()
    {
        var service = CreateService(sourceFolder: @"Y:\MyFiles\source_1|Y:\MyFiles\source_2\");
        var version = new VersionDetails { Id = "2", Sources = "" };

        var favorites = await service.GetFavoritesAsync(version);

        Assert.That(favorites.Select(x => (x.Name, x.Path, x.IsUserFavorite)), Is.EqualTo(new[]
        {
            ("source_1", "source_1", false),
            ("source_2", "source_2", false)
        }));
    }

    [Test]
    public async Task GetFavoritesAsync_WhenVersionSourcesAreNull_FallsBackToConfiguredSourceFolders()
    {
        var service = CreateService(sourceFolder: @"D:\Documents|D:\Pictures");
        var version = new VersionDetails { Id = "1", Sources = null };

        var favorites = await service.GetFavoritesAsync(version);

        Assert.That(favorites.Select(x => x.Path), Is.EqualTo(new[] { "Documents", "Pictures" }));
    }

    [Test]
    public async Task GetFavoritesAsync_WhenVersionSourcesArePresent_DoesNotUseConfiguredFolders()
    {
        var service = CreateService(sourceFolder: @"D:\Configured");
        var version = new VersionDetails { Id = "1", Sources = @"E:\BackupSource\photos" };

        var favorites = await service.GetFavoritesAsync(version);

        Assert.That(favorites.Select(x => x.Path), Is.EqualTo(new[] { "photos" }));
    }

    private static BrowserContentService CreateService(string sourceFolder)
    {
        return new BrowserContentService(
            new UnusedQueryManager(),
            new BrowserFavoritesService(new MemoryLocalSettingsService()),
            new FakeConfigurationManager { SourceFolder = sourceFolder });
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

    private sealed class UnusedQueryManager : FakeQueryManager
    {
        public override Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(new VersionDetails());
        public override Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(new VersionDetails());
        public override Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(new VersionDetails());
        public override Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(new VersionDetails { Id = id });
    }
}
