// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test.Services;

/// <summary>
/// Covers the hardened <see cref="UsbWatchService"/> start/stop lifecycle.
/// Start failures (e.g. no WMI on this machine) are swallowed by design.
/// </summary>
public class UsbWatchServiceTests
{
    [Test]
    public void StartAndStopWatchingDoesNotThrow()
    {
        var service = new UsbWatchService();

        Assert.DoesNotThrow(() => service.StartWatching());
        Assert.DoesNotThrow(() => service.StopWatching());
    }

    [Test]
    public void StopWithoutStartDoesNotThrow()
    {
        var service = new UsbWatchService();

        Assert.DoesNotThrow(() => service.StopWatching());
    }
}
