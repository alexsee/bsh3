// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Providers.Ports;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Brightbits.BSH.Engine.Services;

public class SchedulerService : ISchedulerAdapter
{
    private IScheduler scheduler;

    public void Start()
    {
        // The async bridges must not capture the context of a blocked UI caller.
        StartAsync().GetAwaiter().GetResult();
    }

    private async Task StartAsync()
    {
        if (this.scheduler != null && this.scheduler.IsShutdown)
        {
            this.scheduler = null;
        }

        if (this.scheduler == null)
        {
            var properties = new NameValueCollection {
                { "quartz.threadPool.threadCount", "1" }
            };

            var factory = new StdSchedulerFactory(properties);
            this.scheduler = await factory.GetScheduler().ConfigureAwait(false);
        }

        await this.scheduler.Start().ConfigureAwait(false);
    }

    private static IJobDetail GetJob(Action action)
    {
        var job = JobBuilder.Create<RunActionJob>()
            .SetJobData(new JobDataMap
            {
                {"action", action}
            })
            .Build();

        return job;
    }

    public void ScheduleAutoBackup(Action action)
    {
        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("0 0 * * * ?")
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public void ScheduleOnce(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .StartAt(time)
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public void ScheduleHourly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("0 " + time.Minute + " * * * ?")
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public void ScheduleDaily(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public void ScheduleWeekly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(time.DayOfWeek, time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public void ScheduleMonthly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(time.Day, time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();
    }

    public DateTime GetNextRun()
    {
        var result = GetNextRunAsync().GetAwaiter().GetResult();

        return result.LocalDateTime;
    }

    private async Task<DateTimeOffset> GetNextRunAsync()
    {
        var result = DateTimeOffset.MaxValue;

        var allTriggerKeys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).ConfigureAwait(false);
        foreach (var triggerKey in allTriggerKeys)
        {
            var trigger = await scheduler.GetTrigger(triggerKey).ConfigureAwait(false);
            var nextFireTime = trigger?.GetNextFireTimeUtc();

            if (nextFireTime.HasValue && nextFireTime.Value < result)
            {
                result = nextFireTime.Value;
            }
        }

        return result;
    }

    public void Stop()
    {
        if (scheduler == null)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();
    }

    private async Task StopAsync()
    {
        await scheduler.Shutdown().ConfigureAwait(false);
        scheduler = null;
    }

    private sealed class RunActionJob : IJob
    {
        async Task IJob.Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                var action = context.MergedJobDataMap["action"] as Action;
                action?.Invoke();
            });
        }
    }
}
