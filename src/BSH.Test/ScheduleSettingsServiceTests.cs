// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Services;
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
        scheduleSettingsService = new ScheduleSettingsService(configurationManager, scheduleRepository, dbClientFactory);
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

        var persistedConfiguration = new ConfigurationManager(dbClientFactory);
        await persistedConfiguration.InitializeAsync();
        Assert.That(persistedConfiguration.IntervallDelete, Is.EqualTo("week|3"));
        Assert.That(persistedConfiguration.IntervallAutoHourBackups, Is.EqualTo("36"));
        Assert.That(persistedConfiguration.ScheduleFullBackup, Is.EqualTo("day|7"));
        Assert.That(persistedConfiguration.DoPastBackups, Is.EqualTo("1"));

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
    public async Task SaveAsyncRollsBackScheduleChangesWhenConfigPersistFails()
    {
        var baseline = await scheduleSettingsService.LoadAsync();
        baseline.AddSchedule(ScheduleEntryKind.Daily, new DateTime(2026, 6, 1, 9, 0, 0));
        await scheduleSettingsService.SaveAsync(baseline);

        configurationManager.IntervallDelete = string.Empty;
        configurationManager.IntervallAutoHourBackups = "24";
        configurationManager.ScheduleFullBackup = string.Empty;
        configurationManager.DoPastBackups = "0";

        var failingConfiguration = new FailingPersistConfigurationManager(configurationManager);
        var service = new ScheduleSettingsService(failingConfiguration, scheduleRepository, dbClientFactory);

        var settings = await scheduleSettingsService.LoadAsync();
        settings.Entries.Clear();
        settings.AddSchedule(ScheduleEntryKind.Weekly, new DateTime(2026, 6, 8, 15, 0, 0));
        settings.RetentionMode = ScheduleRetentionMode.Automatic;
        settings.AutomaticHourlyBackupThreshold = 48;
        settings.EnableScheduledFullBackups = true;
        settings.ScheduledFullBackupDays = 5;
        settings.PerformMissedBackupsLater = true;

        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SaveAsync(settings));

        var reloaded = await scheduleSettingsService.LoadAsync();

        Assert.That(reloaded.Entries, Has.Count.EqualTo(1));
        Assert.That(reloaded.Entries[0].Type, Is.EqualTo((int)ScheduleEntryKind.Daily));
        Assert.That(configurationManager.IntervallDelete, Is.EqualTo(string.Empty));
        Assert.That(configurationManager.IntervallAutoHourBackups, Is.EqualTo("24"));
        Assert.That(configurationManager.ScheduleFullBackup, Is.EqualTo(string.Empty));
        Assert.That(configurationManager.DoPastBackups, Is.EqualTo("0"));
        Assert.That(failingConfiguration.ApplyLocalPropertiesCalled, Is.False);
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

    private sealed class FailingPersistConfigurationManager : IConfigurationManager
    {
        private readonly IConfigurationManager inner;

        public FailingPersistConfigurationManager(IConfigurationManager inner)
        {
            this.inner = inner;
        }

        public bool ApplyLocalPropertiesCalled { get; private set; }

        public string AutoBackup { get => inner.AutoBackup; set => inner.AutoBackup = value; }
        public string BackupFolder { get => inner.BackupFolder; set => inner.BackupFolder = value; }
        public string BackupSize { get => inner.BackupSize; set => inner.BackupSize = value; }
        public int Compression { get => inner.Compression; set => inner.Compression = value; }
        public string DbStatus { get => inner.DbStatus; set => inner.DbStatus = value; }
        public string DBVersion { get => inner.DBVersion; set => inner.DBVersion = value; }
        public string DeativateAutoBackupsWhenAkku { get => inner.DeativateAutoBackupsWhenAkku; set => inner.DeativateAutoBackupsWhenAkku = value; }
        public string DoPastBackups { get => inner.DoPastBackups; set => inner.DoPastBackups = value; }
        public int Encrypt { get => inner.Encrypt; set => inner.Encrypt = value; }
        public string EncryptPassMD5 { get => inner.EncryptPassMD5; set => inner.EncryptPassMD5 = value; }
        public string ExcludeCompression { get => inner.ExcludeCompression; set => inner.ExcludeCompression = value; }
        public string ExcludeFile { get => inner.ExcludeFile; set => inner.ExcludeFile = value; }
        public string ExcludeFileBigger { get => inner.ExcludeFileBigger; set => inner.ExcludeFileBigger = value; }
        public string ExcludeFileTypes { get => inner.ExcludeFileTypes; set => inner.ExcludeFileTypes = value; }
        public string ExcludeFolder { get => inner.ExcludeFolder; set => inner.ExcludeFolder = value; }
        public string ExcludeMask { get => inner.ExcludeMask; set => inner.ExcludeMask = value; }
        public string IncludeSystemFolders { get => inner.IncludeSystemFolders; set => inner.IncludeSystemFolders = value; }
        public string FreeSpace { get => inner.FreeSpace; set => inner.FreeSpace = value; }
        public string FtpCoding { get => inner.FtpCoding; set => inner.FtpCoding = value; }
        public string FtpEncryptionMode { get => inner.FtpEncryptionMode; set => inner.FtpEncryptionMode = value; }
        public string FtpFolder { get => inner.FtpFolder; set => inner.FtpFolder = value; }
        public string FtpHost { get => inner.FtpHost; set => inner.FtpHost = value; }
        public string FtpPass { get => inner.FtpPass; set => inner.FtpPass = value; }
        public string FtpPort { get => inner.FtpPort; set => inner.FtpPort = value; }
        public string FtpSslProtocols { get => inner.FtpSslProtocols; set => inner.FtpSslProtocols = value; }
        public string FtpUser { get => inner.FtpUser; set => inner.FtpUser = value; }
        public string InfoBackupDone { get => inner.InfoBackupDone; set => inner.InfoBackupDone = value; }
        public string IntervallAutoHourBackups { get => inner.IntervallAutoHourBackups; set => inner.IntervallAutoHourBackups = value; }
        public string IntervallDelete { get => inner.IntervallDelete; set => inner.IntervallDelete = value; }
        public string IsConfigured { get => inner.IsConfigured; set => inner.IsConfigured = value; }
        public string LastBackupDone { get => inner.LastBackupDone; set => inner.LastBackupDone = value; }
        public string LastVersionDate { get => inner.LastVersionDate; set => inner.LastVersionDate = value; }
        public string MediaVolumeSerial { get => inner.MediaVolumeSerial; set => inner.MediaVolumeSerial = value; }
        public string Medium { get => inner.Medium; set => inner.Medium = value; }
        public MediaType MediumType { get => inner.MediumType; set => inner.MediumType = value; }
        public string OldBackupPrevent { get => inner.OldBackupPrevent; set => inner.OldBackupPrevent = value; }
        public string RemindAfterDays { get => inner.RemindAfterDays; set => inner.RemindAfterDays = value; }
        public string RemindSpace { get => inner.RemindSpace; set => inner.RemindSpace = value; }
        public string ScheduleFullBackup { get => inner.ScheduleFullBackup; set => inner.ScheduleFullBackup = value; }
        public string ShowLocalizedPath { get => inner.ShowLocalizedPath; set => inner.ShowLocalizedPath = value; }
        public string ShowWaitOnMediaAutoBackups { get => inner.ShowWaitOnMediaAutoBackups; set => inner.ShowWaitOnMediaAutoBackups = value; }
        public string SourceFolder { get => inner.SourceFolder; set => inner.SourceFolder = value; }
        public TaskType TaskType { get => inner.TaskType; set => inner.TaskType = value; }
        public string UNCPassword { get => inner.UNCPassword; set => inner.UNCPassword = value; }
        public string UNCUsername { get => inner.UNCUsername; set => inner.UNCUsername = value; }

        public Task InitializeAsync() => inner.InitializeAsync();

        public void PersistProperties(DbClient dbClient, IReadOnlyList<(string Property, string Value)> properties)
        {
            throw new InvalidOperationException("Simulated configuration persist failure.");
        }

        public void ApplyLocalProperties(IReadOnlyList<(string Property, string Value)> properties)
        {
            ApplyLocalPropertiesCalled = true;
            inner.ApplyLocalProperties(properties);
        }
    }
}
