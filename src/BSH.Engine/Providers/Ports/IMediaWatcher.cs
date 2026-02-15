// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Providers.Ports;

public interface IMediaWatcher
{
    event EventHandler<string> DeviceAdded;

    void StartWatching();

    void StopWatching();
}
