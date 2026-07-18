// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Providers.Ports;
using System.IO;

namespace BSH.Test.Mocks;

public class VssClientMock : IVssClient
{
    private readonly bool shouldCopy;

    public int CopyCalls { get; private set; }

    public VssClientMock(bool shouldCopy = false)
    {
        this.shouldCopy = shouldCopy;
    }

    public bool CopyFile(string fileName, string destFileName)
    {
        CopyCalls++;

        if (shouldCopy && File.Exists(fileName))
        {
            File.Copy(fileName, destFileName, true);
            return true;
        }

        return false;
    }
}
