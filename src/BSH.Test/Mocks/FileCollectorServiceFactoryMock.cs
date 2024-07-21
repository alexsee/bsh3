using System.Collections.Generic;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Models;

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
