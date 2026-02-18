// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Services;

public sealed class SchedulerAdapterFactory : ISchedulerAdapterFactory
{
    public ISchedulerAdapter Create()
    {
        return new SchedulerService();
    }
}
