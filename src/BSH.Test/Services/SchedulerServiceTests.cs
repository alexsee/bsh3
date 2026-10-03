// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;
using Quartz.Impl;
using Quartz.Listener;

namespace BSH.Test.Services;

/// <summary>
/// Covers the hardened synchronous <see cref="SchedulerService"/> lifecycle
/// and next-run queries.
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

    [Test]
    public void LifecycleCompletesWithoutPumpingCallerSynchronizationContext()
    {
        var context = new QueuedSynchronizationContext();
        Exception failure = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(context);
            var service = new SchedulerService();
            try
            {
                service.Start();
                var scheduler = new StdSchedulerFactory().GetScheduler("QuartzScheduler").GetAwaiter().GetResult();
                scheduler.ListenerManager.AddSchedulerListener(new AsynchronousShutdownListener());
                service.GetNextRun();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(context);
                try { service.Stop(); }
                catch (Exception ex) { failure = ex; }
            }
        }) { IsBackground = true };

        thread.Start();
        var completed = thread.Join(TimeSpan.FromSeconds(5));
        // Release a captured continuation on failure so the scheduler is cleaned up.
        if (!completed)
        {
            context.RunPendingCallbacks();
            thread.Join(TimeSpan.FromSeconds(5));
        }

        Assert.That(completed, Is.True, "Scheduler lifecycle must not require the blocked caller to pump callbacks.");
        Assert.That(failure, Is.Null);
    }

    private sealed class QueuedSynchronizationContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<Action> callbacks = new();

        public override void Post(SendOrPostCallback callback, object state)
        {
            callbacks.Enqueue(() => callback(state));
        }

        public void RunPendingCallbacks()
        {
            while (callbacks.TryDequeue(out var callback))
            {
                callback();
            }
        }
    }

    private sealed class AsynchronousShutdownListener : SchedulerListenerSupport
    {
        public override Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
        {
            return Task.Delay(50, cancellationToken);
        }
    }
}
