// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
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

    private sealed class UnusedQueryManager : IQueryManager
    {
        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public string GetFileNameFromDrive(FileTableRow file) => file.FileName;
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult((fileName, false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult<FileDetails>(null);
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult(folder);
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(new VersionDetails());
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(new VersionDetails());
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(0);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(new VersionDetails());
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(new VersionDetails { Id = id });
        public List<VersionDetails> GetVersions(bool desc = true) => [];
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }
}
