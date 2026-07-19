// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Types;
using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface IBrowserContentService
{
    Task<IReadOnlyList<BrowserFavoriteItem>> GetFavoritesAsync(VersionDetails version);
    Task<BrowserContentSnapshot> LoadFolderAsync(string version, string path);
    Task<IReadOnlyList<FileOrFolderItem>> SearchFilesAsync(string version, string searchTerms);
    Task AddFavoriteAsync(FileOrFolderItem folder);
    Task RenameFavoriteAsync(BrowserFavoriteItem favorite, string name);
    Task RemoveFavoriteAsync(BrowserFavoriteItem favorite);
    void ClearIconCache();
    FileOrFolderItem? GetFolderForFavorite(FileOrFolderItem? currentItem, IReadOnlyList<FileOrFolderItem> currentFolderPath);
    bool PathsMatch(string left, string right);
}
