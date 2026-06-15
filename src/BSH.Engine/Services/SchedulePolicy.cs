// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Services;

public sealed class SchedulePolicy
{
    private readonly ScheduleSettings settings;

    public SchedulePolicy(ScheduleSettings settings)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public bool ShouldPerformFullBackup(DateTime? lastFullBackup, DateTime now)
    {
        var intervalDays = Math.Max(1, settings.ScheduledFullBackupDays);
        return settings.EnableScheduledFullBackups
            && lastFullBackup.HasValue
            && now.Subtract(lastFullBackup.Value) >= TimeSpan.FromDays(intervalDays);
    }

    public IReadOnlyList<VersionDetails> GetVersionsToDelete(
        IEnumerable<VersionDetails> versions,
        DateTime now)
    {
        return settings.RetentionMode switch
        {
            ScheduleRetentionMode.Automatic => GetAutomaticVersionsToDelete(versions, now),
            ScheduleRetentionMode.Interval => GetIntervalVersionsToDelete(versions, now),
            _ => Array.Empty<VersionDetails>(),
        };
    }

    public IReadOnlyList<VersionDetails> GetAutomaticVersionsToDelete(
        IEnumerable<VersionDetails> versions,
        DateTime now)
    {
        var versionList = versions.ToList();
        var hourlyBackupThreshold = TimeSpan.FromHours(Math.Max(1, settings.AutomaticHourlyBackupThreshold));
        var result = new List<VersionDetails>();

        foreach (var version in versionList)
        {
            if (version.Stable || now.Subtract(version.CreationDate) <= hourlyBackupThreshold)
            {
                continue;
            }

            if (now.Subtract(version.CreationDate) > TimeSpan.FromDays(
                DateTime.DaysInMonth(version.CreationDate.Year, version.CreationDate.Month)))
            {
                if (versionList.Any(x =>
                    version.CreationDate != x.CreationDate
                    && version.CreationDate.Year == x.CreationDate.Year
                    && version.CreationDate.Month == x.CreationDate.Month
                    && version.CreationDate.Day - (int)version.CreationDate.DayOfWeek == x.CreationDate.Day - (int)x.CreationDate.DayOfWeek
                    && version.CreationDate.DayOfWeek < x.CreationDate.DayOfWeek))
                {
                    result.Add(version);
                }
            }
            else if (versionList.Any(x =>
                version.CreationDate != x.CreationDate
                && version.CreationDate.Year == x.CreationDate.Year
                && version.CreationDate.Month == x.CreationDate.Month
                && version.CreationDate.Day == x.CreationDate.Day
                && version.CreationDate.TimeOfDay < x.CreationDate.TimeOfDay))
            {
                result.Add(version);
            }
        }

        return result;
    }

    private IReadOnlyList<VersionDetails> GetIntervalVersionsToDelete(
        IEnumerable<VersionDetails> versions,
        DateTime now)
    {
        var retentionInterval = Math.Max(1, settings.RetentionInterval);
        var retentionAge = settings.RetentionIntervalUnit switch
        {
            ScheduleRetentionIntervalUnit.Hour => TimeSpan.FromHours(retentionInterval),
            ScheduleRetentionIntervalUnit.Week => TimeSpan.FromDays(retentionInterval * 7d),
            _ => TimeSpan.FromDays(retentionInterval),
        };

        return versions
            .Where(version => !version.Stable && now.Subtract(version.CreationDate) > retentionAge)
            .ToList();
    }
}
