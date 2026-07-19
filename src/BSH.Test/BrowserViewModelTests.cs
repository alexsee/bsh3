// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BSH.Test;

public class BrowserViewModelTests
{
    [Test]
    public async Task AddFolderToFavoritesPersistsSelectedFolder()
    {
        var favorites = new BrowserFavoritesService(new MemoryLocalSettingsService());
        var viewModel = CreateViewModel(favoritesService: favorites);
        viewModel.CurrentVersion = Version("1");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", DisplayName = "docs", FullPath = "source\\docs", IsFile = false };

        await viewModel.AddFolderToFavoritesCommand.ExecuteAsync(null);

        var saved = await favorites.GetFavoritesAsync();
        Assert.That(saved.Select(x => (x.Name, x.Path)), Is.EqualTo(new[] { ("docs", "source\\docs") }));
        Assert.That(viewModel.Favorites.Select(x => x.Name), Does.Contain("docs"));
    }

    [Test]
    public async Task FavoritesCanBeRenamedAndRemovedFromPersistedSettings()
    {
        var favorites = new BrowserFavoritesService(new MemoryLocalSettingsService());
        var favorite = new BrowserFavoriteItem { Name = "docs", Path = "source\\docs", IsUserFavorite = true };

        await favorites.AddFavoriteAsync(favorite);
        await favorites.RenameFavoriteAsync(favorite, "Documents");

        var renamed = await favorites.GetFavoritesAsync();
        Assert.That(renamed.Single().Name, Is.EqualTo("Documents"));

        await favorites.RemoveFavoriteAsync(favorite);

        Assert.That(await favorites.GetFavoritesAsync(), Is.Empty);
    }

    [Test]
    public async Task NavigateToPreviousVersionContainingSelectedFolderLoadsMatchingVersion()
    {
        var queryManager = new BrowserQueryManager
        {
            PreviousFolderVersion = "1"
        };
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", FullPath = "source\\docs", IsFile = false };

        await viewModel.NavigateToPreviousVersionCommand.ExecuteAsync(null);

        Assert.That(queryManager.PreviousFolderLookup, Is.EqualTo(("2", "source\\docs")));
        Assert.That(viewModel.CurrentVersion?.Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task DeleteSelectedContentConfirmsAndCallsDeleteSingleForFile()
    {
        var dialogService = new BrowserDialogService { ConfirmSelectedContentDelete = true };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.DeleteSelectedContentCommand.ExecuteAsync(null);

        Assert.That(dialogService.SelectedContentDeletePrompted, Is.True);
        Assert.That(jobService.DeleteSingleCalls, Is.EqualTo(new[] { ("report.txt", @"\source\docs\") }));
    }

    [Test]
    public async Task DeleteMultipleBackupsConfirmsSelectedVersionsAndRefreshes()
    {
        var dialogService = new BrowserDialogService { VersionsSelectedForDelete = ["1", "3"] };
        var jobService = new BrowserJobService();
        var queryManager = new BrowserQueryManager();
        var viewModel = CreateViewModel(queryManager, jobService, dialogService);
        viewModel.CurrentVersion = Version("2");

        await viewModel.DeleteMultipleBackupsCommand.ExecuteAsync(null);

        Assert.That(dialogService.MultiDeletePromptedVersions, Is.EqualTo(new[] { "2", "1", "3" }));
        Assert.That(jobService.DeleteBackupsCalls.Single(), Is.EqualTo(new[] { "1", "3" }));
        Assert.That(queryManager.GetVersionsCallCount, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void ShowFilePreviewCommand_IsEnabledOnlyForFileSelections()
    {
        var viewModel = CreateViewModel();
        viewModel.CurrentVersion = Version("2");

        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", FullPath = @"\source\docs\", IsFile = false };
        Assert.That(viewModel.ShowFilePreviewCommand.CanExecute(null), Is.False);

        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };
        Assert.That(viewModel.ShowFilePreviewCommand.CanExecute(null), Is.True);
    }

    [Test]
    public async Task ShowFilePreviewCommand_DelegatesToPreviewServiceForSelectedFile()
    {
        var previewService = new BrowserPreviewServiceFake();
        var viewModel = CreateViewModel(previewService: previewService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.ShowFilePreviewCommand.ExecuteAsync(null);

        Assert.That(previewService.PreviewCalls.Single(), Is.EqualTo(("2", "report.txt", @"\source\docs\")));
    }

    private static BrowserViewModel CreateViewModel(
        BrowserQueryManager? queryManager = null,
        BrowserJobService? jobService = null,
        BrowserDialogService? browserDialogService = null,
        BrowserFavoritesService? favoritesService = null,
        BrowserPreviewServiceFake? previewService = null)
    {
        queryManager ??= new BrowserQueryManager();
        favoritesService ??= new BrowserFavoritesService(new MemoryLocalSettingsService());

        return new BrowserViewModel(
            queryManager,
            jobService ?? new BrowserJobService(),
            new BrowserBackupService(),
            new BrowserPresentationService(),
            new BrowserContentService(queryManager, favoritesService),
            browserDialogService ?? new BrowserDialogService(),
            previewService ?? new BrowserPreviewServiceFake());
    }

    private static VersionDetails Version(string id) => new()
    {
        Id = id,
        CreationDate = new DateTime(2026, 1, int.Parse(id)),
        Sources = @"D:\source",
        Title = "v" + id,
        Description = string.Empty
    };

    private sealed class BrowserQueryManager : IQueryManager
    {
        public string? PreviousFolderVersion { get; set; }
        public (string Version, string Path)? PreviousFolderLookup { get; private set; }
        public int GetVersionsCallCount { get; private set; }

        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);

        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path)
        {
            PreviousFolderLookup = (startVersion, path);
            return Task.FromResult(PreviousFolderVersion);
        }

        public string GetFileNameFromDrive(FileTableRow file) => file.FileName;
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult((fileName, false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult<FileDetails>(null);
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult(folder);
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(Version("3"));
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(Version("3"));
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(3);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(Version("1"));
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(Version(id));

        public List<VersionDetails> GetVersions(bool desc = true)
        {
            GetVersionsCallCount++;
            return desc ? [Version("2"), Version("1"), Version("3")] : [Version("1"), Version("2"), Version("3")];
        }

        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }

    private sealed class BrowserJobService : IJobService
    {
        public List<(string FileFilter, string FolderFilter)> DeleteSingleCalls { get; } = [];
        public List<List<string>> DeleteBackupsCalls { get; } = [];
        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);
        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true) { DeleteBackupsCalls.Add(versions); return Task.CompletedTask; }
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true) { DeleteSingleCalls.Add((fileFilter, folderFilter)); return Task.CompletedTask; }
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword() => Task.FromResult(true);
        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
    }

    private sealed class BrowserBackupService : IBackupService
    {
        public Task<bool> CheckMedia(bool quickCheck = false) => Task.FromResult(true);
        public string GetPassword() => string.Empty;
        public bool HasPassword() => false;
        public void SetPassword(string password) { }
        public Task SetStableAsync(string version, bool stable) => Task.CompletedTask;
        public Task UpdateVersionAsync(string version, VersionDetails versionDetails) => Task.CompletedTask;
        public Task StartBackup(string title, string description, IJobReport jobReport, CancellationToken cancellationToken, bool fullBackup = false, string sources = "", bool silent = false) => Task.CompletedTask;
        public Task StartDelete(string version, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartDeleteSingle(string fileFilter, string pathFilter, IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartEdit(IJobReport jobReport, CancellationToken cancellationToken, bool silent = false) => Task.CompletedTask;
        public Task StartRestore(string version, string file, string destination, IJobReport jobReport, CancellationToken cancellationToken, FileOverwrite overwrite = FileOverwrite.Ask, bool silent = false) => Task.CompletedTask;
        public void UpdateDatabaseFile(string databaseFile) { }
    }

    private sealed class BrowserDialogService : IBrowserDialogService
    {
        public bool ConfirmSelectedContentDelete { get; set; }
        public bool SelectedContentDeletePrompted { get; private set; }
        public IReadOnlyList<string>? VersionsSelectedForDelete { get; set; }
        public IReadOnlyList<string> MultiDeletePromptedVersions { get; private set; } = [];

        public Task<bool> ShowDeleteSelectedContentWindowAsync(FileOrFolderItem item) { SelectedContentDeletePrompted = true; return Task.FromResult(ConfirmSelectedContentDelete); }
        public Task<IReadOnlyList<string>> ShowDeleteBackupsWindowAsync(IReadOnlyList<VersionDetails> versions) { MultiDeletePromptedVersions = versions.Select(x => x.Id).ToList(); return Task.FromResult(VersionsSelectedForDelete ?? []); }
        public Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite) => Task.FromResult<string?>(favorite.Name);
        public Task ShowFileDetailsAsync(FileDetails fileDetails) => Task.CompletedTask;
    }

    private sealed class BrowserPreviewServiceFake : IBrowserPreviewService
    {
        public List<(string VersionId, string FileName, string FilePath)> PreviewCalls { get; } = [];

        public Task PreviewFileAsync(string versionId, string fileName, string filePath)
        {
            PreviewCalls.Add((versionId, fileName, filePath));
            return Task.CompletedTask;
        }
    }

    private sealed class BrowserPresentationService : IPresentationService
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
        public Task<(bool, NewBackupViewModel)> ShowCreateBackupWindowAsync() => Task.FromResult((false, new NewBackupViewModel()));
        public Task<(bool, EditBackupViewModel)> ShowEditBackupWindowAsync(EditBackupViewModel backupViewModel) => Task.FromResult((false, backupViewModel));
        public Task<bool> ShowDeleteBackupWindowAsync() => Task.FromResult(false);
        public Task ShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
        public Task ShowFileExceptionsAsync(IReadOnlyCollection<FileExceptionEntry> files) => Task.CompletedTask;
        public Task ShowMainWindowAsync() => Task.CompletedTask;
        public Task ShowStatusWindowAsync() => Task.CompletedTask;
        public Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1) => Task.FromResult(ContentDialogResult.None);
        public Task ShowExcludeFileFolderWindowAsync() => Task.CompletedTask;
        public Task ShowScheduleEditorWindowAsync() => Task.CompletedTask;
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
