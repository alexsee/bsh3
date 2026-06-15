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

    public bool ScheduledFullBackupsEnabled => settings.EnableScheduledFullBackups;

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
        var candidates = versionList
            .Where(version => !version.Stable && now.Subtract(version.CreationDate) > hourlyBackupThreshold)
            .ToList();

        var dailyCandidates = candidates
            .Where(version => !UsesWeeklyRetention(version, now));
        var weeklyCandidates = candidates
            .Where(version => UsesWeeklyRetention(version, now));

        return GetSupersededVersions(dailyCandidates, versionList, version => version.CreationDate.Date)
            .Concat(GetSupersededVersions(weeklyCandidates, versionList, version => GetWeekStart(version.CreationDate)))
            .ToList();
    }

    private static IEnumerable<VersionDetails> GetSupersededVersions(
        IEnumerable<VersionDetails> candidates,
        IEnumerable<VersionDetails> allVersions,
        Func<VersionDetails, DateTime> getRetentionPeriod)
    {
        var newestVersionByPeriod = allVersions
            .GroupBy(getRetentionPeriod)
            .ToDictionary(group => group.Key, group => group.Max(version => version.CreationDate));

        return candidates.Where(version =>
            version.CreationDate < newestVersionByPeriod[getRetentionPeriod(version)]);
    }

    private static bool UsesWeeklyRetention(VersionDetails version, DateTime now)
    {
        return now.Subtract(version.CreationDate) > TimeSpan.FromDays(
            DateTime.DaysInMonth(version.CreationDate.Year, version.CreationDate.Month));
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        return date.Date.AddDays(-(int)date.DayOfWeek);
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
