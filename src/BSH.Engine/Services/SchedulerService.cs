﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Brightbits.BSH.Engine.Services;

public class SchedulerService : ISchedulerService
{
    private IScheduler scheduler;

    public SchedulerService()
    {

    }

    public void Start()
    {
        Task.Run(async () =>
        {
            var properties = new NameValueCollection {
                { "quartz.threadPool.threadCount", "1" },
                { "quartz.threadPool.maxConcurrency", "1" }
            };

            var factory = new StdSchedulerFactory(properties);
            this.scheduler = await factory.GetScheduler();

            await this.scheduler.Start();
        }).Wait();
    }

    private static IJobDetail GetJob(Action action)
    {
        // define the job and tie it to our HelloJob class
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

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public void ScheduleOnce(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .StartAt(time)
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public void ScheduleHourly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithCronSchedule("0 " + time.Minute + " * * * ?")
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public void ScheduleDaily(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public void ScheduleWeekly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(time.DayOfWeek, time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public void ScheduleMonthly(Action action, DateTime time)
    {
        var trigger = TriggerBuilder.Create()
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(time.Day, time.Hour, time.Minute))
            .Build();

        scheduler.ScheduleJob(GetJob(action), trigger);
    }

    public DateTime GetNextRun()
    {
        var result = DateTimeOffset.MaxValue;

        Task.Run(async () =>
        {
            var allTriggerKeys = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
            foreach (var triggerKey in allTriggerKeys)
            {
                var trigger = await scheduler.GetTrigger(triggerKey);

                if (trigger.GetNextFireTimeUtc() < result)
                {
                    result = trigger.GetNextFireTimeUtc().Value;
                }
            }
        }).Wait();

        return result.LocalDateTime;
    }

    public void Stop()
    {
        if (scheduler == null)
        {
            return;
        }

        scheduler.Shutdown();
    }

    private sealed class RunActionJob : IJob
    {
        async Task IJob.Execute(IJobExecutionContext context)
        {
            await Task.Run(() =>
            {
                var action = context.MergedJobDataMap["action"] as Action;
                action();
            });
        }
    }
}
