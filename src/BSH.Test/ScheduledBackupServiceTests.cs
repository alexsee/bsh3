// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Services;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class ScheduledBackupServiceTests
{
    private static readonly DateTime NextAutoRun = new(2026, 8, 17, 8, 0, 0);

    [Test]
    public async Task ScheduledBackupRestoresPriorityWhenFullBackupLookupFails()
    {
        var scheduler = new FakeSchedulerAdapter();
        var configuration = new FakeConfigurationManager
        {
            TaskType = TaskType.Schedule,
            ScheduleFullBackup = "day|1"
        };
        var repository = new FakeScheduleRepository([new ScheduleEntry { Type = 2, Date = DateTime.Now }]);
        var jobs = new RecordingJobService();
        var service = new ScheduledBackupService(
            configuration, jobs, new ThrowingFullBackupQueryManager(), repository,
            new FakeSchedulerAdapterFactory(scheduler), new ScheduleSettingsService(configuration, repository),
            new FakeMediaWatcherFactory());
        using var process = Process.GetCurrentProcess();
        var originalPriority = process.PriorityClass;
        try
        {
            process.PriorityClass = ProcessPriorityClass.Normal;
            await service.StartAsync();

            var failure = Assert.ThrowsAsync<InvalidOperationException>(async () => await scheduler.HourlyBackup());

            Assert.That(failure.Message, Is.EqualTo("full backup lookup failed"));
            process.Refresh();
            Assert.That(process.PriorityClass, Is.EqualTo(ProcessPriorityClass.Normal));
            Assert.That(jobs.CreateBackupCalls, Is.Zero);
        }
        finally
        {
            service.Stop();
            process.PriorityClass = originalPriority;
        }
    }

    [Test]
    public async Task AutoLocalVolume_MatchingDriveArrival_StartsBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var jobService = new RecordingJobService { MediaAvailable = true };
        var service = CreateService(TaskType.Auto, MediaType.LocalDevice, jobService, factory);

        await service.StartAsync();
        Assert.That(factory.CreateCount, Is.EqualTo(1));

        factory.Watcher.Arrive("E:");
        await jobService.WaitForBackupAsync();

        Assert.That(jobService.CreateBackupCalls, Is.EqualTo(1));
        Assert.That(jobService.LastBackupTitle, Is.EqualTo("Automatisches Backup"));
    }

    [Test]
    public async Task AutoFtp_DoesNotStartMediaWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var service = CreateService(TaskType.Auto, MediaType.FileTransferServer, new RecordingJobService(), factory);

        await service.StartAsync();

        Assert.That(factory.CreateCount, Is.EqualTo(0));
        Assert.That(factory.Watcher.StartWatchingCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ScheduledLocalVolume_MissingMediaAtFire_StartsBackupWhenDriveArrives()
    {
        var factory = new FakeMediaWatcherFactory();
        var scheduler = new FakeSchedulerAdapter();
        var jobService = new RecordingJobService { MediaAvailable = false };
        var service = CreateService(
            TaskType.Schedule,
            MediaType.LocalDevice,
            jobService,
            factory,
            scheduler,
            schedules: [new ScheduleEntry { Type = 2, Date = DateTime.Now }]);

        await service.StartAsync();
        Assert.That(factory.CreateCount, Is.EqualTo(0));

        await scheduler.HourlyBackup();
        await WaitUntil(() => factory.Watcher.StartWatchingCount == 1);

        Assert.That(jobService.CreateBackupCalls, Is.EqualTo(1));
        Assert.That(factory.CreateCount, Is.EqualTo(1));

        jobService.MediaAvailable = true;
        factory.Watcher.Arrive("E:");
        await jobService.WaitForBackupAsync(minimumCalls: 2);

        Assert.That(jobService.CreateBackupCalls, Is.EqualTo(2));
        Assert.That(jobService.LastBackupTitle, Is.EqualTo("Automatische Sicherung"));
    }

    [Test]
    public async Task ScheduledFtp_MissingMediaAtFire_DoesNotStartMediaWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var scheduler = new FakeSchedulerAdapter();
        var jobService = new RecordingJobService { MediaAvailable = false };
        var service = CreateService(
            TaskType.Schedule,
            MediaType.FileTransferServer,
            jobService,
            factory,
            scheduler,
            schedules: [new ScheduleEntry { Type = 2, Date = DateTime.Now }]);

        await service.StartAsync();
        await scheduler.HourlyBackup();
        await jobService.WaitForBackupAsync();
        await Task.Delay(50);

        Assert.That(factory.CreateCount, Is.EqualTo(0));
        Assert.That(factory.Watcher.StartWatchingCount, Is.EqualTo(0));
    }

    [Test]
    public async Task StartAsyncDoesNotScheduleWhenTaskTypeIsManual()
    {
        var factory = new RecordingSchedulerAdapterFactory();
        var service = CreateService(TaskType.Manual, factory);

        await service.StartAsync();

        Assert.That(factory.CreateCalls, Is.EqualTo(0));
        Assert.That(service.GetNextBackupDate(), Is.EqualTo(DateTime.MaxValue));
    }

    [Test]
    public async Task StartAsyncSchedulesAutoBackupWhenTaskTypeIsAuto()
    {
        var factory = new RecordingSchedulerAdapterFactory();
        var service = CreateService(TaskType.Auto, factory);

        await service.StartAsync();

        Assert.That(factory.CreateCalls, Is.EqualTo(1));
        Assert.That(factory.Adapter.StartCalls, Is.EqualTo(1));
        Assert.That(factory.Adapter.ScheduleAutoBackupCalls, Is.EqualTo(1));
        Assert.That(service.GetNextBackupDate(), Is.EqualTo(NextAutoRun));
    }

    [Test]
    public async Task StartAsyncStartsSchedulerWhenTaskTypeIsSchedule()
    {
        var factory = new RecordingSchedulerAdapterFactory();
        var service = CreateService(TaskType.Schedule, factory);

        await service.StartAsync();

        Assert.That(factory.CreateCalls, Is.EqualTo(1));
        Assert.That(factory.Adapter.StartCalls, Is.EqualTo(1));
        Assert.That(factory.Adapter.ScheduleAutoBackupCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task RefreshStopsAutoAndDoesNotRestartWhenTaskTypeChangesToManual()
    {
        var configurationManager = new FakeConfigurationManager { TaskType = TaskType.Auto };
        var factory = new RecordingSchedulerAdapterFactory();
        var service = CreateService(configurationManager, factory);

        await service.StartAsync();
        configurationManager.TaskType = TaskType.Manual;
        service.Stop();
        await service.StartAsync();

        Assert.That(factory.Adapter.StopCalls, Is.EqualTo(1));
        Assert.That(factory.CreateCalls, Is.EqualTo(1));
        Assert.That(service.GetNextBackupDate(), Is.EqualTo(DateTime.MaxValue));
    }

    [Test]
    public async Task RefreshStartsAutoWhenTaskTypeChangesFromManual()
    {
        var configurationManager = new FakeConfigurationManager { TaskType = TaskType.Manual };
        var factory = new RecordingSchedulerAdapterFactory();
        var service = CreateService(configurationManager, factory);

        await service.StartAsync();
        configurationManager.TaskType = TaskType.Auto;
        service.Stop();
        await service.StartAsync();

        Assert.That(factory.CreateCalls, Is.EqualTo(1));
        Assert.That(factory.Adapter.ScheduleAutoBackupCalls, Is.EqualTo(1));
        Assert.That(service.GetNextBackupDate(), Is.EqualTo(NextAutoRun));
    }

    [Test]
    public async Task ScheduledDailySkipsCatchUpWhenLastBackupIsNewer()
    {
        var scheduler = new FakeSchedulerAdapter();
        var configuration = new FakeConfigurationManager
        {
            TaskType = TaskType.Schedule,
            MediumType = MediaType.LocalDevice,
            BackupFolder = @"E:\Backups",
            DoPastBackups = "1",
            LastBackupDone = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss")
        };
        var schedules = new[] { new ScheduleEntry { Type = 3, Date = DateTime.Today } };
        var scheduleRepository = new FakeScheduleRepository(schedules);
        var service = new ScheduledBackupService(
            configuration,
            new RecordingJobService { MediaAvailable = true },
            new StubQueryManager(),
            scheduleRepository,
            new FakeSchedulerAdapterFactory(scheduler),
            new ScheduleSettingsService(configuration, scheduleRepository),
            new FakeMediaWatcherFactory());

        await service.StartAsync();

        Assert.That(scheduler.ScheduleOnceCalls, Is.EqualTo(0));
        Assert.That(scheduler.ScheduleDailyCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task StopEndsMediaWatchAndAllowsRestart()
    {
        var mediaFactory = new FakeMediaWatcherFactory();
        var scheduler = new RecordingSchedulerAdapterFactory();
        var jobs = new RecordingJobService { MediaAvailable = true };
        var configuration = new FakeConfigurationManager
        {
            TaskType = TaskType.Auto,
            MediumType = MediaType.LocalDevice,
            BackupFolder = @"E:\Backups"
        };
        var repository = new EmptyScheduleRepository();
        var service = new ScheduledBackupService(
            configuration, jobs, new StubQueryManager(), repository, scheduler,
            new ScheduleSettingsService(configuration, repository), mediaFactory);

        await service.StartAsync();
        service.Stop();
        service.Stop();
        mediaFactory.Watcher.Arrive("E:");

        Assert.That(scheduler.Adapter.StopCalls, Is.EqualTo(1));
        Assert.That(service.GetNextBackupDate(), Is.EqualTo(DateTime.MaxValue));
        Assert.That(mediaFactory.Watcher.StopWatchingCount, Is.EqualTo(1));
        Assert.That(jobs.CreateBackupCalls, Is.EqualTo(0));

        await service.StartAsync();
        Assert.That(scheduler.CreateCalls, Is.EqualTo(2));
        Assert.That(mediaFactory.Watcher.StartWatchingCount, Is.EqualTo(2));
        service.Stop();

        Assert.That(scheduler.Adapter.StopCalls, Is.EqualTo(2));
        Assert.That(mediaFactory.Watcher.StopWatchingCount, Is.EqualTo(2));
    }

    [TestCase(-1, 1)]
    [TestCase(0, 0)]
    [TestCase(1, 0)]
    public async Task ScheduledDailyCatchUpDependsOnExistingVersionDate(int minutesFromSchedule, int expectedCatchUps)
    {
        var scheduler = new FakeSchedulerAdapter();
        var configuration = new FakeConfigurationManager
        {
            TaskType = TaskType.Schedule,
            MediumType = MediaType.FileTransferServer,
            DoPastBackups = "1"
        };
        var date = DateTime.Today;
        var repository = new FakeScheduleRepository([new ScheduleEntry { Type = 3, Date = date }]);
        var query = new StubQueryManager
        {
            Versions = [new VersionDetails { CreationDate = date.AddMinutes(minutesFromSchedule) }]
        };
        var service = new ScheduledBackupService(
            configuration, new RecordingJobService(), query, repository,
            new FakeSchedulerAdapterFactory(scheduler), new ScheduleSettingsService(configuration, repository),
            new FakeMediaWatcherFactory());

        await service.StartAsync();

        Assert.That(scheduler.ScheduleOnceCalls, Is.EqualTo(expectedCatchUps));
        Assert.That(scheduler.ScheduleDailyCalls, Is.EqualTo(1));
        service.Stop();
    }

    [TestCase(3, -1, true)]
    [TestCase(3, 0, false)]
    [TestCase(3, 1, false)]
    [TestCase(4, -1, true)]
    [TestCase(4, 0, false)]
    [TestCase(5, -1, true)]
    [TestCase(5, 0, false)]
    public void SharedCatchUpOnScheduledDayUsesInclusiveHistoryBoundary(int type, int secondsFromSlot, bool expected)
    {
        var date = new DateTime(2026, 11, 18, 8, 0, 0);
        AssertRegistration(type, date, date.AddHours(4), [date.AddSeconds(secondsFromSlot)], expected);
    }

    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void SharedCatchUpOnScheduledDayRequiresTimeToHavePassed(int type)
    {
        var date = new DateTime(2026, 11, 18, 8, 0, 0);
        AssertRegistration(type, date, date.AddSeconds(-1), [], false);
        AssertRegistration(type, date, date, [], false);
        AssertRegistration(type, date, date.AddSeconds(1), [], true);
        AssertRegistration(type, date, date.AddSeconds(1), [], false, enabled: "0");
        AssertRegistration(type, date, date.AddSeconds(1), [], false, lastBackup: "18-11-2026 08-00-00");
        AssertRegistration(type, date, date.AddSeconds(1), [], true, lastBackup: "18-11-2026 07-59-59");
    }

    [TestCase(4, 16)]
    [TestCase(5, 5)]
    [TestCase(5, 31)]
    public void SharedCatchUpBetweenSlotsPreservesOrderedHistoryAndWholeDayChecks(int type, int day)
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0);
        var date = new DateTime(2026, day == 31 ? 10 : 11, day, 8, 0, 0);
        AssertRegistration(type, date, now, [], false);
        AssertRegistration(type, date, now, [date.AddDays(-2)], true);
        AssertRegistration(type, date, now, [date, date.AddDays(-2)], false);
        AssertRegistration(type, date, now, [now, date.AddDays(-2)], false);
        AssertRegistration(type, date, now, [date.AddDays(-2)], false, enabled: "0");
        AssertRegistration(type, date, now, [date.AddDays(-2)], false, lastBackup: "18-11-2026 08-00-00");
        AssertRegistration(type, date, now, [date.Date.AddDays(-1).AddHours(13)], false);
        AssertRegistration(type, date, now, [date.Date.AddDays(-1).AddHours(12)], true);
    }

    private static void AssertRegistration(int type, DateTime date, DateTime now, DateTime[] versions,
        bool expectedCatchUp, string enabled = "1", string lastBackup = "")
    {
        var configuration = new FakeConfigurationManager { DoPastBackups = enabled, LastBackupDone = lastBackup };
        var query = new StubQueryManager
        {
            Versions = new List<VersionDetails>(Array.ConvertAll(versions, value => new VersionDetails { CreationDate = value }))
        };
        var scheduler = new CatchUpScheduler();
        var watchCalls = 0;
        Func<Task> backup = () => Task.CompletedTask;
        new ScheduledBackupRegistration(configuration, query).Register(
            new ScheduleEntry { Type = type, Date = date }, scheduler, backup, () => watchCalls++, now);

        Assert.That(watchCalls, Is.EqualTo(expectedCatchUp ? 1 : 0));
        Assert.That(scheduler.Registrations, Is.EqualTo(expectedCatchUp
            ? new[] { (1, now.AddMinutes(1)), (type, date) }
            : new[] { (type, date) }));
        Assert.That(ReferenceEquals(scheduler.Callback, backup), Is.True);
    }

    [TestCase(1, -1, 0)]
    [TestCase(1, 0, 0)]
    [TestCase(1, 1, 1)]
    [TestCase(2, -1, 1)]
    [TestCase(99, 1, 0)]
    public void SharedRegistrationPreservesOneShotAndHourlyDispatch(int type, int secondsFromNow, int expected)
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0);
        var date = now.AddSeconds(secondsFromNow);
        var registration = new ScheduledBackupRegistration(new FakeConfigurationManager { DoPastBackups = "1" }, new ThrowingVersionsQueryManager());
        var scheduler = new CatchUpScheduler();
        registration.Register(new ScheduleEntry { Type = type, Date = date }, scheduler,
            () => Task.CompletedTask, () => Assert.Fail("Unexpected media watch"), now);
        Assert.That(scheduler.Registrations.Count, Is.EqualTo(expected));
        if (expected > 0)
        {
            Assert.That(scheduler.Registrations[0], Is.EqualTo((type, date)));
        }
    }

    private sealed class CatchUpScheduler : ISchedulerAdapter
    {
        public List<(int Type, DateTime Date)> Registrations { get; } = [];
        public Func<Task> Callback { get; private set; }
        private void Record(int type, Func<Task> action, DateTime time)
        {
            Registrations.Add((type, time));
            Callback = action;
        }
        public void ScheduleOnce(Func<Task> action, DateTime time) => Record(1, action, time);
        public void ScheduleHourly(Func<Task> action, DateTime time) => Record(2, action, time);
        public void ScheduleDaily(Func<Task> action, DateTime time) => Record(3, action, time);
        public void ScheduleWeekly(Func<Task> action, DateTime time) => Record(4, action, time);
        public void ScheduleMonthly(Func<Task> action, DateTime time) => Record(5, action, time);
        public void ScheduleAutoBackup(Func<Task> action) => throw new NotSupportedException();
        public DateTime GetNextRun() => DateTime.MaxValue;
        public void Start() { }
        public void Stop() { }
    }

    private static ScheduledBackupService CreateService(
        TaskType taskType,
        MediaType mediaType,
        RecordingJobService jobService,
        FakeMediaWatcherFactory factory,
        FakeSchedulerAdapter scheduler = null,
        IReadOnlyList<ScheduleEntry> schedules = null)
    {
        var configuration = new FakeConfigurationManager
        {
            TaskType = taskType,
            MediumType = mediaType,
            BackupFolder = @"E:\Backups",
            DoPastBackups = "0"
        };
        var scheduleRepository = new FakeScheduleRepository(schedules ?? Array.Empty<ScheduleEntry>());
        return new ScheduledBackupService(
            configuration,
            jobService,
            new StubQueryManager(),
            scheduleRepository,
            new FakeSchedulerAdapterFactory(scheduler ?? new FakeSchedulerAdapter()),
            new ScheduleSettingsService(configuration, scheduleRepository),
            factory);
    }

    private static ScheduledBackupService CreateService(TaskType taskType, RecordingSchedulerAdapterFactory factory)
    {
        return CreateService(new FakeConfigurationManager { TaskType = taskType }, factory);
    }

    private static ScheduledBackupService CreateService(
        FakeConfigurationManager configurationManager,
        RecordingSchedulerAdapterFactory factory)
    {
        var scheduleRepository = new EmptyScheduleRepository();
        return new ScheduledBackupService(
            configurationManager,
            new FakeJobService(),
            new StubQueryManager(),
            scheduleRepository,
            factory,
            new ScheduleSettingsService(configurationManager, scheduleRepository),
            new FakeMediaWatcherFactory());
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.Fail("Timed out waiting for condition.");
    }

    private sealed class FakeMediaWatcherFactory : IMediaWatcherFactory
    {
        public FakeMediaWatcher Watcher { get; } = new();
        public int CreateCount { get; private set; }

        public IMediaWatcher Create()
        {
            CreateCount++;
            return Watcher;
        }
    }

    private sealed class FakeMediaWatcher : IMediaWatcher
    {
        public event EventHandler<string> DeviceAdded;
        public int StartWatchingCount { get; private set; }
        public int StopWatchingCount { get; private set; }

        public void StartWatching() => StartWatchingCount++;

        public void StopWatching() => StopWatchingCount++;

        public void Arrive(string driveLetter) => DeviceAdded?.Invoke(this, driveLetter);
    }

    private sealed class FakeSchedulerAdapterFactory : ISchedulerAdapterFactory
    {
        private readonly ISchedulerAdapter adapter;

        public FakeSchedulerAdapterFactory(ISchedulerAdapter adapter)
        {
            this.adapter = adapter;
        }

        public ISchedulerAdapter Create() => adapter;
    }

    private sealed class FakeSchedulerAdapter : ISchedulerAdapter
    {
        public Func<Task> AutoBackup { get; private set; }
        public Func<Task> HourlyBackup { get; private set; }

        public int ScheduleOnceCalls { get; private set; }
        public int ScheduleDailyCalls { get; private set; }

        public DateTime GetNextRun() => DateTime.MaxValue;
        public void ScheduleAutoBackup(Func<Task> action) => AutoBackup = action;
        public void ScheduleDaily(Func<Task> action, DateTime time) => ScheduleDailyCalls++;
        public void ScheduleHourly(Func<Task> action, DateTime time) => HourlyBackup = action;
        public void ScheduleMonthly(Func<Task> action, DateTime time) { }
        public void ScheduleOnce(Func<Task> action, DateTime time) => ScheduleOnceCalls++;
        public void ScheduleWeekly(Func<Task> action, DateTime time) { }
        public void Start() { }
        public void Stop() { }
    }

    private sealed class RecordingSchedulerAdapterFactory : ISchedulerAdapterFactory
    {
        public int CreateCalls { get; private set; }
        public RecordingSchedulerAdapter Adapter { get; } = new();

        public ISchedulerAdapter Create()
        {
            CreateCalls++;
            return Adapter;
        }
    }

    private sealed class RecordingSchedulerAdapter : ISchedulerAdapter
    {
        public int StartCalls { get; private set; }
        public int StopCalls { get; private set; }
        public int ScheduleAutoBackupCalls { get; private set; }

        public DateTime GetNextRun() => NextAutoRun;
        public void ScheduleAutoBackup(Func<Task> action) => ScheduleAutoBackupCalls++;
        public void ScheduleDaily(Func<Task> action, DateTime time) { }
        public void ScheduleHourly(Func<Task> action, DateTime time) { }
        public void ScheduleMonthly(Func<Task> action, DateTime time) { }
        public void ScheduleOnce(Func<Task> action, DateTime time) { }
        public void ScheduleWeekly(Func<Task> action, DateTime time) { }
        public void Start() => StartCalls++;
        public void Stop() => StopCalls++;
    }

    private sealed class FakeScheduleRepository : IScheduleRepository
    {
        private readonly IReadOnlyList<ScheduleEntry> schedules;

        public FakeScheduleRepository(IReadOnlyList<ScheduleEntry> schedules)
        {
            this.schedules = schedules;
        }

        public Task<bool> HasScheduleEntriesAsync() => Task.FromResult(schedules.Count > 0);
        public Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync() => Task.FromResult(schedules);
        public Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> _) => Task.CompletedTask;
    }

    private sealed class EmptyScheduleRepository : IScheduleRepository
    {
        public Task<bool> HasScheduleEntriesAsync() => Task.FromResult(false);
        public Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync() => Task.FromResult<IReadOnlyList<ScheduleEntry>>([]);
        public Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> schedules) => Task.CompletedTask;
    }

    private sealed class RecordingJobService : FakeJobService
    {
        public bool MediaAvailable { get; set; } = true;
        public int CreateBackupCalls { get; private set; }
        public string LastBackupTitle { get; private set; }

        public override Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(MediaAvailable);

        public override Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, string sourceFolders = "")
        {
            CreateBackupCalls++;
            LastBackupTitle = title;
            return Task.FromResult(MediaAvailable);
        }

        public Task WaitForBackupAsync(int minimumCalls = 1)
        {
            return WaitUntil(() => CreateBackupCalls >= minimumCalls);
        }
    }

    private class StubQueryManager : FakeQueryManager
    {
        public List<VersionDetails> Versions { get; init; } = [];

        public override Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public override Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public override string GetFileNameFromDrive(FileTableRow file) => "";
        public override Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public override Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(new FileDetails());
        public override Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public override Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public override Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public override List<VersionDetails> GetVersions(bool desc = true) => Versions;
    }

    [Test]
    public async Task RemoveOldBackupsSurvivesVersionLookupFailure()
    {
        var configuration = new FakeConfigurationManager { TaskType = TaskType.Manual };
        var scheduleRepository = new FakeScheduleRepository(Array.Empty<ScheduleEntry>());
        var service = new ScheduledBackupService(
            configuration,
            new FakeJobService(),
            new ThrowingVersionsQueryManager(),
            scheduleRepository,
            new FakeSchedulerAdapterFactory(new FakeSchedulerAdapter()),
            new ScheduleSettingsService(configuration, scheduleRepository),
            new FakeMediaWatcherFactory());

        var removeOldBackups = typeof(ScheduledBackupService).GetMethod(
            "RemoveOldBackups",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.That(removeOldBackups, Is.Not.Null);

        // the failure is logged and swallowed so the backup itself does not fail
        Assert.DoesNotThrowAsync(async () => await (Task)removeOldBackups.Invoke(service, null));
    }

    private sealed class ThrowingVersionsQueryManager : StubQueryManager
    {
        public override List<VersionDetails> GetVersions(bool desc = true) => throw new InvalidOperationException("version lookup failed");
    }

    private sealed class ThrowingFullBackupQueryManager : StubQueryManager
    {
        public override async Task<VersionDetails> GetLastFullBackupAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("full backup lookup failed");
        }
    }
}
