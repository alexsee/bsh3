// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface IBrowserDialogService
{
    Task<IReadOnlyList<string>> ShowDeleteBackupsWindowAsync(IReadOnlyList<VersionDetails> versions);

    /// <summary>
    /// Asks the user how to delete the selected file or folder from backups.
    /// </summary>
    /// <param name="item">The selected browser item.</param>
    /// <param name="versions">Candidate backup versions (typically those containing the item).</param>
    Task<DeleteSelectedContentResult> ShowDeleteSelectedContentWindowAsync(
        FileOrFolderItem item,
        IReadOnlyList<VersionDetails> versions);

    Task ShowFileDetailsAsync(FileDetails fileDetails);
    Task<string?> ShowRenameFavoriteWindowAsync(BrowserFavoriteItem favorite);

    /// <summary>
    /// Opens a folder picker for an alternate restore destination.
    /// Returns the selected path, or null if the user cancels or an error was already shown.
    /// </summary>
    Task<string?> PickRestoreDestinationFolderAsync();
}
