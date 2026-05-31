// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface IBrowserDialogService
{
    Task<IReadOnlyList<string>> ShowDeleteBackupsWindowAsync(IReadOnlyList<VersionDetails> versions);
    Task<bool> ShowDeleteSelectedContentWindowAsync(FileOrFolderItem item);
    Task ShowFileDetailsAsync(FileDetails fileDetails);
    Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite);
}
