// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Storage;
using NUnit.Framework;

namespace BSH.Test;

public class RestoreTests
{
    private const string TestDbName = "testdb_restore.db";

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
        var storage = new RecordingRestoreStorage(failCheckMedium: true);
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

        var destination = CreateTempDestination();
        var storage = new RecordingRestoreStorage();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\");
        restoreJob.Password = "test123";

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList, Is.Empty);
        Assert.That(storage.CopiedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.CopiedCompressed, Has.Count.EqualTo(1));
        Assert.That(storage.CopiedEncrypted, Has.Count.EqualTo(1));

        Assert.That(storage.CopiedPlain[0].RemoteFile, Is.EqualTo(versionDate + @"\docs\" + "plain.txt"));
        Assert.That(storage.CopiedCompressed[0].RemoteFile, Is.EqualTo(versionDate + @"\docs\" + "compressed.txt"));
        Assert.That(storage.CopiedEncrypted[0].RemoteFile, Is.EqualTo(Path.Combine(versionDate, "_LONGFILES_", "very-long-file-name")));
    }

    [Test]
    public async Task TestRestoreSingleFileFilter()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");
        await SeedFileForVersionAsync(2, 2, 1, "other.txt", @"\docs\", 1, "");

        var destination = CreateTempDestination();
        var storage = new RecordingRestoreStorage();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\docs\plain.txt");

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopiedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.CopiedPlain[0].RemoteFile, Does.Contain("plain.txt"));
    }

    [Test]
    public async Task TestOverwriteDontCopySkipsExistingFile()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateTempDestination();
        Directory.CreateDirectory(destination);
        var existingPath = Path.Combine(destination, "plain.txt");
        File.WriteAllText(existingPath, "local-content");

        var storage = new RecordingRestoreStorage();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.DontCopy);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopiedPlain, Is.Empty);
        Assert.That(File.ReadAllText(existingPath), Is.EqualTo("local-content"));
    }

    [Test]
    public async Task TestOverwritePolicyReplacesExistingFile()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateTempDestination();
        Directory.CreateDirectory(destination);
        var existingPath = Path.Combine(destination, "plain.txt");
        File.WriteAllText(existingPath, "local-content");

        var storage = new RecordingRestoreStorage(writeRestoredContent: "restored-content");
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.Overwrite);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopiedPlain, Has.Count.EqualTo(1));
        Assert.That(File.ReadAllText(existingPath), Is.EqualTo("restored-content"));
    }

    [Test]
    public async Task TestOverwriteAskRespectsNoOverwriteDecision()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateTempDestination();
        Directory.CreateDirectory(destination);
        var existingPath = Path.Combine(destination, "plain.txt");
        File.WriteAllText(existingPath, "local-content");

        var storage = new RecordingRestoreStorage();
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\", overwrite: FileOverwrite.Ask);
        var observer = new JobReportStub { OverwriteResult = RequestOverwriteResult.NoOverwrite };
        restoreJob.AddObserver(observer);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(observer.RequestOverwriteCalls, Is.EqualTo(1));
        Assert.That(storage.CopiedPlain, Is.Empty);
        Assert.That(File.ReadAllText(existingPath), Is.EqualTo("local-content"));
    }

    [Test]
    public async Task TestCancellationReportsCanceledState()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var destination = CreateTempDestination();
        using var cts = new CancellationTokenSource();
        var storage = new RecordingRestoreStorage(cancelOnCopy: cts);
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\");
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

        var destination = CreateTempDestination();
        var storage = new RecordingRestoreStorage(throwOnRemoteContaining: "fails.txt");
        var restoreJob = CreateRestoreJob(storage, destination: destination, file: @"\");
        var observer = new JobReportStub();
        restoreJob.AddObserver(observer);

        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList.Count, Is.EqualTo(1));
        Assert.That(storage.CopiedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.CopiedPlain[0].RemoteFile, Does.Contain("ok.txt"));
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

        var destination = CreateTempDestination();
        var storage = new RecordingRestoreStorage();

        var restoreV1 = CreateRestoreJob(storage, version: 1, destination: destination, file: @"\");
        await restoreV1.RestoreAsync(CancellationToken.None);

        var restoreV2 = CreateRestoreJob(storage, version: 2, destination: destination, file: @"\", overwrite: FileOverwrite.Overwrite);
        await restoreV2.RestoreAsync(CancellationToken.None);

        Assert.That(storage.CopiedPlain, Has.Count.EqualTo(2));
        Assert.That(storage.CopiedPlain[0].RemoteFile, Is.EqualTo(versionDate1 + @"\docs\" + "doc.txt"));
        Assert.That(storage.CopiedPlain[1].RemoteFile, Is.EqualTo(versionDate2 + @"\docs\" + "doc.txt"));
    }

    private RestoreJob CreateRestoreJob(
        IStorageProvider storage,
        int version = 1,
        string destination = null,
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

    private static string CreateTempDestination()
    {
        var path = Path.Combine(Path.GetTempPath(), "BSH.Test", "restore-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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

    private sealed class RecordingRestoreStorage : IStorageProvider
    {
        private readonly bool failCheckMedium;
        private readonly string throwOnRemoteContaining;
        private readonly string writeRestoredContent;
        private readonly CancellationTokenSource cancelOnCopy;

        public RecordingRestoreStorage(
            bool failCheckMedium = false,
            string throwOnRemoteContaining = null,
            string writeRestoredContent = null,
            CancellationTokenSource cancelOnCopy = null)
        {
            this.failCheckMedium = failCheckMedium;
            this.throwOnRemoteContaining = throwOnRemoteContaining;
            this.writeRestoredContent = writeRestoredContent;
            this.cancelOnCopy = cancelOnCopy;
        }

        public StorageProviderKind Kind => StorageProviderKind.LocalFileSystem;
        public List<(string LocalFile, string RemoteFile)> CopiedPlain { get; } = [];
        public List<(string LocalFile, string RemoteFile)> CopiedCompressed { get; } = [];
        public List<(string LocalFile, string RemoteFile, string Password)> CopiedEncrypted { get; } = [];

        public Task<bool> CheckMedium(bool quickCheck = false) => Task.FromResult(!failCheckMedium);
        public void Open() { }
        public bool CanWriteToStorage() => true;
        public bool CopyFileToStorage(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageCompressed(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password) => true;
        public bool DecryptOnStorage(string remoteFile, string password) => true;
        public bool DeleteFileFromStorage(string remoteFile) => true;
        public bool DeleteFileFromStorageCompressed(string remoteFile) => true;
        public bool DeleteFileFromStorageEncrypted(string remoteFile) => true;
        public bool DeleteDirectory(string remoteDirectory) => true;
        public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget) => true;
        public bool UploadDatabaseFile(string databaseFile) => true;
        public void UpdateStorageVersion(int versionId) { }
        public bool IsPathTooLong(string path, bool compression, bool encryption) => false;
        public long GetFreeSpace() => 0;
        public void Dispose() { }

        public bool CopyFileFromStorage(string localFile, string remoteFile)
        {
            return CopyFromStorage(CopiedPlain, localFile, remoteFile);
        }

        public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
        {
            return CopyFromStorage(CopiedCompressed, localFile, remoteFile);
        }

        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
        {
            if (!string.IsNullOrEmpty(throwOnRemoteContaining) && remoteFile.Contains(throwOnRemoteContaining, StringComparison.Ordinal))
            {
                throw new IOException("Simulated restore failure");
            }

            cancelOnCopy?.Cancel();
            WriteRestoredContentIfRequested(localFile);
            CopiedEncrypted.Add((localFile, remoteFile, password));
            return true;
        }

        private bool CopyFromStorage(
            List<(string LocalFile, string RemoteFile)> target,
            string localFile,
            string remoteFile)
        {
            if (!string.IsNullOrEmpty(throwOnRemoteContaining) && remoteFile.Contains(throwOnRemoteContaining, StringComparison.Ordinal))
            {
                throw new IOException("Simulated restore failure");
            }

            cancelOnCopy?.Cancel();
            WriteRestoredContentIfRequested(localFile);
            target.Add((localFile, remoteFile));
            return true;
        }

        private void WriteRestoredContentIfRequested(string localFile)
        {
            if (writeRestoredContent == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localFile)!);
            File.WriteAllText(localFile, writeRestoredContent);
        }
    }
}
