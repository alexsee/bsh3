// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;

namespace BSH.Test.Mocks;
public class StorageFactoryMock : IStorageFactory
{
    public IStorageProvider GetCurrentStorageProvider() => new StorageMock();
}
