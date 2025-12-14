// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts.Services;

namespace Brightbits.BSH.Engine.Services.FileCollector;

public class FileCollectorServiceFactory : IFileCollectorServiceFactory
{
    public IFileCollectorService Create()
    {
        return new FileCollectorService();
    }
}
