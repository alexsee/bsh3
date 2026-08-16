// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Providers.Ports;

namespace BSH.Test.Mocks;

public class StorageFactoryMock : IStorageFactory
{
    private readonly IStorageProvider storageProvider;

    public StorageFactoryMock(IStorageProvider storageProvider = null)
    {
        this.storageProvider = storageProvider ?? new StorageMock();
    }

    public IStorageProvider GetCurrentStorageProvider() => storageProvider;
}
