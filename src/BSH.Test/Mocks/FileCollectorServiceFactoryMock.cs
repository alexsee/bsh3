// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Brightbits.BSH.Engine.Service.Contracts;
using Brightbits.BSH.Engine.Types;

namespace BSH.Test.Mocks;
public class FileCollectorServiceFactoryMock : IFileCollectorServiceFactory
{
    private readonly List<FolderTableRow> localFolders;
    private readonly List<FileTableRow> localFiles;

    public FileCollectorServiceFactoryMock(List<FolderTableRow> localFolders, List<FileTableRow> localFiles)
    {
        this.localFolders = localFolders;
        this.localFiles = localFiles;
    }

    public IFileCollectorService Create() => new FileCollectorServiceMock(localFolders, localFiles);
}
