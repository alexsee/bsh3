// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test;

public class SchedulePolicyTests
{
    private static readonly DateTime Now = new(2026, 6, 15, 23, 0, 0);

    [Test]
    public void ShouldPerformFullBackupUsesTypedScheduleSettings()
    {
        var policy = new SchedulePolicy(new ScheduleSettings
        {
            EnableScheduledFullBackups = true,
            ScheduledFullBackupDays = 7
        });

        Assert.That(policy.ShouldPerformFullBackup(Now.AddDays(-7), Now), Is.True);
        Assert.That(policy.ShouldPerformFullBackup(Now.AddDays(-6), Now), Is.False);
        Assert.That(policy.ShouldPerformFullBackup(null, Now), Is.False);
    }

    [Test]
    public void ScheduledFullBackupsEnabledExposesWhetherHistoryIsRequired()
    {
        Assert.That(new SchedulePolicy(new ScheduleSettings()).ScheduledFullBackupsEnabled, Is.False);
        Assert.That(new SchedulePolicy(new ScheduleSettings
        {
            EnableScheduledFullBackups = true
        }).ScheduledFullBackupsEnabled, Is.True);
    }

    [Test]
    public void GetVersionsToDeleteUsesTypedIntervalRetention()
    {
        var oldVersion = CreateVersion("old", Now.AddDays(-15));
        var recentVersion = CreateVersion("recent", Now.AddDays(-13));
        var stableVersion = CreateVersion("stable", Now.AddDays(-30), stable: true);
        var policy = new SchedulePolicy(new ScheduleSettings
        {
            RetentionMode = ScheduleRetentionMode.Interval,
            RetentionIntervalUnit = ScheduleRetentionIntervalUnit.Week,
            RetentionInterval = 2
        });

        var result = policy.GetVersionsToDelete(new[] { oldVersion, recentVersion, stableVersion }, Now);

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { "old" }));
    }

    [Test]
    public void GetAutomaticVersionsToDeleteUsesTypedHourlyThreshold()
    {
        var supersededHourlyVersion = CreateVersion("old-hour", Now.AddHours(-37));
        var latestDailyVersion = CreateVersion("latest-day", Now.AddHours(-36));
        var policy = new SchedulePolicy(new ScheduleSettings
        {
            RetentionMode = ScheduleRetentionMode.Automatic,
            AutomaticHourlyBackupThreshold = 36
        });

        var result = policy.GetVersionsToDelete(
            new[] { supersededHourlyVersion, latestDailyVersion },
            Now);

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { "old-hour" }));
    }

    [Test]
    public void GetAutomaticVersionsToDeleteKeepsNewestVersionAcrossMonthBoundaryWeek()
    {
        var now = new DateTime(2026, 7, 15, 23, 0, 0);
        var olderVersion = CreateVersion("may", new DateTime(2026, 5, 31, 8, 0, 0));
        var newerVersion = CreateVersion("june", new DateTime(2026, 6, 1, 8, 0, 0));
        var policy = new SchedulePolicy(new ScheduleSettings
        {
            RetentionMode = ScheduleRetentionMode.Automatic,
            AutomaticHourlyBackupThreshold = 1
        });

        var result = policy.GetVersionsToDelete(new[] { olderVersion, newerVersion }, now);

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { "may" }));
    }

    [Test]
    public void GetAutomaticVersionsToDeleteKeepsStableVersionAsPeriodWinner()
    {
        var oldVersion = CreateVersion("old", Now.AddHours(-38));
        var stableVersion = CreateVersion("stable", Now.AddHours(-37), stable: true);
        var policy = new SchedulePolicy(new ScheduleSettings
        {
            RetentionMode = ScheduleRetentionMode.Automatic,
            AutomaticHourlyBackupThreshold = 24
        });

        var result = policy.GetVersionsToDelete(new[] { oldVersion, stableVersion }, Now);

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { "old" }));
    }

    private static VersionDetails CreateVersion(string id, DateTime creationDate, bool stable = false)
    {
        return new VersionDetails
        {
            Id = id,
            CreationDate = creationDate,
            Stable = stable
        };
    }
}
