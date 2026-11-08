// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Listener;
using Quartz.Spi;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace BSH.Test.Services;

/// <summary>
/// Covers the hardened synchronous <see cref="SchedulerService"/> lifecycle,
/// next-run queries, and awaited scheduled actions.
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
            service.ScheduleOnce(() => Task.CompletedTask, DateTime.Now.AddHours(1));

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
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleOnce(() => Task.CompletedTask, time))).SetArgDisplayNames("Once");
        foreach (var registration in RecurringRegistrations())
        {
            yield return registration;
        }
    }

    private static IEnumerable<TestCaseData> RecurringRegistrations()
    {
        var time = DateTime.Now.AddHours(1);
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleAutoBackup(() => Task.CompletedTask))).SetArgDisplayNames("Auto");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleHourly(() => Task.CompletedTask, time))).SetArgDisplayNames("Hourly");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleDaily(() => Task.CompletedTask, time))).SetArgDisplayNames("Daily");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleWeekly(() => Task.CompletedTask, time))).SetArgDisplayNames("Weekly");
        yield return new TestCaseData((Action<SchedulerService>)(service => service.ScheduleMonthly(() => Task.CompletedTask, time))).SetArgDisplayNames("Monthly");
    }

    [TestCaseSource(nameof(RecurringRegistrations))]
    public async Task MissedRecurringSlotAdvancesToFutureScheduledTime(Action<SchedulerService> register)
    {
        var service = new SchedulerService();
        service.Start();
        try
        {
            var scheduler = await new StdSchedulerFactory().GetScheduler("QuartzScheduler");
            await scheduler.Standby();
            register(service);

            var keys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            Assert.That(keys, Has.Count.EqualTo(1));
            var trigger = (IOperableTrigger)await scheduler.GetTrigger(keys.Single());
            var now = DateTimeOffset.UtcNow;
            // Exercise Quartz's misfire recovery on the actual registered trigger,
            // without waiting for an hourly (or monthly) slot to become overdue.
            trigger.SetNextFireTimeUtc(now.AddDays(-32));
            var originalClock = SystemTime.UtcNow;
            try
            {
                SystemTime.UtcNow = () => now;
                trigger.UpdateAfterMisfire(null);
            }
            finally
            {
                SystemTime.UtcNow = originalClock;
            }

            Assert.That(trigger.GetNextFireTimeUtc(), Is.GreaterThan(now),
                "A missed recurring backup must not fire immediately when the worker becomes free.");
            Assert.That(trigger.GetNextFireTimeUtc(), Is.EqualTo(trigger.GetFireTimeAfter(now)));
        }
        finally
        {
            service.Stop();
        }
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

    [Test]
    public async Task ScheduledActionRemainsExecutingUntilItCompletes()
    {
        var observation = await ObserveScheduledActionAsync(() => Task.CompletedTask);

        Assert.That(observation.ExecutingJobCount, Is.EqualTo(1));
        Assert.That(observation.ContinuedPastAwaitBeforeRelease, Is.False);
        Assert.That(observation.ContinuedPastAwait, Is.True);
        Assert.That(observation.JobException, Is.Null);
        Assert.That(observation.Sink.ErrorCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExceptionAfterAwaitIsLoggedAndHandled()
    {
        var observation = await ObserveScheduledActionAsync(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("backup failed after await");
        });

        Assert.That(observation.ExecutingJobCount, Is.EqualTo(1));
        Assert.That(observation.ContinuedPastAwaitBeforeRelease, Is.False);
        Assert.That(observation.ContinuedPastAwait, Is.True);
        Assert.That(observation.JobException, Is.Null);
        Assert.That(
            observation.Sink.Contains("Scheduled backup failed.", typeof(InvalidOperationException), "backup failed after await"),
            Is.True);
    }

    private static async Task<ScheduledActionObservation> ObserveScheduledActionAsync(Func<Task> afterAwait)
    {
        var previousLogger = Log.Logger;
        var sink = new CollectingLogSink();
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink).CreateLogger();

        var service = new SchedulerService();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var continuedPastAwait = false;
        var listener = new JobCompletionListener();
        var executingJobCount = 0;
        var continuedPastAwaitBeforeRelease = false;

        service.Start();
        try
        {
            var scheduler = await new StdSchedulerFactory().GetScheduler("QuartzScheduler");
            scheduler.ListenerManager.AddJobListener(listener);

            service.ScheduleOnce(async () =>
            {
                started.TrySetResult();
                await release.Task.ConfigureAwait(false);
                continuedPastAwait = true;
                await afterAwait().ConfigureAwait(false);
            }, DateTime.Now.AddSeconds(-5));

            var startedOrTimeout = await Task.WhenAny(started.Task, Task.Delay(TimeSpan.FromSeconds(15)));
            Assert.That(startedOrTimeout, Is.SameAs(started.Task), "Scheduled action did not start.");

            executingJobCount = (await scheduler.GetCurrentlyExecutingJobs()).Count;
            continuedPastAwaitBeforeRelease = continuedPastAwait;
            release.TrySetResult();

            var completedOrTimeout = await Task.WhenAny(listener.Completed, Task.Delay(TimeSpan.FromSeconds(15)));
            Assert.That(completedOrTimeout, Is.SameAs(listener.Completed), "Quartz job did not finish after the action completed.");

            return new ScheduledActionObservation
            {
                ExecutingJobCount = executingJobCount,
                ContinuedPastAwaitBeforeRelease = continuedPastAwaitBeforeRelease,
                ContinuedPastAwait = continuedPastAwait,
                JobException = await listener.Completed,
                Sink = sink
            };
        }
        finally
        {
            release.TrySetResult();
            service.Stop();
            Log.Logger = previousLogger;
        }
    }

    private sealed class ScheduledActionObservation
    {
        public int ExecutingJobCount { get; init; }
        public bool ContinuedPastAwaitBeforeRelease { get; init; }
        public bool ContinuedPastAwait { get; init; }
        public JobExecutionException JobException { get; init; }
        public CollectingLogSink Sink { get; init; }
    }

    private sealed class JobCompletionListener : JobListenerSupport
    {
        private readonly TaskCompletionSource<JobExecutionException> completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override string Name => "ScheduledActionCompletion";

        public Task<JobExecutionException> Completed => completed.Task;

        public override Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            completed.TrySetResult(jobException);
            return Task.CompletedTask;
        }
    }

    private sealed class CollectingLogSink : ILogEventSink
    {
        private readonly ConcurrentQueue<LogEvent> events = new();

        public int ErrorCount
        {
            get
            {
                var count = 0;
                foreach (var logEvent in events)
                {
                    if (logEvent.Level >= LogEventLevel.Error)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void Emit(LogEvent logEvent) => events.Enqueue(logEvent);

        public bool Contains(string message, Type exceptionType, string exceptionMessage)
        {
            foreach (var logEvent in events)
            {
                if (logEvent.Exception != null
                    && exceptionType.IsInstanceOfType(logEvent.Exception)
                    && logEvent.Exception.Message == exceptionMessage
                    && logEvent.RenderMessage() == message)
                {
                    return true;
                }
            }

            return false;
        }
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
