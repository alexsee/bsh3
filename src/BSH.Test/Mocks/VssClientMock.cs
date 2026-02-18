// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Providers.Ports;

namespace BSH.Test.Mocks;

public class VssClientMock : IVssClient
{
    public bool CopyFile(string fileName, string destFileName)
    {
        return false;
    }
}
