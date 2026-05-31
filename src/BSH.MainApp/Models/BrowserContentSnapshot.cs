// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Models;

public class BrowserContentSnapshot
{
    public IReadOnlyList<FileOrFolderItem> FolderPath
    {
        get; init;
    } = [];

    public IReadOnlyList<FileOrFolderItem> Items
    {
        get; init;
    } = [];
}
