// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types.Exceptions;
using Brightbits.BSH.Engine.Service.Jobs;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Providers.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Unit tests for <see cref="RestoreJob"/> using <see cref="StorageMock"/>.
/// Content round-trips against real storage live in Integration tests.
/// </summary>
public class RestoreTests
{
    private const string TestDbName = "testdb_restore.db";

    /// <summary>
    /// Synthetic destination used by most tests. RestoreJob may attempt
    /// Directory.CreateDirectory / timestamp updates; those failures are ignored.
    /// No files are written by StorageMock.
    /// </summary>
    private const string SyntheticDestination = @"Z:\BSH.Test.Restore";

    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            dbClientFactory = null;
        }

        if (File.Exists(TestDbName))
        {
            File.Delete(TestDbName);
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, TestDbName));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        versionQueryRepository = new VersionQueryRepository();

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();

        if (File.Exists(TestDbName))
        {
            File.Delete(TestDbName);
        }
    }

    [Test]
    public void TestFailMedium()
    {
        var storage = new StorageMock(failCheckMedium: true);
        var restoreJob = CreateRestoreJob(storage);

        Assert.ThrowsAsync<DeviceNotReadyException>(async () => await restoreJob.RestoreAsync(CancellationToken.None));
    }

    [Test]
    public async Task TestRestoreRoutesByFileType()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");
        await SeedFileForVersionAsync(2, 2, 1, "compressed.txt", @"\docs\", 2, "");
        await SeedFileForVersionAsync(3, 3, 1, "encrypted-long.txt", @"\docs\", 6, "very-long-file-name");

        var storage = new StorageMock();
        var restoreJob = CreateRestoreJob(storage, file: @"\");
        restoreJob.Password = "test123";

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList, Is.Empty);
        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(1));
        Assert.That(storage.CopyFileFromStorageCompressedCalls, Is.EqualTo(1));
        Assert.That(storage.CopyFileFromStorageEncryptedCalls, Is.EqualTo(1));

        Assert.That(storage.CopiedFromStorageRemoteFiles[0], Is.EqualTo(versionDate + @"\docs\" + "plain.txt"));
        Assert.That(storage.CopiedFromStorageCompressedRemoteFiles[0], Is.EqualTo(versionDate + @"\docs\" + "compressed.txt"));
        Assert.That(storage.CopiedFromStorageEncryptedRemoteFiles[0], Is.EqualTo(Path.Combine(versionDate, "_LONGFILES_", "very-long-file-name")));
    }

    [Test]
    public async Task TestRestoreSingleFileFilter()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");
        await SeedFileForVersionAsync(2, 2, 1, "other.txt", @"\docs\", 1, "");

        var storage = new StorageMock();
        var restoreJob = CreateRestoreJob(storage, file: @"\docs\plain.txt");

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(1));
        Assert.That(storage.CopiedFromStorageRemoteFiles[0], Does.Contain("plain.txt"));
        Assert.That(storage.CopiedFromStorageRemoteFiles[0], Does.Not.Contain("other.txt"));
    }

    [Test]
    public async Task TestOverwriteDontCopySkipsExistingFile()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        // RestoreJob gates overwrite on File.Exists — only local touch needed for this path.
        var destination = CreateExistingDestinationFile("plain.txt");

        var storage = new StorageMock();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.DontCopy);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task TestOverwritePolicyCopiesWhenExistingFilePresent()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateExistingDestinationFile("plain.txt");

        var storage = new StorageMock();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.Overwrite);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task TestOverwriteAskRespectsNoOverwriteDecision()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateExistingDestinationFile("plain.txt");

        var storage = new StorageMock();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.Ask);
        var observer = new JobReportStub { OverwriteResult = RequestOverwriteResult.NoOverwrite };
        restoreJob.AddObserver(observer);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(observer.RequestOverwriteCalls, Is.EqualTo(1));
        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task TestCancellationReportsCanceledState()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        using var cts = new CancellationTokenSource();
        var storage = new StorageMock(cancelOnCopy: cts);
        var restoreJob = CreateRestoreJob(storage, file: @"\");
        var observer = new JobReportStub();
        restoreJob.AddObserver(observer);

        await restoreJob.RestoreAsync(cts.Token);

        Assert.That(observer.ReportedStates, Does.Contain(JobState.CANCELED));
        Assert.That(observer.ReportedStates, Does.Not.Contain(JobState.FINISHED));
    }

    [Test]
    public async Task TestRestoreCollectsFileErrorsAndContinues()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "fails.txt", @"\docs\", 1, "");
        await SeedFileForVersionAsync(2, 2, 1, "ok.txt", @"\docs\", 1, "");

        var storage = new StorageMock(throwOnRemoteContaining: "fails.txt");
        var restoreJob = CreateRestoreJob(storage, file: @"\");
        var observer = new JobReportStub();
        restoreJob.AddObserver(observer);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList.Count, Is.EqualTo(1));
        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(1));
        Assert.That(storage.CopiedFromStorageRemoteFiles[0], Does.Contain("ok.txt"));
        Assert.That(observer.ReportedStates, Does.Contain(JobState.ERROR));
    }

    [Test]
    public async Task TestRestoreIncrementalVersionsUseLinkedFilePackages()
    {
        const string versionDate1 = "01-01-2021 00-00-00";
        const string versionDate2 = "02-01-2021 00-00-00";

        await SeedVersionAsync(1, versionDate1);
        await SeedVersionAsync(2, versionDate2);

        // Same logical file; version 2 links a newer fileversion package written in version 2.
        await SeedFileForVersionAsync(1, 1, 1, "doc.txt", @"\docs\", 1, "");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) VALUES " +
            "(2, 1, 1, '', '2021-01-02 00:00:00', '2021-01-01 00:00:00', 456, 2, 1, '')");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO filelink (fileversionID, versionID) VALUES (2, 2)");

        var storage = new StorageMock();

        var restoreV1 = CreateRestoreJob(storage, version: 1, file: @"\");
        await restoreV1.RestoreAsync(CancellationToken.None);

        var restoreV2 = CreateRestoreJob(storage, version: 2, file: @"\", overwrite: FileOverwrite.Overwrite);
        await restoreV2.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopyFileFromStorageCalls, Is.EqualTo(2));
        Assert.That(storage.CopiedFromStorageRemoteFiles[0], Is.EqualTo(versionDate1 + @"\docs\" + "doc.txt"));
        Assert.That(storage.CopiedFromStorageRemoteFiles[1], Is.EqualTo(versionDate2 + @"\docs\" + "doc.txt"));
    }

    private RestoreJob CreateRestoreJob(
        IStorageProvider storage,
        int version = 1,
        string destination = SyntheticDestination,
        string file = @"\",
        FileOverwrite overwrite = FileOverwrite.Overwrite)
    {
        return new RestoreJob(
            storage,
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository)
        {
            Version = version,
            File = file,
            Destination = destination,
            FileOverwrite = overwrite,
        };
    }

    private async Task SeedVersionAsync(int versionId, string versionDate)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable, versionSources) VALUES ({versionId}, '{versionDate}', 'v{versionId}', '', 2, 0, 1, 'D:\\\\Source')");
    }

    private async Task SeedFileForVersionAsync(
        int fileId,
        int fileVersionId,
        int versionId,
        string fileName,
        string filePath,
        int fileType,
        string longFileName)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO filetable (fileID, fileName, filePath) VALUES ({fileId}, '{fileName}', '{filePath}')");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) VALUES " +
            $"({fileVersionId}, 1, {fileType}, '', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 123, {versionId}, {fileId}, '{longFileName}')");
        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO filelink (fileversionID, versionID) VALUES ({fileVersionId}, {versionId})");
    }

    /// <summary>
    /// Creates a real destination file only where RestoreJob checks File.Exists for overwrite policy.
    /// </summary>
    private static string CreateExistingDestinationFile(string fileName)
    {
        var destination = Path.Combine(Path.GetTempPath(), "BSH.Test", "restore-unit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(destination, fileName), "existing");
        return destination;
    }

    private sealed class JobReportStub : IJobReport
    {
        public RequestOverwriteResult OverwriteResult { get; set; } = RequestOverwriteResult.Overwrite;
        public int RequestOverwriteCalls { get; private set; }
        public List<JobState> ReportedStates { get; } = [];

        public void ReportAction(ActionType action, bool silent) { }
        public void ReportState(JobState jobState) => ReportedStates.Add(jobState);
        public void ReportStatus(string title, string text) { }
        public void ReportProgress(int total, int current) { }
        public void ReportFileProgress(string file) { }
        public void ReportExceptions(Collection<FileExceptionEntry> files, bool silent) { }
        public Task RequestShowErrorInsufficientDiskSpaceAsync() => Task.CompletedTask;

        public Task<RequestOverwriteResult> RequestOverwrite(FileTableRow localFile, FileTableRow remoteFile)
        {
            RequestOverwriteCalls++;
            return Task.FromResult(OverwriteResult);
        }
    }
}
