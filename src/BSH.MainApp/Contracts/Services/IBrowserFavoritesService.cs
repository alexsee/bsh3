// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface IBrowserFavoritesService
{
    Task<IReadOnlyList<BrowserFavoriteItem>> GetFavoritesAsync();
    Task AddFavoriteAsync(BrowserFavoriteItem favorite);
    Task RenameFavoriteAsync(BrowserFavoriteItem favorite, string name);
    Task RemoveFavoriteAsync(BrowserFavoriteItem favorite);
}
