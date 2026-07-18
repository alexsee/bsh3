// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Service.FileCollector;

namespace Brightbits.BSH.Engine.Service.Contracts;

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