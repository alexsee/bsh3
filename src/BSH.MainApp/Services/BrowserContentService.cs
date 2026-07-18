// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.Utils;

namespace BSH.MainApp.Services;

public class BrowserContentService : IBrowserContentService
{
    private readonly IQueryManager queryManager;
    private readonly IBrowserFavoritesService favoritesService;

    public BrowserContentService(IQueryManager queryManager, IBrowserFavoritesService favoritesService)
    {
        this.queryManager = queryManager;
        this.favoritesService = favoritesService;
    }

    public async Task<IReadOnlyList<BrowserFavoriteItem>> GetFavoritesAsync(VersionDetails version)
    {
        var sourceFavorites = version.Sources.Split("|", StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeBrowserPath)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => new BrowserFavoriteItem
            {
                Name = x[(x.LastIndexOf("\\") + 1)..],
                Path = x,
                IsUserFavorite = false
            });

        var savedFavorites = await favoritesService.GetFavoritesAsync();

        return sourceFavorites.Concat(savedFavorites)
            .Select(x => new BrowserFavoriteItem
            {
                Name = x.Name,
                Path = NormalizeBrowserPath(x.Path),
                IsUserFavorite = x.IsUserFavorite
            })
            .ToList();
    }

    public async Task<BrowserContentSnapshot> LoadFolderAsync(string version, string path)
    {
        var rootSplit = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);
        var folderPath = BuildFolderPath(rootSplit);
        var folderList = await LoadChildFoldersAsync(version, path, rootSplit.Length);
        var fileList = await LoadFilesAsync(version, path);

        return new BrowserContentSnapshot
        {
            FolderPath = folderPath,
            Items = folderList.Concat(fileList).ToList()
        };
    }

    public async Task<IReadOnlyList<FileOrFolderItem>> SearchFilesAsync(string version, string searchTerms)
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

        await LoadIconsAsync(fileList);
        return fileList;
    }

    public async Task AddFavoriteAsync(FileOrFolderItem folder)
    {
        await favoritesService.AddFavoriteAsync(new BrowserFavoriteItem
        {
            Name = folder.Name,
            Path = folder.FullPath,
            IsUserFavorite = true
        });
    }

    public async Task RenameFavoriteAsync(BrowserFavoriteItem favorite, string name)
    {
        await favoritesService.RenameFavoriteAsync(favorite, name);
    }

    public async Task RemoveFavoriteAsync(BrowserFavoriteItem favorite)
    {
        await favoritesService.RemoveFavoriteAsync(favorite);
    }

    public void ClearIconCache()
    {
        FileSystemIconHelpers.ClearIconCache();
    }

    public FileOrFolderItem? GetFolderForFavorite(FileOrFolderItem? currentItem, IReadOnlyList<FileOrFolderItem> currentFolderPath)
    {
        if (currentItem?.IsFile == false)
        {
            return currentItem;
        }

        return currentFolderPath.LastOrDefault();
    }

    public bool PathsMatch(string left, string right)
    {
        return NormalizeBrowserPath(left).Equals(NormalizeBrowserPath(right), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyList<FileOrFolderItem>> LoadChildFoldersAsync(string version, string path, int rootDepth)
    {
        return (await queryManager.GetFolderListAsync(version, '\\' + path + @"\%"))
            .Where(x => x.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length == rootDepth + 1)
            .Select(x => new FileOrFolderItem()
            {
                Name = x[(x.LastIndexOf("\\") + 1)..],
                DisplayName = x[(x.LastIndexOf("\\") + 1)..],
                FullPath = x
            })
            .ToList();
    }

    private async Task<IReadOnlyList<FileOrFolderItem>> LoadFilesAsync(string version, string path)
    {
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

        await LoadIconsAsync(fileList);
        return fileList;
    }

    private static List<FileOrFolderItem> BuildFolderPath(string[] rootSplit)
    {
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

        return folderPath;
    }

    private static async Task LoadIconsAsync(IEnumerable<FileOrFolderItem> fileList)
    {
        foreach (var file in fileList)
        {
            file.Icon16 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive);
            file.Icon64 = await FileSystemIconHelpers.GetFileIconAsync(file.FileNameOnDrive, 64);
        }
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
}
