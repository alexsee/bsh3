// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Storage;

public class StorageFactory : IStorageFactory
{
    private readonly IConfigurationManager configurationManager;

    public StorageFactory(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public IStorageProvider GetCurrentStorageProvider()
    {
        return configurationManager.MediumType switch
        {
            MediaType.FileTransferServer => new FtpStorage(configurationManager),
            MediaType.WebDav => new WebDavStorage(configurationManager),
            _ => new FileSystemStorage(configurationManager),
        };
    }
}
