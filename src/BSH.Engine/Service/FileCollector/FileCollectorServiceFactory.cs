// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Service.Contracts;

namespace Brightbits.BSH.Engine.Service.FileCollector;

public class FileCollectorServiceFactory : IFileCollectorServiceFactory
{
    public IFileCollectorService Create()
    {
        return new FileCollectorService();
    }
}
