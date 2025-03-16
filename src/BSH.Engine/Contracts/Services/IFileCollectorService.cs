// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services.FileCollector;

namespace Brightbits.BSH.Engine.Contracts.Services;
public interface IFileCollectorService
{
    List<FolderTableRow> EmptyFolders
    {
        get;
        set;
    }

    List<IFileExclusion> FileExclusionHandlers
    {
        get; set;
    }

    List<IFolderExclusion> FolderExclusionHandlers
    {
        get; set;
    }

    List<FileTableRow> GetLocalFileList(string root, bool subFolders = true);
}