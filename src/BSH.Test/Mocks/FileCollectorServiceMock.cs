// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services.FileCollector;

namespace BSH.Test.Mocks;
public class FileCollectorServiceMock : IFileCollectorService
{
    private readonly List<FileTableRow> localFiles;



    public FileCollectorServiceMock(List<FolderTableRow> emptyFolders, List<FileTableRow> localFiles)
    {
        EmptyFolders = emptyFolders;
        this.localFiles = localFiles;
    }

    public List<FolderTableRow> EmptyFolders
    {
        get; set;
    }

    public List<IFileExclusion> FileExclusionHandlers
    {
        get; set;
    } = [];

    public List<IFolderExclusion> FolderExclusionHandlers
    {
        get; set;
    } = [];

    public List<FileTableRow> GetLocalFileList(string root, bool subFolders = true)
    {
        return localFiles.Where(x => !FileExclusionHandlers.Any(handler => handler.IsFileExcluded(x))).ToList();
    }
}
