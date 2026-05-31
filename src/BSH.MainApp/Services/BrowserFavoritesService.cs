// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;

public class BrowserFavoritesService : IBrowserFavoritesService
{
    private const string SettingsKey = "BrowserFolderFavorites";
    private readonly ILocalSettingsService localSettingsService;

    public BrowserFavoritesService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
    }

    public async Task<IReadOnlyList<BrowserFavoriteItem>> GetFavoritesAsync()
    {
        return await localSettingsService.ReadSettingAsync<List<BrowserFavoriteItem>>(SettingsKey) ?? [];
    }

    public async Task AddFavoriteAsync(BrowserFavoriteItem favorite)
    {
        var favorites = (await GetFavoritesAsync()).ToList();
        var normalizedPath = NormalizePath(favorite.Path);
        if (favorites.Any(x => NormalizePath(x.Path).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        favorites.Add(new BrowserFavoriteItem
        {
            Name = string.IsNullOrWhiteSpace(favorite.Name) ? Path.GetFileName(normalizedPath) : favorite.Name,
            Path = normalizedPath,
            IsUserFavorite = true
        });
        await SaveAsync(favorites);
    }

    public async Task RenameFavoriteAsync(BrowserFavoriteItem favorite, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var favorites = (await GetFavoritesAsync()).ToList();
        var saved = favorites.FirstOrDefault(x => SamePath(x.Path, favorite.Path));
        if (saved == null)
        {
            return;
        }

        saved.Name = name.Trim();
        await SaveAsync(favorites);
    }

    public async Task RemoveFavoriteAsync(BrowserFavoriteItem favorite)
    {
        var favorites = (await GetFavoritesAsync()).Where(x => !SamePath(x.Path, favorite.Path)).ToList();
        await SaveAsync(favorites);
    }

    private async Task SaveAsync(List<BrowserFavoriteItem> favorites)
    {
        await localSettingsService.SaveSettingAsync(SettingsKey, favorites);
    }

    private static bool SamePath(string left, string right)
    {
        return NormalizePath(left).Equals(NormalizePath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        return (path ?? string.Empty).Trim('\\');
    }
}
