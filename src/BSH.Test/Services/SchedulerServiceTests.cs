// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;
using Quartz;
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

    private static IEnumerable<TestCaseData> Registrations()
    {
        var time = DateTime.Now.AddHours(1);
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleAutoBackup(() => { }))).SetArgDisplayNames("Auto");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleOnce(() => { }, time))).SetArgDisplayNames("Once");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleHourly(() => { }, time))).SetArgDisplayNames("Hourly");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleDaily(() => { }, time))).SetArgDisplayNames("Daily");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleWeekly(() => { }, time))).SetArgDisplayNames("Weekly");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleMonthly(() => { }, time))).SetArgDisplayNames("Monthly");
    }

    [TestCaseSource(nameof(Registrations))]
    public void RegistrationCompletesBeforeReturning(Action<SchedulerService> register)
    {
        var service = new SchedulerService();
        service.Start();
        try
        {
            var scheduler = new StdSchedulerFactory().GetScheduler("QuartzScheduler").GetAwaiter().GetResult();
            var listener = new AsynchronousRegistrationListener();
            scheduler.ListenerManager.AddSchedulerListener(listener);

            register(service);

            Assert.That(listener.Completed, Is.True, "Registration must observe the asynchronous Quartz completion.");
            Assert.That(service.GetNextRun(), Is.LessThan(DateTime.Now.AddYears(1)));
        }
        finally
        {
            service.Stop();
        }
    }

    [TestCaseSource(nameof(Registrations))]
    public void RegistrationPropagatesSchedulerFailure(Action<SchedulerService> register)
    {
        var service = new SchedulerService();
        service.Start();
        try
        {
            var scheduler = new StdSchedulerFactory().GetScheduler("QuartzScheduler").GetAwaiter().GetResult();
            scheduler.Shutdown().GetAwaiter().GetResult();

            Assert.Throws<SchedulerException>(() => register(service));
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
                scheduler.ListenerManager.AddSchedulerListener(new AsynchronousRegistrationListener());
                foreach (var registration in Registrations())
                {
                    ((Action<SchedulerService>)registration.Arguments[0])(service);
                }
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

    private sealed class AsynchronousRegistrationListener : SchedulerListenerSupport
    {
        public bool Completed { get; private set; }

        public override async Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
            Completed = true;
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
