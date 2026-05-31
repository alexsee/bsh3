// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using BSH.MainApp.Utils;
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
    private readonly IBrowserFavoritesService favoritesService;
    private BrowserContentMode contentMode = BrowserContentMode.Folder;
    private long contentRequestId;
    private bool suppressSearchTermsChanged;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteMultipleBackupsCommand))]
    private VersionDetails? currentVersion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
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
        IBrowserFavoritesService favoritesService)
    {
        this.queryManager = queryManager;
        this.jobService = jobService;
        this.backupService = backupService;
        this.presentationService = presentationService;
        this.favoritesService = favoritesService;
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
        if (CurrentItem == null || CurrentVersion == null)
        {
            return;
        }

        // restore file
        if (CurrentItem.IsFile)
        {
            await jobService.RestoreBackupAsync(CurrentVersion.Id, CurrentItem.FullPath + CurrentItem.Name, "");
        }
        else
        {
            await jobService.RestoreBackupAsync(CurrentVersion.Id, CurrentItem.FullPath, "");
        }
    }

    [RelayCommand(CanExecute = nameof(CanRestoreAll))]
    private async Task RestoreAll()
    {
        if (CurrentVersion == null || CurrentFolderPath[^1] == null)
        {
            return;
        }

        // restore all
        await jobService.RestoreBackupAsync(CurrentVersion.Id, CurrentFolderPath[^1].FullPath, "");
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
            await presentationService.ShowFileDetailsAsync(fileDetails);
        }
    }

    [RelayCommand(CanExecute = nameof(HasFileSelected))]
    private async Task ShowFilePreview()
    {
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(CanAddFolderToFavorites))]
    private async Task AddFolderToFavorites()
    {
        var folder = GetFolderForFavorite();
        if (folder == null)
        {
            return;
        }

        await favoritesService.AddFavoriteAsync(new BrowserFavoriteItem
        {
            Name = folder.Name,
            Path = folder.FullPath,
            IsUserFavorite = true
        });
        await LoadFavoritesAsync();
        CurrentFavorite = Favorites.FirstOrDefault(x => PathsMatch(x.Path, folder.FullPath)) ?? CurrentFavorite;
    }

    [RelayCommand(CanExecute = nameof(HasUserFavoriteSelected))]
    private async Task RenameFavorite()
    {
        if (CurrentFavorite?.IsUserFavorite != true)
        {
            return;
        }

        var name = await presentationService.ShowRenameFavoriteWindowAsync(CurrentFavorite);
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await favoritesService.RenameFavoriteAsync(CurrentFavorite, name);
        var path = CurrentFavorite.Path;
        await LoadFavoritesAsync();
        CurrentFavorite = Favorites.FirstOrDefault(x => PathsMatch(x.Path, path));
    }

    [RelayCommand(CanExecute = nameof(HasUserFavoriteSelected))]
    private async Task RemoveFavorite()
    {
        if (CurrentFavorite?.IsUserFavorite != true)
        {
            return;
        }

        await favoritesService.RemoveFavoriteAsync(CurrentFavorite);
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

        if (!await presentationService.ShowDeleteSelectedContentWindowAsync(CurrentItem))
        {
            return;
        }

        if (CurrentItem.IsFile)
        {
            await jobService.DeleteSingleFileAsync(CurrentItem.Name, CurrentItem.FullPath);
        }
        else
        {
            await jobService.DeleteSingleFileAsync(string.Empty, CurrentItem.FullPath + "%");
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

        var versionsToDelete = await presentationService.ShowDeleteBackupsWindowAsync(Versions.ToList());
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
            await backupService.UpdateVersionAsync(CurrentVersion.Id, new VersionDetails { Title = viewModel.Title, Description = viewModel.Description });
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

        var sourceFavorites = CurrentVersion.Sources.Split("|", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => NormalizeBrowserPath(x))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new BrowserFavoriteItem
            {
                Name = x[(x.LastIndexOf("\\") + 1)..],
                Path = x,
                IsUserFavorite = false
            });

        var savedFavorites = await favoritesService.GetFavoritesAsync();

        Favorites.Clear();
        foreach (var favorite in sourceFavorites.Concat(savedFavorites))
        {
            Favorites.Add(new BrowserFavoriteItem
            {
                Name = favorite.Name,
                Path = NormalizeBrowserPath(favorite.Path),
                IsUserFavorite = favorite.IsUserFavorite
            });
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
        var rootSplit = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

        var folderPath = new List<FileOrFolderItem>();
        for (var i = 0; i < rootSplit.Length; i++)
        {
            folderPath.Add(new FileOrFolderItem()
            {
                Name = rootSplit[i],
                DisplayName = rootSplit[i],
                FullPath = string.Join("\\", rootSplit[0..(i + 1)])
            });
        }

        // obtain child folders
        var folderList = (await queryManager.GetFolderListAsync(version, '\\' + path + @"\%"))
            .Where(x =>
            {
                var split = x.Split("\\", StringSplitOptions.RemoveEmptyEntries);
                return split.Length == rootSplit.Length + 1;
            })
            .Select(x => new FileOrFolderItem()
            {
                Name = x[(x.LastIndexOf("\\") + 1)..],
                DisplayName = x[(x.LastIndexOf("\\") + 1)..],
                FullPath = x
            })
            .ToList();

        // obtain files in folder
        var fileList = (await queryManager.GetFilesByVersionAsync(version, '\\' + path + '\\'))
            .Select(x => new FileOrFolderItem()
            {
                Name = x.FileName,
                DisplayName = x.FileName,
                FullPath = x.FilePath,
                IsFile = true,
                FileNameOnDrive = x.FileName,

                FileDateModified = x.FileDateModified,
                FileDateCreated = x.FileDateCreated,
                FileSize = x.FileSize
            })
            .ToList();

        foreach (var file in fileList)
        {
            file.Icon16 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive);
            file.Icon64 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive, 64);
        }

        if (!IsCurrentContentRequest(requestId))
        {
            return;
        }

        contentMode = BrowserContentMode.Folder;
        CurrentItem = null;
        CurrentFolderPath.Clear();
        folderPath.ForEach(CurrentFolderPath.Add);
        ReplaceItems(folderList.Concat(fileList));
        NotifyContentCommandsChanged();
    }

    private async Task LoadSearchResultsAsync(string version, string searchTerms, long requestId)
    {
        var fileList = (await queryManager.SearchFilesByVersionAsync(version, searchTerms))
            .Select(x => new FileOrFolderItem()
            {
                Name = x.FileName,
                DisplayName = $"{x.FileName} - {x.FilePath.Trim('\\')}",
                FullPath = x.FilePath,
                IsFile = true,
                FileNameOnDrive = x.FileName,

                FileDateModified = x.FileDateModified,
                FileDateCreated = x.FileDateCreated,
                FileSize = x.FileSize
            })
            .ToList();

        foreach (var file in fileList)
        {
            file.Icon16 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive);
            file.Icon64 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive, 64);
        }

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
        AddFolderToFavoritesCommand.NotifyCanExecuteChanged();
        NavigateToPreviousVersionCommand.NotifyCanExecuteChanged();
        NavigateToNextVersionCommand.NotifyCanExecuteChanged();
        DeleteSelectedContentCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddFolderToFavorites() => GetFolderForFavorite() != null;

    private FileOrFolderItem? GetFolderForFavorite()
    {
        if (CurrentItem?.IsFile == false)
        {
            return CurrentItem;
        }

        return CurrentFolderPath.LastOrDefault();
    }

    private static bool PathsMatch(string left, string right)
    {
        return NormalizeBrowserPath(left).Equals(NormalizeBrowserPath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeBrowserPath(string path)
    {
        path ??= string.Empty;
        path = path.Trim('\\');
        if (path.Contains(':'))
        {
            path = path[(path.LastIndexOf("\\") + 1)..];
        }

        return path;
    }

    public async void OnNavigatedTo(object parameter)
    {
        LoadVersions();

        if (Versions.Count > 0)
        {
            CurrentVersion = Versions[0];
            await LoadVersion();
        }
    }

    public void OnNavigatedFrom()
    {
        FileSystemIconHelpers.ClearIconCache();
    }

    private enum BrowserContentMode
    {
        Folder,
        Search
    }
}
