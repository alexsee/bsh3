// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Covers the hardened <see cref="NotificationController"/> status marshaling:
/// without a message loop the tray-control invoke fails and must stay silent.
/// </summary>
public class NotificationControllerTests
{
    [Test]
    public void ReportSystemStatusWithoutTrayIconDoesNotThrow()
    {
        Assert.DoesNotThrow(() => NotificationController.Current.ReportSystemStatus(SystemStatus.ACTIVATED));
    }
}
