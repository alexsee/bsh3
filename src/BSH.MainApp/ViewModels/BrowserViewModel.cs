using System.Collections.ObjectModel;
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

    public ObservableCollection<FileOrFolderItem> CurrentFolderPath { get; } = new();

    public ObservableCollection<string> Favorites { get; } = new();

    public ObservableCollection<FileOrFolderItem> Items { get; } = new();

    public ObservableCollection<VersionDetails> Versions { get; } = new();

    public ICommand LoadVersionCommand
    {
        get;
    }

    public ICommand LoadFolderCommand
    {
        get;
    }

    public BrowserViewModel(IQueryManager queryManager)
    {
        this.queryManager = queryManager;

        this.LoadVersionCommand = new AsyncRelayCommand(LoadVersionCommandAsync);
        this.LoadFolderCommand = new AsyncRelayCommand(LoadFolderCommandAsync);
    }

    private async Task LoadVersionCommandAsync()
    {
        var sources = CurrentVersion.Sources.Split("|")
            .Select(x => x[(x.LastIndexOf("\\") + 1)..])
            .ToList();

        this.Favorites.Clear();
        sources.ForEach(x => this.Favorites.Add(x));

        await LoadFolderAsync(CurrentVersion.Id, sources[0]);
    }

    private async Task LoadFolderCommandAsync()
    {
        if (CurrentItem != null && !CurrentItem.IsFile)
        {
            await LoadFolderAsync(CurrentVersion.Id, CurrentItem.FullPath);
        }
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
