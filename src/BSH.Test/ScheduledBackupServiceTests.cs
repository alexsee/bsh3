// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Jobs;
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

        Assert.That(factory.Adapter.StopCalls, Is.GreaterThanOrEqualTo(1));
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
            new UnusedJobService(),
            new UnusedQueryManager(),
            scheduleRepository,
            factory,
            new ScheduleSettingsService(configurationManager, scheduleRepository));
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
        public void ScheduleAutoBackup(Action action) => ScheduleAutoBackupCalls++;
        public void ScheduleDaily(Action action, DateTime time) { }
        public void ScheduleHourly(Action action, DateTime time) { }
        public void ScheduleMonthly(Action action, DateTime time) { }
        public void ScheduleOnce(Action action, DateTime time) { }
        public void ScheduleWeekly(Action action, DateTime time) { }
        public void Start() => StartCalls++;
        public void Stop() => StopCalls++;
    }

    private sealed class EmptyScheduleRepository : IScheduleRepository
    {
        public Task<bool> HasScheduleEntriesAsync() => Task.FromResult(false);
        public Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync() => Task.FromResult<IReadOnlyList<ScheduleEntry>>([]);
        public Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> schedules) => Task.CompletedTask;
    }

    private sealed class UnusedJobService : IJobService
    {
        public bool IsCancellationRequested => false;
        public void Cancel() { }
        public Task<bool> CheckMediaAsync(ActionType action, bool silent = false) => Task.FromResult(true);
        public Task<bool> CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "") => Task.FromResult(true);
        public Task DeleteBackupAsync(string version, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true) => Task.CompletedTask;
        public Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true, IReadOnlyList<int>? versionIds = null) => Task.CompletedTask;
        public CancellationToken GetNewCancellationToken() => CancellationToken.None;
        public Task<bool> RequestPassword() => Task.FromResult(true);
        public Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true) => Task.CompletedTask;
        public Task ModifyBackupAsync(bool statusDialog = true) => Task.CompletedTask;
    }

    private sealed class UnusedQueryManager : IQueryManager
    {
        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public string GetFileNameFromDrive(FileTableRow file) => "";
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult(("", false));
        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(new FileDetails());
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult("");
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult("");
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult("");
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(0);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult<VersionDetails>(null);
        public List<VersionDetails> GetVersions(bool desc = true) => [];
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
    }
}
