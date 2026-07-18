// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Providers.Ports;

using Brightbits.BSH.Engine.Types;
namespace Brightbits.BSH.Engine.Providers.Storage;

public class StorageFactory : IStorageFactory
{
    private readonly IConfigurationManager configurationManager;

    public StorageFactory(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public IStorageProvider GetCurrentStorageProvider()
    {
        if (configurationManager.MediumType != MediaType.FileTransferServer)
        {
            return new FileSystemStorage(configurationManager);
        }
        else
        {
            return new FtpStorage(configurationManager);
        }
    }
}
