// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test.Services;

/// <summary>
/// Covers the hardened synchronous <see cref="SchedulerService"/> lifecycle
/// (no <c>Task.Run(...).Wait()</c> deadlocks, null-safe trigger reads).
/// </summary>
public class SchedulerServiceTests
{
    [Test]
    public void StartScheduleAndStopRoundTrip()
    {
        var service = new SchedulerService();

        service.Start();
        try
        {
            service.ScheduleOnce(() => { }, DateTime.Now.AddHours(1));

            var nextRun = service.GetNextRun();

            Assert.That(nextRun, Is.EqualTo(DateTime.Now.AddHours(1)).Within(TimeSpan.FromMinutes(2)));
        }
        finally
        {
            service.Stop();
        }
    }

    [Test]
    public void GetNextRunWithoutTriggersReturnsDistantFuture()
    {
        var service = new SchedulerService();

        service.Start();
        try
        {
            Assert.That(service.GetNextRun(), Is.GreaterThan(DateTime.Now.AddYears(1)));
        }
        finally
        {
            service.Stop();
        }
    }

    [Test]
    public void StopWithoutStartDoesNotThrow()
    {
        var service = new SchedulerService();

        Assert.DoesNotThrow(() => service.Stop());
    }
}
