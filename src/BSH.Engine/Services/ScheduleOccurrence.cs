// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Services;

/// <summary>
/// Predicts when schedule entries fire next, matching the triggers built by <see cref="SchedulerService"/>.
/// </summary>
public static class ScheduleOccurrence
{
    public static DateTime? GetNext(IEnumerable<ScheduleEntry> entries, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .Select(entry => GetNext(entry, now))
            .Where(next => next.HasValue)
            .OrderBy(next => next)
            .FirstOrDefault();
    }

    public static DateTime? GetNext(ScheduleEntry entry, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var date = entry.Date;
        var after = TruncateToMinute(now);

        switch ((ScheduleEntryKind)entry.Type)
        {
            case ScheduleEntryKind.Once:
                return date > now ? date : null;

            case ScheduleEntryKind.Hourly:
                var hourly = after.Date.AddHours(after.Hour).AddMinutes(date.Minute);
                return hourly > now ? hourly : hourly.AddHours(1);

            case ScheduleEntryKind.Daily:
                var daily = after.Date.Add(TimeOfDay(date));
                return daily > now ? daily : daily.AddDays(1);

            case ScheduleEntryKind.Weekly:
                var daysAhead = ((int)date.DayOfWeek - (int)after.DayOfWeek + 7) % 7;
                var weekly = after.Date.AddDays(daysAhead).Add(TimeOfDay(date));
                return weekly > now ? weekly : weekly.AddDays(7);

            case ScheduleEntryKind.Monthly:
                // Like the cron trigger, months without the configured day are skipped.
                for (var month = new DateTime(after.Year, after.Month, 1); month < after.AddYears(1).AddMonths(1); month = month.AddMonths(1))
                {
                    if (date.Day > DateTime.DaysInMonth(month.Year, month.Month))
                    {
                        continue;
                    }

                    var monthly = month.AddDays(date.Day - 1).Add(TimeOfDay(date));
                    if (monthly > now)
                    {
                        return monthly;
                    }
                }

                return null;

            default:
                return null;
        }
    }

    private static TimeSpan TimeOfDay(DateTime date) => new(date.Hour, date.Minute, 0);

    private static DateTime TruncateToMinute(DateTime value) => new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);
}
