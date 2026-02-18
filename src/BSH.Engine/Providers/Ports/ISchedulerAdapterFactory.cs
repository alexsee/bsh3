// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Providers.Ports;

public interface ISchedulerAdapterFactory
{
    ISchedulerAdapter Create();
}
