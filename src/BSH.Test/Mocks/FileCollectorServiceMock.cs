﻿using System.Collections.Generic;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;

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

    public List<FileTableRow> GetLocalFileList(string root, bool subFolders = true)
    {
        return localFiles;
    }
}
