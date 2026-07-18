// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Service;
using NUnit.Framework;

namespace BSH.Test;

public class ScheduleSettingsServiceTests
{
    private const string DatabaseFileName = "testdb_schedule_settings.db";

    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IScheduleRepository scheduleRepository;
    private ScheduleSettingsService scheduleSettingsService;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists(DatabaseFileName))
        {
            File.Delete(DatabaseFileName);
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, DatabaseFileName));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        scheduleRepository = new ScheduleRepository(dbClientFactory);
        scheduleSettingsService = new ScheduleSettingsService(configurationManager, scheduleRepository);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists(DatabaseFileName))
        {
            File.Delete(DatabaseFileName);
        }
    }

    [Test]
    public async Task SaveAsyncPersistsCreatedAndDeletedScheduleEntries()
    {
        var settings = await scheduleSettingsService.LoadAsync();
        var once = settings.AddSchedule(ScheduleEntryKind.Once, new DateTime(2026, 6, 1, 8, 30, 0));
        settings.AddSchedule(ScheduleEntryKind.Hourly, new DateTime(2026, 6, 1, 0, 15, 0));
        settings.AddSchedule(ScheduleEntryKind.Daily, new DateTime(2026, 6, 1, 10, 0, 0));
        settings.AddSchedule(ScheduleEntryKind.Weekly, new DateTime(2026, 6, 3, 11, 0, 0));
        settings.AddSchedule(ScheduleEntryKind.Monthly, new DateTime(2026, 6, 12, 12, 0, 0));

        settings.DeleteSchedule(once);
        await scheduleSettingsService.SaveAsync(settings);

        var reloaded = await scheduleSettingsService.LoadAsync();

        Assert.That(reloaded.Entries, Has.Count.EqualTo(4));
        Assert.That(reloaded.Entries.Select(x => x.Type), Is.EquivalentTo(new[]
        {
            (int)ScheduleEntryKind.Hourly,
            (int)ScheduleEntryKind.Daily,
            (int)ScheduleEntryKind.Weekly,
            (int)ScheduleEntryKind.Monthly
        }));
        Assert.That(reloaded.Entries.Any(x => x.Type == (int)ScheduleEntryKind.Once), Is.False);
    }

    [Test]
    public async Task SaveAsyncPersistsRetentionFullBackupAndMissedBackupSettings()
    {
        var settings = await scheduleSettingsService.LoadAsync();
        settings.RetentionMode = ScheduleRetentionMode.Interval;
        settings.RetentionIntervalUnit = ScheduleRetentionIntervalUnit.Week;
        settings.RetentionInterval = 3;
        settings.AutomaticHourlyBackupThreshold = 36;
        settings.EnableScheduledFullBackups = true;
        settings.ScheduledFullBackupDays = 7;
        settings.PerformMissedBackupsLater = true;

        await scheduleSettingsService.SaveAsync(settings);

        Assert.That(configurationManager.IntervallDelete, Is.EqualTo("week|3"));
        Assert.That(configurationManager.IntervallAutoHourBackups, Is.EqualTo("36"));
        Assert.That(configurationManager.ScheduleFullBackup, Is.EqualTo("day|7"));
        Assert.That(configurationManager.DoPastBackups, Is.EqualTo("1"));

        var reloaded = await scheduleSettingsService.LoadAsync();

        Assert.That(reloaded.RetentionMode, Is.EqualTo(ScheduleRetentionMode.Interval));
        Assert.That(reloaded.RetentionIntervalUnit, Is.EqualTo(ScheduleRetentionIntervalUnit.Week));
        Assert.That(reloaded.RetentionInterval, Is.EqualTo(3));
        Assert.That(reloaded.AutomaticHourlyBackupThreshold, Is.EqualTo(36));
        Assert.That(reloaded.EnableScheduledFullBackups, Is.True);
        Assert.That(reloaded.ScheduledFullBackupDays, Is.EqualTo(7));
        Assert.That(reloaded.PerformMissedBackupsLater, Is.True);
    }

    [Test]
    public async Task LoadAsyncDisablesMalformedDestructivePolicySettings()
    {
        configurationManager.IntervallDelete = "week|invalid";
        configurationManager.IntervallAutoHourBackups = "invalid";
        configurationManager.ScheduleFullBackup = "day|invalid";

        var settings = await scheduleSettingsService.LoadAsync();

        Assert.That(settings.RetentionMode, Is.EqualTo(ScheduleRetentionMode.None));
        Assert.That(settings.AutomaticHourlyBackupThreshold, Is.EqualTo(24));
        Assert.That(settings.EnableScheduledFullBackups, Is.False);
    }

    [Test]
    public void BuildScheduleDateUsesKindSpecificDateParts()
    {
        var baseDate = new DateTimeOffset(new DateTime(2026, 6, 1, 8, 0, 0));
        var startTime = new TimeSpan(14, 30, 0);

        Assert.That(
            ScheduleSettings.BuildScheduleDate(ScheduleEntryKind.Once, baseDate, startTime, DayOfWeek.Friday, 31),
            Is.EqualTo(new DateTime(2026, 6, 1, 14, 30, 0)));
        Assert.That(
            ScheduleSettings.BuildScheduleDate(ScheduleEntryKind.Weekly, baseDate, startTime, DayOfWeek.Friday, 31),
            Is.EqualTo(new DateTime(2026, 6, 5, 14, 30, 0)));
        Assert.That(
            ScheduleSettings.BuildScheduleDate(ScheduleEntryKind.Monthly, baseDate, startTime, DayOfWeek.Friday, 31),
            Is.EqualTo(new DateTime(2026, 6, 30, 14, 30, 0)));
    }
}
