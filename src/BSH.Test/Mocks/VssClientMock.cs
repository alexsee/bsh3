// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Providers.Ports;
using System.Collections.Generic;
using System.IO;

namespace BSH.Test.Mocks;

public class VssClientMock : IVssClient
{
    private readonly bool shouldCopy;

    public int CopyCalls { get; private set; }

    public List<string> DestinationFiles { get; } = [];

    public string SnapshotContent { get; set; }

    public bool ThrowAfterCopy { get; set; }

    public VssClientMock(bool shouldCopy = false)
    {
        this.shouldCopy = shouldCopy;
    }

    public bool CopyFile(string fileName, string destFileName)
    {
        CopyCalls++;
        DestinationFiles.Add(destFileName);

        if (shouldCopy && File.Exists(fileName))
        {
            if (SnapshotContent != null)
            {
                File.WriteAllText(destFileName, SnapshotContent);
            }
            else
            {
                File.Copy(fileName, destFileName, true);
            }

            if (ThrowAfterCopy)
            {
                throw new IOException("Simulated VSS failure after writing plaintext.");
            }
            return true;
        }

        return false;
    }
}
