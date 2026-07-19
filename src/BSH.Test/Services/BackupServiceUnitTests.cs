// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Providers.Storage;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Service;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Types.Exceptions;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test.Services;

public class BackupServiceUnitTests
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;
    private IBackupMutationRepository backupMutationRepository;
    private BackupService backupService;

    [SetUp]
    public async Task SetUp()
    {
        if (File.Exists("testdb_backupservice.db"))
        {
            DbClientFactory.ClosePool();
            File.Delete("testdb_backupservice.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_backupservice.db"));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.Encrypt = 0;

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
        versionQueryRepository = new VersionQueryRepository();
        backupMutationRepository = new BackupMutationRepository(dbClientFactory);

        backupService = new BackupService(
            configurationManager,
            queryManager,
            dbClientFactory,
            new StorageFactoryMock(),
            new VssClientMock(),
            new FileCollectorServiceFactoryMock(new List<FolderTableRow>(), new List<FileTableRow>()),
            versionQueryRepository,
            backupMutationRepository);
    }

    [TearDown]
    public void TearDown()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_backupservice.db"))
        {
            File.Delete("testdb_backupservice.db");
        }
    }

    [Test]
    public void PasswordHelpers_RoundTrip()
    {
        Assert.That(backupService.HasPassword(), Is.False);
        Assert.That(backupService.GetPassword(), Is.Null);

        backupService.SetPassword("secret");
        Assert.That(backupService.HasPassword(), Is.True);
        Assert.That(backupService.GetPassword(), Is.EqualTo("secret"));

        backupService.SetPassword("");
        Assert.That(backupService.HasPassword(), Is.False);
    }

    [Test]
    public void Constructor_Throws_WhenDependenciesAreNull()
    {
        Assert.Throws<ArgumentNullException>(() => new BackupService(
            null,
            queryManager,
            dbClientFactory,
            new StorageFactoryMock(),
            new VssClientMock(),
            new FileCollectorServiceFactoryMock(new List<FolderTableRow>(), new List<FileTableRow>()),
            versionQueryRepository,
            backupMutationRepository));
    }

    [Test]
    public void StartBackup_Throws_WhenPasswordRequiredAndMissing()
    {
        configurationManager.Encrypt = 1;
        Assert.ThrowsAsync<PasswordRequiredException>(async () =>
            await backupService.StartBackup("t", "d", new NoopJobReport(), default));
    }

    [Test]
    public void UpdateVersionAsync_Throws_ForInvalidVersionId()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await backupService.UpdateVersionAsync("not-a-number", new VersionDetails { Title = "t", Description = "d" }));
    }

    [Test]
    public void UpdateVersionAsync_Throws_ForNullDetails()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await backupService.UpdateVersionAsync("1", null));
    }

    private sealed class NoopJobReport : IJobReport
    {
        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) { }
        public void ReportStatus(string title, string text) { }
        public void ReportProgress(int total, int current) { }
        public void ReportFileProgress(string file) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }
        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) =>
            Task.FromResult(RequestOverwriteResult.Overwrite);
        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;
    }
}

public class ForwardJobReportTests
{
    [Test]
    public async Task ForwardJobReport_ForwardsStatusOverwriteAndExceptions()
    {
        var inner = new RecordingJobReport();
        var forward = new ForwardJobReport(inner);

        forward.ReportAction(ActionType.Backup, silent: true);
        forward.ReportState(JobState.RUNNING);
        forward.ReportProgress(10, 1);
        forward.ReportStatus("title", "text");
        forward.ReportFileProgress("file.txt");
        forward.ReportExceptions(new Collection<FileExceptionEntry>
        {
            new() { File = new FileTableRow { FileName = "a.txt" }, Exception = new Exception("x") }
        }, silent: false);

        Assert.That(forward.HasExceptions, Is.True);
        Assert.That(inner.StatusCalls, Is.EqualTo(1));
        Assert.That(inner.FileProgressCalls, Is.EqualTo(1));

        var overwrite = await forward.RequestOverwrite(
            new FileTableRow { FileName = "local" },
            new FileTableRow { FileName = "remote" });
        Assert.That(overwrite, Is.EqualTo(RequestOverwriteResult.Overwrite));

        await forward.RequestShowErrorInsufficientDiskSpaceAsync();
        Assert.That(inner.DiskSpaceCalls, Is.EqualTo(1));

        forward.ForwardExceptions(silent: true);
        Assert.That(inner.ExceptionBatches, Is.EqualTo(1));
    }

    private sealed class RecordingJobReport : IJobReport
    {
        public int StatusCalls { get; private set; }
        public int FileProgressCalls { get; private set; }
        public int DiskSpaceCalls { get; private set; }
        public int ExceptionBatches { get; private set; }

        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) { }
        public void ReportStatus(string title, string text) => StatusCalls++;
        public void ReportProgress(int total, int current) { }
        public void ReportFileProgress(string file) => FileProgressCalls++;
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) => ExceptionBatches++;
        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile) =>
            Task.FromResult(RequestOverwriteResult.Overwrite);
        public Task RequestShowErrorInsufficientDiskSpaceAsync()
        {
            DiskSpaceCalls++;
            return Task.CompletedTask;
        }
    }
}
