// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels;

public partial class BrowserViewModel : ObservableObject, INavigationAware
{
    private readonly IQueryManager queryManager;
    private readonly IJobService jobService;
    private readonly IBackupService backupService;
    private readonly IPresentationService presentationService;
    private readonly IBrowserContentService browserContentService;
    private readonly IBrowserDialogService browserDialogService;
    private readonly IBrowserPreviewService browserPreviewService;
    private readonly IBrowserViewPreferencesService viewPreferencesService;
    private BrowserContentMode contentMode = BrowserContentMode.Folder;
    private long contentRequestId;
    private bool suppressSearchTermsChanged;
    private bool suppressInfoPaneChanged;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileToCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllToCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteMultipleBackupsCommand))]
    private VersionDetails? currentVersion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileToCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllToCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFilePreviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFilePropertiesCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddFolderToFavoritesCommand))]
    [NotifyCanExecuteChangedFor(nameof(NavigateToPreviousVersionCommand))]
    [NotifyCanExecuteChangedFor(nameof(NavigateToNextVersionCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedContentCommand))]
    private FileOrFolderItem? currentItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameFavoriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveFavoriteCommand))]
    private BrowserFavoriteItem? currentFavorite;

    [ObservableProperty]
    private string searchTerms = string.Empty;

    [ObservableProperty]
    private bool toggleInfoPane = false;

    [ObservableProperty]
    private bool hasVersions = false;

    public ObservableCollection<FileOrFolderItem> CurrentFolderPath { get; set; } = [];

    public ObservableCollection<BrowserFavoriteItem> Favorites { get; set; } = [];

    public ObservableCollection<FileOrFolderItem> Items { get; set; } = [];

    public ObservableCollection<VersionDetails> Versions { get; set; } = [];

    public BrowserViewModel(
        IQueryManager queryManager,
        IJobService jobService,
        IBackupService backupService,
        IPresentationService presentationService,
        IBrowserContentService browserContentService,
        IBrowserDialogService browserDialogService,
        IBrowserPreviewService browserPreviewService,
        IBrowserViewPreferencesService viewPreferencesService)
    {
        this.queryManager = queryManager;
        this.jobService = jobService;
        this.backupService = backupService;
        this.presentationService = presentationService;
        this.browserContentService = browserContentService;
        this.browserDialogService = browserDialogService;
        this.browserPreviewService = browserPreviewService;
        this.viewPreferencesService = viewPreferencesService;
    }

    partial void OnToggleInfoPaneChanged(bool value)
    {
        if (suppressInfoPaneChanged)
        {
            return;
        }

        _ = PersistInfoPaneVisibleAsync(value);
    }

    public async Task LoadViewPreferencesAsync()
    {
        suppressInfoPaneChanged = true;
        try
        {
            ToggleInfoPane = await viewPreferencesService.GetInfoPaneVisibleAsync();
        }
        catch
        {
            // Preference restore must not disrupt browsing; keep the default pane state.
        }
        finally
        {
            suppressInfoPaneChanged = false;
        }
    }

    private async Task PersistInfoPaneVisibleAsync(bool value)
    {
        try
        {
            await viewPreferencesService.SetInfoPaneVisibleAsync(value);
        }
        catch
        {
            // Preference persistence must not disrupt browsing.
        }
    }

    private bool CanUpFolder() => contentMode == BrowserContentMode.Folder && CurrentFolderPath.Count > 1;
    private bool HasFileOrFolderSelected() => CurrentItem != null && CurrentVersion != null;
    private bool CanRestoreAll() => contentMode == BrowserContentMode.Folder && CurrentVersion != null && CurrentFolderPath.Count > 0;
    private bool HasVersionSelected() => CurrentVersion != null;
    private bool HasFileSelected() => CurrentItem != null && CurrentItem.IsFile;
    private bool HasUserFavoriteSelected() => CurrentFavorite?.IsUserFavorite == true;

    partial void OnSearchTermsChanged(string value)
    {
        if (suppressSearchTermsChanged)
        {
            return;
        }

        _ = ApplySearchTermsAsync(value);
    }

    private async Task ApplySearchTermsAsync(string value)
    {
        if (CurrentVersion == null)
        {
            return;
        }

        var requestId = BeginContentRequest();
        if (string.IsNullOrWhiteSpace(value))
        {
            if (CurrentFolderPath.Count > 0)
            {
                await LoadFolderAsync(CurrentVersion.Id, CurrentFolderPath[^1].FullPath, requestId);
            }

            return;
        }

        await LoadSearchResultsAsync(CurrentVersion.Id, value, requestId);
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task LoadVersion()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        await LoadFavoritesAsync();
        if (Favorites.Count == 0)
        {
            return;
        }

        CurrentFavorite = Favorites[0];

        ClearSearchTerms();
        await LoadFolderAsync(CurrentVersion.Id, CurrentFavorite.Path, BeginContentRequest());
    }

    [RelayCommand]
    private async Task LoadFavorite()
    {
        if (CurrentVersion == null || CurrentFavorite == null)
        {
            return;
        }

        ClearSearchTerms();
        await LoadFolderAsync(CurrentVersion.Id, CurrentFavorite.Path, BeginContentRequest());
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task LoadFolder()
    {
        if (CurrentItem == null || CurrentItem.IsFile || CurrentVersion == null)
        {
            return;
        }

        // load folder
        ClearSearchTerms();
        await LoadFolderAsync(CurrentVersion.Id, CurrentItem.FullPath, BeginContentRequest());
    }

    [RelayCommand(CanExecute = nameof(CanUpFolder))]
    private async Task UpFolder()
    {
        if (CurrentFolderPath.Count < 2 || CurrentVersion == null)
        {
            return;
        }

        // load parent folder
        ClearSearchTerms();
        await LoadFolderAsync(CurrentVersion.Id, CurrentFolderPath[^2].FullPath, BeginContentRequest());
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (CurrentVersion == null || CurrentFolderPath.Count == 0)
        {
            return;
        }

        var favorite = CurrentFavorite;
        var versionId = CurrentVersion.Id;
        var currentFolder = CurrentFolderPath[^1].FullPath;
        var searchTerms = SearchTerms;
        var wasSearching = contentMode == BrowserContentMode.Search && !string.IsNullOrWhiteSpace(searchTerms);

        LoadVersions();

        CurrentVersion = Versions.First(x => x.Id == versionId);
        CurrentFavorite = favorite;

        var requestId = BeginContentRequest();
        if (wasSearching)
        {
            await LoadSearchResultsAsync(CurrentVersion.Id, searchTerms, requestId);
        }
        else
        {
            await LoadFolderAsync(CurrentVersion.Id, currentFolder, requestId);
        }
    }

    [RelayCommand]
    private async Task LoadFolderWithParam(FileOrFolderItem selectedItem)
    {
        if (selectedItem == null || CurrentVersion == null)
        {
            return;
        }

        ClearSearchTerms();
        await LoadFolderAsync(CurrentVersion.Id, selectedItem.FullPath, BeginContentRequest());
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task RestoreFile()
    {
        await RestoreSelectedAsync(destination: string.Empty);
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task RestoreFileTo()
    {
        var destination = await browserDialogService.PickRestoreDestinationFolderAsync();
        if (destination == null)
        {
            return;
        }

        await RestoreSelectedAsync(destination);
    }

    [RelayCommand(CanExecute = nameof(CanRestoreAll))]
    private async Task RestoreAll()
    {
        await RestoreCurrentFolderAsync(destination: string.Empty);
    }

    [RelayCommand(CanExecute = nameof(CanRestoreAll))]
    private async Task RestoreAllTo()
    {
        var destination = await browserDialogService.PickRestoreDestinationFolderAsync();
        if (destination == null)
        {
            return;
        }

        await RestoreCurrentFolderAsync(destination);
    }

    private async Task RestoreSelectedAsync(string destination)
    {
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        // RestoreJob treats a path as a single file when Path.GetFileName is non-empty.
        // Browser folder FullPaths are slash-stripped (e.g. "source\docs"), so folder
        // restores must be normalized to "\source\docs\" like WinForms.
        var path = CurrentItem.IsFile
            ? CurrentItem.FullPath + CurrentItem.Name
            : ToFolderRestorePath(CurrentItem.FullPath);

        await jobService.RestoreBackupAsync(CurrentVersion.Id, path, destination);
    }

    private async Task RestoreCurrentFolderAsync(string destination)
    {
        if (CurrentVersion == null || CurrentFolderPath.Count == 0 || CurrentFolderPath[^1] == null)
        {
            return;
        }

        await jobService.RestoreBackupAsync(
            CurrentVersion.Id,
            ToFolderRestorePath(CurrentFolderPath[^1].FullPath),
            destination);
    }

    /// <summary>
    /// Formats a browser folder path for RestoreJob folder-mode restores.
    /// </summary>
    private static string ToFolderRestorePath(string path)
    {
        path ??= string.Empty;
        path = path.Trim('\\');
        if (string.IsNullOrEmpty(path))
        {
            return "\\";
        }

        return "\\" + path + "\\";
    }

    [RelayCommand(CanExecute = nameof(HasFileSelected))]
    private async Task ShowFileProperties()
    {
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        var fileDetails = await queryManager.GetFileDetailsAsync(CurrentVersion.Id, CurrentItem.Name, CurrentItem.FullPath);
        if (fileDetails != null)
        {
            await browserDialogService.ShowFileDetailsAsync(fileDetails);
        }
    }

    [RelayCommand(CanExecute = nameof(HasFileSelected))]
    private async Task ShowFilePreview()
    {
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        await browserPreviewService.PreviewFileAsync(CurrentVersion.Id, CurrentItem.Name, CurrentItem.FullPath);
    }

    [RelayCommand(CanExecute = nameof(CanAddFolderToFavorites))]
    private async Task AddFolderToFavorites()
    {
        var folder = GetFolderForFavorite();
        if (folder == null)
        {
            return;
        }

        await browserContentService.AddFavoriteAsync(folder);
        await LoadFavoritesAsync();
        CurrentFavorite = Favorites.FirstOrDefault(x => browserContentService.PathsMatch(x.Path, folder.FullPath)) ?? CurrentFavorite;
    }

    [RelayCommand(CanExecute = nameof(HasUserFavoriteSelected))]
    private async Task RenameFavorite()
    {
        if (CurrentFavorite?.IsUserFavorite != true)
        {
            return;
        }

        var name = await browserDialogService.ShowRenameFavoriteWindowAsync(CurrentFavorite);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await browserContentService.RenameFavoriteAsync(CurrentFavorite, name);
        var path = CurrentFavorite.Path;
        await LoadFavoritesAsync();
        CurrentFavorite = Favorites.FirstOrDefault(x => browserContentService.PathsMatch(x.Path, path));
    }

    [RelayCommand(CanExecute = nameof(HasUserFavoriteSelected))]
    private async Task RemoveFavorite()
    {
        if (CurrentFavorite?.IsUserFavorite != true)
        {
            return;
        }

        await browserContentService.RemoveFavoriteAsync(CurrentFavorite);
        await LoadFavoritesAsync();
        CurrentFavorite = Favorites.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task NavigateToPreviousVersion()
    {
        await NavigateToContainingVersionAsync(previous: true);
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task NavigateToNextVersion()
    {
        await NavigateToContainingVersionAsync(previous: false);
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task DeleteSelectedContent()
    {
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        if (Versions.Count == 0)
        {
            LoadVersions();
        }

        IReadOnlyList<VersionDetails> candidateVersions = Versions.ToList();
        if (CurrentItem.IsFile)
        {
            var details = await queryManager.GetFileDetailsAsync(CurrentVersion.Id, CurrentItem.Name, CurrentItem.FullPath);
            if (details?.AvailableVersions is { Count: > 0 })
            {
                candidateVersions = details.AvailableVersions;
            }
        }

        var deleteResult = await browserDialogService.ShowDeleteSelectedContentWindowAsync(CurrentItem, candidateVersions);
        if (deleteResult.IsCancelled)
        {
            return;
        }

        IReadOnlyList<int>? versionIds = null;
        if (!deleteResult.DeleteFromAllVersions)
        {
            versionIds = deleteResult.VersionIds
                .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();
            if (versionIds.Count == 0)
            {
                return;
            }
        }

        if (CurrentItem.IsFile)
        {
            await jobService.DeleteSingleFileAsync(CurrentItem.Name, CurrentItem.FullPath, versionIds: versionIds);
        }
        else
        {
            await jobService.DeleteSingleFileAsync(string.Empty, CurrentItem.FullPath + "%", versionIds: versionIds);
        }

        await Refresh();
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task DeleteMultipleBackups()
    {
        if (Versions.Count == 0)
        {
            LoadVersions();
        }

        var versionsToDelete = await browserDialogService.ShowDeleteBackupsWindowAsync(Versions.ToList());
        if (versionsToDelete.Count == 0)
        {
            return;
        }

        await jobService.DeleteBackupsAsync(versionsToDelete.ToList());

        LoadVersions();
        if (Versions.Count == 0)
        {
            CurrentVersion = null;
            Items.Clear();
            CurrentFolderPath.Clear();
            return;
        }

        CurrentVersion = Versions.FirstOrDefault(x => !versionsToDelete.Contains(x.Id)) ?? Versions[0];
        await LoadVersion();
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task EditBackup()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        var existingBackupViewModel = new EditBackupViewModel
        {
            Title = CurrentVersion?.Title,
            Description = CurrentVersion?.Description,
        };

        var (result, viewModel) = await presentationService.ShowEditBackupWindowAsync(existingBackupViewModel);
        if (result)
        {
            await backupService.UpdateVersionAsync(CurrentVersion!.Id, new VersionDetails { Title = viewModel.Title, Description = viewModel.Description });
            await Refresh();
        }
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task DeleteBackup()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        var result = await presentationService.ShowDeleteBackupWindowAsync();
        if (result)
        {
            await jobService.DeleteBackupAsync(CurrentVersion.Id);

            LoadVersions();

            if (Versions.Count > 0)
            {
                CurrentVersion = Versions[0];
                await LoadVersion();
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasVersionSelected))]
    private async Task LockBackup()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        await backupService.SetStableAsync(CurrentVersion.Id, !CurrentVersion.Stable);
        await Refresh();
    }

    [RelayCommand]
    private async Task GoHome()
    {
        await presentationService.ShowMainWindowAsync();
    }

    private void LoadVersions()
    {
        var backupVersions = queryManager.GetVersions(true);

        Versions.Clear();
        HasVersions = backupVersions.Count > 0;
        backupVersions.ForEach(Versions.Add);
    }

    private async Task LoadFavoritesAsync()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        Favorites.Clear();
        foreach (var favorite in await browserContentService.GetFavoritesAsync(CurrentVersion))
        {
            Favorites.Add(favorite);
        }
    }

    private async Task NavigateToContainingVersionAsync(bool previous)
    {
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        var versionId = CurrentItem.IsFile
            ? previous
                ? await queryManager.GetBackVersionWhereFileAsync(CurrentVersion.Id, CurrentItem.Name)
                : await queryManager.GetNextVersionWhereFileAsync(CurrentVersion.Id, CurrentItem.Name)
            : previous
                ? await queryManager.GetBackVersionWhereFilesInFolderAsync(CurrentVersion.Id, CurrentItem.FullPath)
                : await queryManager.GetNextVersionWhereFilesInFolderAsync(CurrentVersion.Id, CurrentItem.FullPath);

        if (versionId == null)
        {
            return;
        }

        CurrentVersion = await queryManager.GetVersionByIdAsync(versionId);
        await LoadVersion();
    }

    private async Task LoadFolderAsync(string version, string path, long requestId)
    {
        var snapshot = await browserContentService.LoadFolderAsync(version, path);

        if (!IsCurrentContentRequest(requestId))
        {
            return;
        }

        contentMode = BrowserContentMode.Folder;
        CurrentItem = null;
        CurrentFolderPath.Clear();
        foreach (var folder in snapshot.FolderPath)
        {
            CurrentFolderPath.Add(folder);
        }
        ReplaceItems(snapshot.Items);
        NotifyContentCommandsChanged();
    }

    private async Task LoadSearchResultsAsync(string version, string searchTerms, long requestId)
    {
        var fileList = await browserContentService.SearchFilesAsync(version, searchTerms);

        if (!IsCurrentContentRequest(requestId))
        {
            return;
        }

        contentMode = BrowserContentMode.Search;
        CurrentItem = null;
        ReplaceItems(fileList);
        NotifyContentCommandsChanged();
    }

    private long BeginContentRequest() => Interlocked.Increment(ref contentRequestId);

    private bool IsCurrentContentRequest(long requestId) => Interlocked.Read(ref contentRequestId) == requestId;

    private void ClearSearchTerms()
    {
        if (SearchTerms.Length == 0)
        {
            return;
        }

        suppressSearchTermsChanged = true;
        SearchTerms = string.Empty;
        suppressSearchTermsChanged = false;
    }

    private void ReplaceItems(IEnumerable<FileOrFolderItem> items)
    {
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    private void NotifyContentCommandsChanged()
    {
        LoadVersionCommand.NotifyCanExecuteChanged();
        LoadFolderCommand.NotifyCanExecuteChanged();
        UpFolderCommand.NotifyCanExecuteChanged();
        RestoreAllCommand.NotifyCanExecuteChanged();
        RestoreAllToCommand.NotifyCanExecuteChanged();
        AddFolderToFavoritesCommand.NotifyCanExecuteChanged();
        NavigateToPreviousVersionCommand.NotifyCanExecuteChanged();
        NavigateToNextVersionCommand.NotifyCanExecuteChanged();
        DeleteSelectedContentCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddFolderToFavorites() => GetFolderForFavorite() != null;

    private FileOrFolderItem? GetFolderForFavorite() => browserContentService.GetFolderForFavorite(CurrentItem, CurrentFolderPath);

    public async void OnNavigatedTo(object parameter)
    {
        // Restore layout prefs in parallel so a slow/failed settings read cannot block browsing.
        var preferencesTask = LoadViewPreferencesAsync();

        LoadVersions();

        if (Versions.Count > 0)
        {
            CurrentVersion = Versions[0];
            await LoadVersion();
        }

        await preferencesTask;
    }

    public void OnNavigatedFrom()
    {
        browserContentService.ClearIconCache();
    }
}
