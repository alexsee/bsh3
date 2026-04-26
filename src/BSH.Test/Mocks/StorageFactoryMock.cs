// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;

namespace BSH.Test.Mocks;

public class StorageFactoryMock : IStorageFactory
{
    private readonly Func<IStorageProvider> factory;

    public StorageFactoryMock()
        : this(() => new StorageMock())
    {
    }

    public StorageFactoryMock(IStorageProvider storageProvider)
        : this(() => storageProvider)
    {
    }

    public StorageFactoryMock(Func<IStorageProvider> factory)
    {
        this.factory = factory;
    }

    public IStorageProvider GetCurrentStorageProvider() => factory();
}
