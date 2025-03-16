// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Models;

public class FolderTableRow
{
    public string Folder
    {
        get; set;
    }

    public string RootPath
    {
        get; set;
    }

    public FolderTableRow(string folder, string rootPath)
    {
        Folder = folder;
        RootPath = rootPath;
    }
}
