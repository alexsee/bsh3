// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.ViewModels;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BSH.MainApp.ViewModels;

public partial class BrowserViewModel : ObservableRecipient, INavigationAware
{
    private readonly IQueryManager queryManager;

    [ObservableProperty]
    private VersionDetails? currentVersion;

    [ObservableProperty]
    private FileOrFolderItem? currentItem;

    [ObservableProperty]
    private string blub;

    public ObservableCollection<FileOrFolderItem> CurrentFolderPath { get; } = new();

    public ObservableCollection<string> Favorites { get; } = new();

    public ObservableCollection<FileOrFolderItem> Items { get; } = new();

    public ObservableCollection<VersionDetails> Versions { get; } = new();

    public BrowserViewModel(IQueryManager queryManager)
    {
        this.queryManager = queryManager;
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
        sources.ForEach(x => this.Favorites.Add(x));

        await LoadFolderAsync(CurrentVersion.Id, sources[0]);
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
    private async Task LoadFolderWithParam(FileOrFolderItem selectedItem)
    {
        if (selectedItem == null || CurrentVersion == null)
        {
            return;
        }

        await LoadFolderAsync(CurrentVersion.Id, selectedItem.FullPath);
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
        var folderList = (await queryManager.GetFolderListAsync(version, @"\" + path + @"\%"))
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
        var fileList = (await queryManager.GetFilesByVersionAsync(version, @"\" + path + @"\"))
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

    public void OnNavigatedTo(object parameter)
    {
        LoadVersions();
        CurrentVersion = Versions[0];
    }

    public void OnNavigatedFrom()
    {
    }
}
