// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels;

public partial class BrowserViewModel : ObservableObject, INavigationAware
{
    private readonly IQueryManager queryManager;
    private readonly IJobService jobService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
    private VersionDetails? currentVersion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RestoreFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestoreAllCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFilePreviewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFilePropertiesCommand))]
    private FileOrFolderItem? currentItem;

    [ObservableProperty]
    private string? currentFavorite;

    [ObservableProperty]
    private string searchTerms;

    [ObservableProperty]
    private bool toggleInfoPane = false;

    [ObservableProperty]
    private ObservableCollection<FileOrFolderItem> currentFolderPath = new();

    [ObservableProperty]
    private ObservableCollection<string> favorites = new();

    [ObservableProperty]
    private ObservableCollection<FileOrFolderItem> items = new();

    [ObservableProperty]
    private ObservableCollection<VersionDetails> versions = new();

    public BrowserViewModel(IQueryManager queryManager, IJobService jobService)
    {
        this.queryManager = queryManager;
        this.jobService = jobService;
    }

    [RelayCommand]
    private async Task LoadVersion()
    {
        if (CurrentVersion == null)
        {
            return;
        }

        var sources = CurrentVersion.Sources.Split("|")
            .Select(x => x[(x.LastIndexOf("\\") + 1)..])
            .ToList();

        this.Favorites.Clear();
        sources.ForEach(this.Favorites.Add);
        CurrentFavorite = sources[0];

        await LoadFolderAsync(CurrentVersion.Id, CurrentFavorite);
    }

    [RelayCommand]
    private async Task LoadFavorite()
    {
        if (CurrentVersion == null || CurrentFavorite == null)
        {
            return;
        }

        await LoadFolderAsync(CurrentVersion.Id, CurrentFavorite);
    }

    [RelayCommand]
    private async Task LoadFolder()
    {
        if (CurrentItem == null || CurrentItem.IsFile || CurrentVersion == null)
        {
            return;
        }

        // load folder
        await LoadFolderAsync(CurrentVersion.Id, CurrentItem.FullPath);
    }

    [RelayCommand(CanExecute = nameof(CanUpFolder))]
    private async Task UpFolder()
    {
        if (CurrentFolderPath.Count < 2 || CurrentVersion == null)
        {
            return;
        }

        // load parent folder
        await LoadFolderAsync(CurrentVersion.Id, CurrentFolderPath[^2].FullPath);
    }

    private bool CanUpFolder() => CurrentFolderPath.Count > 1;

    [RelayCommand]
    private async Task Refresh()
    {
        if (CurrentVersion == null && CurrentFolderPath.Count > 0)
        {
            return;
        }

        var version = CurrentVersion;
        LoadVersions();

        CurrentVersion = version;
        await LoadFolderAsync(version.Id, CurrentFolderPath[^1].FullPath);
    }

    [RelayCommand]
    private async Task LoadFolderWithParam(FileOrFolderItem selectedItem)
    {
        if (selectedItem == null || CurrentVersion == null)
        {
            return;
        }

        await LoadFolderAsync(CurrentVersion.Id, selectedItem.FullPath);
    }

    [RelayCommand(CanExecute = nameof(HasFileOrFolderSelected))]
    private async Task RestoreFile()
    {
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

    private bool HasFileOrFolderSelected() => CurrentItem != null && CurrentVersion != null;

    [RelayCommand(CanExecute = nameof(CanRestoreAll))]
    private async Task RestoreAll()
    {
        if (CurrentFolderPath[^1] == null)
        {
            return;
        }

        // restore all
        await jobService.RestoreBackupAsync(CurrentVersion.Id, CurrentFolderPath[^1].FullPath, "");
    }

    private bool CanRestoreAll() => CurrentVersion != null && CurrentFolderPath.Count > 0;

    [RelayCommand(CanExecute = nameof(HasFileSelected))]
    private async Task ShowFileProperties()
    {

    }

    private bool HasFileSelected() => CurrentItem != null && CurrentItem.IsFile;

    [RelayCommand(CanExecute = nameof(HasFileSelected))]
    private async Task ShowFilePreview()
    {

    }

    [RelayCommand]
    private async Task AddFolderToFavorites()
    {

    }

    [RelayCommand]
    private async Task EditBackup()
    {

    }

    [RelayCommand]
    private async Task DeleteBackup()
    {

    }

    [RelayCommand]
    private async Task DeleteBackups()
    {

    }

    [RelayCommand]
    private async Task LockBackup()
    {
    }

    private void LoadVersions()
    {
        var backupVersions = queryManager.GetVersions(true);

        Versions.Clear();
        backupVersions.ForEach(Versions.Add);
    }

    private async Task LoadFolderAsync(string version, string path)
    {
        var rootSplit = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

        CurrentFolderPath.Clear();
        for (var i = 0; i < rootSplit.Length; i++)
        {
            CurrentFolderPath.Add(new FileOrFolderItem()
            {
                Name = rootSplit[i],
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
                FullPath = x
            })
            .ToList();

        // obtain files in folder
        var fileList = (await queryManager.GetFilesByVersionAsync(version, '\\' + path + '\\'))
            .Select(x => new FileOrFolderItem()
            {
                Name = x.FileName,
                FullPath = x.FilePath,
                IsFile = true,

                FileDateModified = x.FileDateModified,
                FileDateCreated = x.FileDateCreated,
                FileSize = x.FileSize
            })
            .ToList();

        // merge lists
        Items.Clear();
        folderList.ForEach(Items.Add);
        fileList.ForEach(Items.Add);

        // notify UI about potential changes
        LoadVersionCommand.NotifyCanExecuteChanged();
        LoadFolderCommand.NotifyCanExecuteChanged();
        UpFolderCommand.NotifyCanExecuteChanged();
    }

    public async void OnNavigatedTo(object parameter)
    {
        LoadVersions();
        CurrentVersion = Versions[0];
        await LoadVersion();
    }

    public void OnNavigatedFrom()
    {
    }
}
