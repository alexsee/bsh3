// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Utils;

namespace Brightbits.BSH.Engine.Services;

public sealed class ScheduledBackupRegistration
{
    private readonly IConfigurationManager configurationManager;
    private readonly IQueryManager queryManager;

    public ScheduledBackupRegistration(IConfigurationManager configurationManager, IQueryManager queryManager)
    {
        this.configurationManager = configurationManager;
        this.queryManager = queryManager;
    }

    public void Register(ScheduleEntry schedule, ISchedulerAdapter scheduler, Func<Task> runBackup,
        Action watchForDueBackup, DateTime now)
    {
        if (ShouldCatchUp(schedule, now))
        {
            scheduler.ScheduleOnce(runBackup, now.AddMinutes(1));
            watchForDueBackup();
        }

        switch (schedule.Type)
        {
            case 1 when now < schedule.Date:
                scheduler.ScheduleOnce(runBackup, schedule.Date);
                break;
            case 2:
                scheduler.ScheduleHourly(runBackup, schedule.Date);
                break;
            case 3:
                scheduler.ScheduleDaily(runBackup, schedule.Date);
                break;
            case 4:
                scheduler.ScheduleWeekly(runBackup, schedule.Date);
                break;
            case 5:
                scheduler.ScheduleMonthly(runBackup, schedule.Date);
                break;
        }
    }

    private bool ShouldCatchUp(ScheduleEntry schedule, DateTime now)
    {
        var date = schedule.Date;
        if (schedule.Type == 3
            || (schedule.Type == 4 && now.DayOfWeek == date.DayOfWeek)
            || (schedule.Type == 5 && now.Day == date.Day))
        {
            return now.TimeOfDay > date.TimeOfDay && DoPastBackup(now.Date.Add(date.TimeOfDay));
        }

        if (schedule.Type != 4 && schedule.Type != 5)
        {
            return false;
        }

        var checkDate = schedule.Type == 4
            ? DateUtils.GetDateToWeekDay(date.DayOfWeek, now)
            : DateUtils.GetDateToMonth(date.Day, now);

        foreach (var versionDate in queryManager.GetVersions(true).Select(x => x.CreationDate))
        {
            // Preserve whole-day truncation and ordered history checks used by both shells.
            if (versionDate.Subtract(checkDate).Days < 0)
            {
                return DoPastBackup(now.Date.Add(date.TimeOfDay));
            }

            if (schedule.Type == 4 ? versionDate.DayOfWeek == date.DayOfWeek : versionDate.Day == date.Day)
            {
                return false;
            }
        }

        return false;
    }

    private bool DoPastBackup(DateTime date)
    {
        if (configurationManager.DoPastBackups != "1")
        {
            return false;
        }

        if (!string.IsNullOrEmpty(configurationManager.LastBackupDone)
            && DateTime.ParseExact(configurationManager.LastBackupDone, "dd-MM-yyyy HH-mm-ss", CultureInfo.InvariantCulture) >= date)
        {
            return false;
        }

        return !queryManager.GetVersions().Any(version => version.CreationDate >= date);
    }
}
