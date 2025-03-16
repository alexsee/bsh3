// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Storage;

namespace Brightbits.BSH.Engine.Contracts.Storage;

public interface IStorageFactory
{
    IStorage GetCurrentStorageProvider();
}