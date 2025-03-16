// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.Service.Shared;
using BSH.Service.VSS;

namespace BSH.Service;

public class VSSService : IVSSRemoteObject
{
    private Exception exception;

    public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination)
    {
        var fileInfo = new FileInfo(source);

        try
        {
            using var vss = new VssBackup();
            vss.Setup(fileInfo.Directory.Root.Name);

            var snapshotPath = vss.GetSnapshotPath(source);
            XCopy.Copy(snapshotPath, destination, true, false);

            return true;
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }

        return false;
    }

    public Exception GetException()
    {
        return this.exception;
    }
}
