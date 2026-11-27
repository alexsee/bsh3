// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using BSH.Test.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSH.Test;

public class BrowserViewModelTests
{
    [Test]
    public async Task VersionStatisticsReusePendingAndCompletedRequestsPerVersion()
    {
        var pending = new TaskCompletionSource<VersionChangeStatistics>();
        var queryManager = new BrowserQueryManager { StatisticsTask = pending.Task };
        var viewModel = CreateViewModel(queryManager);

        var first = viewModel.GetVersionChangeStatisticsAsync("1");
        var repeated = viewModel.GetVersionChangeStatisticsAsync("1");
        pending.SetResult(new VersionChangeStatistics(1, 4, 5));

        Assert.That(await first, Is.EqualTo(new VersionChangeStatistics(1, 4, 5)));
        Assert.That(await repeated, Is.EqualTo(new VersionChangeStatistics(1, 4, 5)));
        queryManager.StatisticsTask = Task.FromResult(new VersionChangeStatistics(2, 0, 0));
        Assert.That(await viewModel.GetVersionChangeStatisticsAsync("1"), Is.EqualTo(new VersionChangeStatistics(1, 4, 5)));
        Assert.That(await viewModel.GetVersionChangeStatisticsAsync("2"), Is.EqualTo(new VersionChangeStatistics(2, 0, 0)));
        Assert.That(queryManager.StatisticsCalls, Is.EqualTo(new[] { "1", "2" }));
    }

    [Test]
    public async Task RefreshInvalidatesVersionStatistics()
    {
        var queryManager = new BrowserQueryManager { StatisticsTask = Task.FromResult(new VersionChangeStatistics(1, 4, 5)) };
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source" });
        await viewModel.GetVersionChangeStatisticsAsync("2");

        queryManager.StatisticsTask = Task.FromResult(new VersionChangeStatistics(1, 4, 0));
        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.That(await viewModel.GetVersionChangeStatisticsAsync("2"), Is.EqualTo(new VersionChangeStatistics(1, 4, 0)));
        Assert.That(queryManager.StatisticsCalls, Is.EqualTo(new[] { "2", "2" }));
    }

    [Test]
    public async Task FailedVersionStatisticsCanBeRetried()
    {
        var queryManager = new BrowserQueryManager
        {
            StatisticsTask = Task.FromException<VersionChangeStatistics>(new InvalidOperationException("Database unavailable"))
        };
        var viewModel = CreateViewModel(queryManager);
        Assert.ThrowsAsync<InvalidOperationException>(async () => await viewModel.GetVersionChangeStatisticsAsync("2"));

        queryManager.StatisticsTask = Task.FromResult(new VersionChangeStatistics(1, 4, 5));

        Assert.That(await viewModel.GetVersionChangeStatisticsAsync("2"), Is.EqualTo(new VersionChangeStatistics(1, 4, 5)));
        Assert.That(queryManager.StatisticsCalls, Is.EqualTo(new[] { "2", "2" }));
    }

    [Test]
    public async Task InfoPaneShowsCurrentFolderWhenNoBrowserItemIsSelected()
    {
        var viewModel = CreateViewModel();
        viewModel.CurrentVersion = Version("2");
        viewModel.ToggleInfoPane = true;

        await viewModel.LoadVersionCommand.ExecuteAsync(null);

        Assert.That(viewModel.CurrentItem, Is.Null);
        Assert.That(viewModel.InfoPaneItem?.FullPath, Is.EqualTo("source"));

        var file = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\", IsFile = true };
        viewModel.CurrentItem = file;
        Assert.That(viewModel.InfoPaneItem, Is.SameAs(file));

        viewModel.CurrentItem = null;
        Assert.That(viewModel.InfoPaneItem?.FullPath, Is.EqualTo("source"));

        viewModel.SearchTerms = "missing";
        await viewModel.CommitSearchCommand.ExecuteAsync(null);
        Assert.That(viewModel.InfoPaneItem, Is.Null);
    }

    [Test]
    public async Task RestoreFileVersionRestoresOnlySelectedFileFromRequestedVersion()
    {
        var older = Version("1");
        var queryManager = new BrowserQueryManager
        {
            FileDetails = new FileDetails { AvailableVersions = [older] }
        };
        var jobs = new BrowserJobService();
        var viewModel = CreateViewModel(queryManager, jobs);
        viewModel.CurrentVersion = Version("2");
        viewModel.ToggleInfoPane = true;
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };
        viewModel.SelectedItems.Add(viewModel.CurrentItem);
        viewModel.SelectedItems.Add(new FileOrFolderItem { Name = "other.txt", FullPath = @"\source\docs\", IsFile = true });

        await viewModel.RestoreFileVersionCommand.ExecuteAsync(older);

        Assert.That(jobs.SingleRestoreCalls, Is.EqualTo(new[] { ("1", @"\source\docs\report.txt", "") }));
        Assert.That(jobs.BatchRestoreCalls, Is.Empty);
        Assert.That(viewModel.CurrentVersion.Id, Is.EqualTo("2"));
    }

    [Test]
    public async Task JumpToFileVersionLeavesSearchAndSelectsFileInItsFolder()
    {
        var older = Version("1");
        var current = Version("2");
        var file = new FileTableRow { FileName = "report.txt", FilePath = @"\source\docs\" };
        var queryManager = new BrowserQueryManager
        {
            FileDetails = new FileDetails { AvailableVersions = [older, current] },
            Files = [file],
            SearchResults = [file]
        };
        var viewModel = CreateViewModel(queryManager);
        viewModel.Versions.Add(current);
        viewModel.Versions.Add(older);
        viewModel.CurrentVersion = current;
        viewModel.ToggleInfoPane = true;
        viewModel.SearchTerms = "report";
        await viewModel.CommitSearchCommand.ExecuteAsync(null);
        viewModel.CurrentItem = viewModel.Items.Single();

        await viewModel.JumpToFileVersionCommand.ExecuteAsync(Version("1"));

        Assert.That(viewModel.CurrentVersion, Is.SameAs(older));
        Assert.That(viewModel.IsSearchMode, Is.False);
        Assert.That(viewModel.CurrentFolderPath.Last().FullPath, Is.EqualTo(@"source\docs"));
        Assert.That(viewModel.CurrentItem?.Name, Is.EqualTo("report.txt"));
        Assert.That(viewModel.CurrentItem?.FullPath, Is.EqualTo(@"\source\docs\"));
    }

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
    public void PreviousAndNextVersionCommandsAreEnabledForCurrentFolderOrSearch()
    {
        var viewModel = CreateViewModel();
        viewModel.CurrentVersion = Version("2");

        Assert.That(viewModel.NavigateToPreviousVersionCommand.CanExecute(null), Is.False);
        Assert.That(viewModel.NavigateToNextVersionCommand.CanExecute(null), Is.False);

        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", FullPath = "source\\docs", IsFile = false };
        Assert.That(viewModel.NavigateToPreviousVersionCommand.CanExecute(null), Is.False);
        Assert.That(viewModel.NavigateToNextVersionCommand.CanExecute(null), Is.False);

        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = "source\\docs", IsFile = false });
        viewModel.NavigateToPreviousVersionCommand.NotifyCanExecuteChanged();
        viewModel.NavigateToNextVersionCommand.NotifyCanExecuteChanged();
        Assert.That(viewModel.NavigateToPreviousVersionCommand.CanExecute(null), Is.True);
        Assert.That(viewModel.NavigateToNextVersionCommand.CanExecute(null), Is.True);
    }

    [Test]
    public async Task NavigateToPreviousVersionUsesCurrentFolderAndSelectsExistingVersionInstance()
    {
        var queryManager = new BrowserQueryManager
        {
            PreviousFolderVersion = "1"
        };
        var older = Version("1");
        var current = Version("2");
        var viewModel = CreateViewModel(queryManager);
        viewModel.Versions.Add(current);
        viewModel.Versions.Add(older);
        viewModel.CurrentVersion = current;
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source", IsFile = false });
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = "source\\docs", IsFile = false });

        await viewModel.NavigateToPreviousVersionCommand.ExecuteAsync(null);

        Assert.That(queryManager.PreviousFolderLookup, Is.EqualTo(("2", "source\\docs")));
        Assert.That(viewModel.CurrentVersion, Is.SameAs(older));
        Assert.That(viewModel.CurrentFolderPath.Select(x => x.FullPath), Is.EqualTo(new[] { "source", "source\\docs" }));
    }

    [Test]
    public async Task LoadVersionDoesNotDuplicateFavoritesWhenInvokedConcurrently()
    {
        var viewModel = CreateViewModel();
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source", IsFile = false });

        await viewModel.LoadVersionCommand.ExecuteAsync(null);
        var expected = viewModel.Favorites.Select(x => (x.Name, x.Path, x.IsUserFavorite)).ToList();
        Assert.That(expected, Is.Not.Empty);

        await Task.WhenAll(
            viewModel.LoadVersionCommand.ExecuteAsync(null),
            viewModel.LoadVersionCommand.ExecuteAsync(null));

        Assert.That(viewModel.Favorites.Select(x => (x.Name, x.Path, x.IsUserFavorite)), Is.EqualTo(expected));
    }

    [Test]
    public async Task NavigateToNextVersionContainingCurrentFolderKeepsCurrentFile()
    {
        var queryManager = new BrowserQueryManager
        {
            NextFolderVersion = "3",
            Files =
            [
                new FileTableRow { FileName = "report.txt", FilePath = @"\source\docs\", VersionId = "3" }
            ]
        };
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source", IsFile = false });
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false });
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.NavigateToNextVersionCommand.ExecuteAsync(null);

        Assert.That(queryManager.NextFolderLookup, Is.EqualTo(("2", @"source\docs")));
        Assert.That(viewModel.CurrentVersion?.Id, Is.EqualTo("3"));
        Assert.That(viewModel.CurrentFolderPath[^1].FullPath, Is.EqualTo(@"source\docs"));
        Assert.That(viewModel.CurrentItem?.Name, Is.EqualTo("report.txt"));
        Assert.That(viewModel.CurrentItem?.IsFile, Is.True);
    }

    [Test]
    public async Task NavigateToNextVersionDuringSearchUsesSearchTermsAndStaysInSearch()
    {
        var queryManager = new BrowserQueryManager
        {
            NextFileVersion = "3",
            SearchResults =
            [
                new FileTableRow { FileName = "report.txt", FilePath = @"\source\docs\", VersionId = "3" }
            ]
        };
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.SearchTerms = "report";
        await viewModel.CommitSearchCommand.ExecuteAsync(null);

        await viewModel.NavigateToNextVersionCommand.ExecuteAsync(null);

        Assert.That(queryManager.NextFileLookup, Is.EqualTo(("2", "report")));
        Assert.That(viewModel.CurrentVersion?.Id, Is.EqualTo("3"));
        Assert.That(viewModel.IsSearchMode, Is.True);
        Assert.That(viewModel.CommittedSearchTerms, Is.EqualTo("report"));
    }

    [Test]
    public async Task LoadVersionKeepsCurrentFolderInsteadOfResettingToFirstFavorite()
    {
        var viewModel = CreateViewModel();
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false };
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source", IsFile = false });
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false });

        viewModel.CurrentVersion = Version("1");
        await viewModel.LoadVersionCommand.ExecuteAsync(null);

        Assert.That(viewModel.CurrentVersion?.Id, Is.EqualTo("1"));
        Assert.That(viewModel.CurrentFolderPath.Select(x => x.FullPath), Is.EqualTo(new[] { "source", @"source\docs" }));
    }

    [Test]
    public async Task DeleteSelectedContentConfirmsAndCallsDeleteSingleForFile()
    {
        var dialogService = new BrowserDialogService { SelectedContentResult = DeleteSelectedContentResult.AllVersions };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.DeleteSelectedContentCommand.ExecuteAsync(null);

        Assert.That(dialogService.SelectedContentDeletePrompted, Is.True);
        Assert.That(jobService.DeleteSingleCalls, Is.EqualTo(new[] { ("report.txt", @"\source\docs\", (IReadOnlyList<int>?)null) }));
    }

    [Test]
    public async Task DeleteSelectedContentPassesSelectedVersionIds()
    {
        var dialogService = new BrowserDialogService
        {
            SelectedContentResult = DeleteSelectedContentResult.FromVersions(["2", "3"])
        };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.DeleteSelectedContentCommand.ExecuteAsync(null);

        Assert.That(jobService.DeleteSingleCalls, Has.Count.EqualTo(1));
        Assert.That(jobService.DeleteSingleCalls[0].FileFilter, Is.EqualTo("report.txt"));
        Assert.That(jobService.DeleteSingleCalls[0].FolderFilter, Is.EqualTo(@"\source\docs\"));
        Assert.That(jobService.DeleteSingleCalls[0].VersionIds, Is.EqualTo(new[] { 2, 3 }));
    }

    [Test]
    public async Task DeleteSelectedContentUsesMultipleSelectedItemsInsteadOfCurrentItem()
    {
        var dialogService = new BrowserDialogService
        {
            SelectedContentResult = DeleteSelectedContentResult.FromVersions(["2"])
        };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "keep.txt", FullPath = @"\source\", IsFile = true };
        viewModel.SelectedItems.Add(new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\", IsFile = true });
        viewModel.SelectedItems.Add(new FileOrFolderItem { Name = "old_files", FullPath = @"\source\old_files" });

        await viewModel.DeleteSelectedContentCommand.ExecuteAsync(null);

        Assert.That(jobService.DeleteSingleCalls, Is.Empty);
        Assert.That(jobService.DeleteSelectionTargets, Is.EqualTo(new[]
        {
            ("report.txt", @"\source\"),
            (string.Empty, @"\source\old_files")
        }));
        Assert.That(jobService.DeleteSelectionVersionIds, Is.EqualTo(new[] { 2 }));
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

    [Test]
    public void ToggleInfoPaneDefaultsToHiddenWhenNoPreferenceSaved()
    {
        var viewModel = CreateViewModel();

        Assert.That(viewModel.ToggleInfoPane, Is.False);
    }

    [Test]
    public async Task ToggleInfoPaneChangePersistsPreference()
    {
        var preferences = new BrowserViewPreferencesService(new MemoryLocalSettingsService());
        var viewModel = CreateViewModel(viewPreferencesService: preferences);

        viewModel.ToggleInfoPane = true;

        Assert.That(await preferences.GetInfoPaneVisibleAsync(), Is.True);

        viewModel.ToggleInfoPane = false;

        Assert.That(await preferences.GetInfoPaneVisibleAsync(), Is.False);
    }

    [Test]
    public async Task LoadViewPreferencesRestoresPersistedInfoPanePreference()
    {
        var settings = new MemoryLocalSettingsService();
        await settings.SaveSettingAsync(BrowserViewPreferencesService.InfoPaneVisibleSettingsKey, true);
        var preferences = new BrowserViewPreferencesService(settings);
        var viewModel = CreateViewModel(viewPreferencesService: preferences);

        await viewModel.LoadViewPreferencesAsync();

        Assert.That(viewModel.ToggleInfoPane, Is.True);
    }

    [Test]
    public async Task LoadViewPreferencesLeavesDefaultWhenSettingsReadFails()
    {
        var viewModel = CreateViewModel(viewPreferencesService: new FailingBrowserViewPreferencesService());

        await viewModel.LoadViewPreferencesAsync();

        Assert.That(viewModel.ToggleInfoPane, Is.False);
    }

    [Test]
    public async Task ToggleInfoPaneChangeIgnoresPersistenceFailures()
    {
        var viewModel = CreateViewModel(viewPreferencesService: new FailingBrowserViewPreferencesService());

        Assert.DoesNotThrow(() => viewModel.ToggleInfoPane = true);
        await Task.Yield();

        Assert.That(viewModel.ToggleInfoPane, Is.True);
    }

    [Test]
    public async Task RestoreFileCommand_RestoresSelectedFileToOriginalPath()
    {
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.RestoreFileCommand.ExecuteAsync(null);

        Assert.That(jobService.RestoreCalls.Single(), Is.EqualTo(("2", @"\source\docs\report.txt", "")));
    }

    [Test]
    public async Task RestoreFileToCommand_PassesChosenDestinationFolder()
    {
        var dialogService = new BrowserDialogService { DestinationFolder = @"D:\RestoreHere" };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.RestoreFileToCommand.ExecuteAsync(null);

        Assert.That(dialogService.DestinationPickerPrompted, Is.True);
        Assert.That(jobService.RestoreCalls.Single(), Is.EqualTo(("2", @"\source\docs\report.txt", @"D:\RestoreHere")));
    }

    [Test]
    public async Task RestoreFileToCommand_CancelsWhenDestinationPickerIsCancelled()
    {
        var dialogService = new BrowserDialogService { DestinationFolder = null };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true };

        await viewModel.RestoreFileToCommand.ExecuteAsync(null);

        Assert.That(dialogService.DestinationPickerPrompted, Is.True);
        Assert.That(jobService.RestoreCalls, Is.Empty);
    }

    [Test]
    public async Task RestoreAllToCommand_PassesChosenDestinationForCurrentFolder()
    {
        var dialogService = new BrowserDialogService { DestinationFolder = @"D:\RestoreHere" };
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService, browserDialogService: dialogService);
        viewModel.CurrentVersion = Version("2");
        // Production BrowserContentService stores slash-stripped folder paths.
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false });

        await viewModel.RestoreAllToCommand.ExecuteAsync(null);

        Assert.That(jobService.RestoreCalls.Single(), Is.EqualTo(("2", @"\source\docs\", @"D:\RestoreHere")));
    }

    [Test]
    public async Task RestoreFileCommand_NormalizesSelectedFolderPathForRestoreJob()
    {
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentItem = new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false };

        await viewModel.RestoreFileCommand.ExecuteAsync(null);

        Assert.That(jobService.RestoreCalls.Single(), Is.EqualTo(("2", @"\source\docs\", "")));
    }

    [Test]
    public async Task RestoreAllCommand_NormalizesCurrentFolderPathForRestoreJob()
    {
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false });

        await viewModel.RestoreAllCommand.ExecuteAsync(null);

        Assert.That(jobService.RestoreCalls.Single(), Is.EqualTo(("2", @"\source\docs\", "")));
    }

    [Test]
    public async Task RestoreFileCommand_RestoresSeveralSelectedFilesAndFoldersInOneJob()
    {
        var jobService = new BrowserJobService();
        var viewModel = CreateViewModel(jobService: jobService);
        viewModel.CurrentVersion = Version("2");
        viewModel.SelectedItems.Add(new FileOrFolderItem { Name = "report.txt", FullPath = @"\source\docs\", IsFile = true });
        viewModel.SelectedItems.Add(new FileOrFolderItem { Name = "notes", FullPath = @"source\notes", IsFile = false });

        await viewModel.RestoreFileCommand.ExecuteAsync(null);

        Assert.That(jobService.BatchRestoreCalls, Has.Count.EqualTo(1));
        Assert.That(jobService.BatchRestoreCalls[0].Version, Is.EqualTo("2"));
        Assert.That(jobService.BatchRestoreCalls[0].Destination, Is.EqualTo(""));
        Assert.That(jobService.BatchRestoreCalls[0].Files, Is.EqualTo(new[] { @"\source\docs\report.txt", @"\source\notes\" }));
        Assert.That(jobService.SingleRestoreCalls, Is.Empty);
    }

    [Test]
    public async Task SearchTermsChange_DoesNotQueryUntilSearchIsCommitted()
    {
        var queryManager = new BrowserQueryManager();
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "docs", FullPath = @"source\docs", IsFile = false });

        viewModel.SearchTerms = "report";
        await Task.Delay(50);

        Assert.That(queryManager.SearchCalls, Is.Empty);

        await viewModel.CommitSearchCommand.ExecuteAsync(null);

        Assert.That(queryManager.SearchCalls.Single(), Is.EqualTo(("2", "report")));
        Assert.That(viewModel.IsSearchMode, Is.True);
        Assert.That(viewModel.CommittedSearchTerms, Is.EqualTo("report"));
        Assert.That(viewModel.SearchPathLabel, Does.Contain("report"));
    }

    [Test]
    public async Task CommitSearch_WhenNoFilesMatch_ShowsSearchEmptyState()
    {
        var queryManager = new BrowserQueryManager { SearchResults = [] };
        var viewModel = CreateViewModel(queryManager);
        viewModel.CurrentVersion = Version("2");
        viewModel.SearchTerms = "missing-file";

        await viewModel.CommitSearchCommand.ExecuteAsync(null);

        Assert.That(viewModel.IsSearchMode, Is.True);
        Assert.That(viewModel.ShowSearchEmptyState, Is.True);
        Assert.That(viewModel.Items, Is.Empty);
        Assert.That(viewModel.SearchPathLabel, Does.Contain("missing-file"));
    }

    [Test]
    public async Task ClearingSearchWithoutCurrentVersionRestoresFolderContext()
    {
        var viewModel = CreateViewModel();
        var folder = new FileOrFolderItem { Name = "docs", FullPath = @"source\docs" };
        viewModel.CurrentVersion = Version("2");
        viewModel.CurrentFolderPath.Add(new FileOrFolderItem { Name = "source", FullPath = "source" });
        viewModel.CurrentFolderPath.Add(folder);
        viewModel.SearchTerms = "missing-file";
        await viewModel.CommitSearchCommand.ExecuteAsync(null);
        Assert.That(viewModel.InfoPaneItem, Is.Null);
        Assert.That(viewModel.UpFolderCommand.CanExecute(null), Is.False);

        viewModel.CurrentVersion = null;
        var infoPaneChanged = false;
        var upFolderChanged = false;
        viewModel.PropertyChanged += (_, args) => infoPaneChanged |= args.PropertyName == nameof(BrowserViewModel.InfoPaneItem);
        viewModel.UpFolderCommand.CanExecuteChanged += (_, _) => upFolderChanged = true;
        viewModel.SearchTerms = string.Empty;

        Assert.That(viewModel.IsSearchMode, Is.False);
        Assert.That(viewModel.InfoPaneItem, Is.SameAs(folder));
        Assert.That(viewModel.UpFolderCommand.CanExecute(null), Is.False);
        Assert.That(infoPaneChanged, Is.True);
        Assert.That(upFolderChanged, Is.True);

        upFolderChanged = false;
        viewModel.CurrentVersion = Version("2");

        Assert.That(viewModel.UpFolderCommand.CanExecute(null), Is.True);
        Assert.That(upFolderChanged, Is.True);
    }

    private static BrowserViewModel CreateViewModel(
        BrowserQueryManager? queryManager = null,
        BrowserJobService? jobService = null,
        BrowserDialogService? browserDialogService = null,
        BrowserFavoritesService? favoritesService = null,
        BrowserPreviewServiceFake? previewService = null,
        IBrowserViewPreferencesService? viewPreferencesService = null)
    {
        queryManager ??= new BrowserQueryManager();
        favoritesService ??= new BrowserFavoritesService(new MemoryLocalSettingsService());
        viewPreferencesService ??= new BrowserViewPreferencesService(new MemoryLocalSettingsService());

        return new BrowserViewModel(
            queryManager,
            jobService ?? new BrowserJobService(),
            new FakeBackupService(),
            new FakePresentationService(),
            new BrowserContentService(queryManager, favoritesService, new FakeConfigurationManager()),
            browserDialogService ?? new BrowserDialogService(),
            previewService ?? new BrowserPreviewServiceFake(),
            viewPreferencesService);
    }

    private static VersionDetails Version(string id) => new()
    {
        Id = id,
        CreationDate = new DateTime(2026, 1, int.Parse(id)),
        Sources = @"D:\source",
        Title = "v" + id,
        Description = string.Empty
    };

    private sealed class BrowserQueryManager : FakeQueryManager
    {
        public string? PreviousFolderVersion { get; set; }
        public string? NextFolderVersion { get; set; }
        public string? NextFileVersion { get; set; }
        public (string Version, string Path)? PreviousFolderLookup { get; private set; }
        public (string Version, string Path)? NextFolderLookup { get; private set; }
        public (string Version, string Search)? NextFileLookup { get; private set; }
        public List<FileTableRow> Files { get; set; } = [];
        public FileDetails? FileDetails { get; set; }
        public int GetVersionsCallCount { get; private set; }
        public Task<VersionChangeStatistics> StatisticsTask { get; set; } = Task.FromResult(new VersionChangeStatistics(0, 0, 0));
        public List<string> StatisticsCalls { get; } = [];
        public List<FileTableRow> SearchResults { get; set; } = [];
        public List<(string Version, string SearchTerm)> SearchCalls { get; } = [];

        public override Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path)
        {
            PreviousFolderLookup = (startVersion, path);
            return Task.FromResult(PreviousFolderVersion);
        }

        public override Task<VersionChangeStatistics> GetVersionChangeStatisticsAsync(string versionId)
        {
            StatisticsCalls.Add(versionId);
            return StatisticsTask;
        }

        public override Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(FileDetails!);
        public override Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(Files);
        public override Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(Version("3"));
        public override Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(Version("3"));

        public override Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString)
        {
            NextFileLookup = (startVersion, searchString);
            return Task.FromResult(NextFileVersion);
        }

        public override Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path)
        {
            NextFolderLookup = (startVersion, path);
            return Task.FromResult(NextFolderVersion);
        }

        public override Task<int> GetNumberOfVersionsAsync() => Task.FromResult(3);
        public override Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(Version("1"));
        public override Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(Version(id));

        public override List<VersionDetails> GetVersions(bool desc = true)
        {
            GetVersionsCallCount++;
            return desc ? [Version("2"), Version("1"), Version("3")] : [Version("1"), Version("2"), Version("3")];
        }

        public override Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500)
        {
            SearchCalls.Add((version, searchTerm));
            return Task.FromResult(SearchResults);
        }
    }

    private sealed class BrowserJobService : FakeJobService
    {
        public List<(string FileFilter, string FolderFilter, IReadOnlyList<int>? VersionIds)> DeleteSingleCalls { get; } = [];
        public IReadOnlyList<(string FileFilter, string PathFilter)> DeleteSelectionTargets { get; private set; } = [];
        public IReadOnlyList<int>? DeleteSelectionVersionIds { get; private set; }
        public List<List<string>> DeleteBackupsCalls { get; } = [];
        public List<(string Version, string File, string Destination)> RestoreCalls { get; } = [];
        public List<(string Version, string File, string Destination)> SingleRestoreCalls { get; } = [];
        public List<(string Version, IReadOnlyList<string> Files, string Destination)> BatchRestoreCalls { get; } = [];

        public override Task<JobSessionResult> DeleteBackupsAsync(List<string> versions, bool statusDialog = true) { DeleteBackupsCalls.Add(versions); return Task.FromResult(new JobSessionResult { Started = true }); }

        public override Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true, IReadOnlyList<int>? versionIds = null)
        {
            DeleteSingleCalls.Add((fileFilter, folderFilter, versionIds));
            return Task.CompletedTask;
        }

        public override Task DeleteSelectedFilesAsync(IReadOnlyList<(string FileFilter, string PathFilter)> targets, IReadOnlyList<int>? versionIds)
        {
            DeleteSelectionTargets = targets;
            DeleteSelectionVersionIds = versionIds;
            return Task.CompletedTask;
        }

        public override Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true)
        {
            BatchRestoreCalls.Add((version, files, destination));
            foreach (var file in files)
            {
                RestoreCalls.Add((version, file, destination));
            }

            return Task.CompletedTask;
        }

        public override Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true)
        {
            SingleRestoreCalls.Add((version, file, destination));
            RestoreCalls.Add((version, file, destination));
            return Task.CompletedTask;
        }
    }

    private sealed class BrowserDialogService : IBrowserDialogService
    {
        public DeleteSelectedContentResult SelectedContentResult { get; set; } = DeleteSelectedContentResult.Cancelled;
        public bool SelectedContentDeletePrompted { get; private set; }
        public IReadOnlyList<string>? VersionsSelectedForDelete { get; set; }
        public IReadOnlyList<string> MultiDeletePromptedVersions { get; private set; } = [];
        public IReadOnlyList<VersionDetails> SelectedContentPromptedVersions { get; private set; } = [];
        public string? DestinationFolder { get; set; }
        public bool DestinationPickerPrompted { get; private set; }

        public Task<DeleteSelectedContentResult> ShowDeleteSelectedContentWindowAsync(IReadOnlyList<FileOrFolderItem> items, IReadOnlyList<VersionDetails> versions)
        {
            SelectedContentDeletePrompted = true;
            SelectedContentPromptedVersions = versions;
            return Task.FromResult(SelectedContentResult);
        }

        public Task<IReadOnlyList<string>> ShowDeleteBackupsWindowAsync(IReadOnlyList<VersionDetails> versions) { MultiDeletePromptedVersions = versions.Select(x => x.Id).ToList(); return Task.FromResult(VersionsSelectedForDelete ?? []); }
        public Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite) => Task.FromResult<string?>(favorite.Name);
        public Task<string?> PickRestoreDestinationFolderAsync()
        {
            DestinationPickerPrompted = true;
            return Task.FromResult(DestinationFolder);
        }
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

    private sealed class FailingBrowserViewPreferencesService : IBrowserViewPreferencesService
    {
        public Task<bool> GetInfoPaneVisibleAsync() => throw new InvalidOperationException("settings unavailable");

        public Task SetInfoPaneVisibleAsync(bool visible) => throw new InvalidOperationException("settings unavailable");
    }
}
