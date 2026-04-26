// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class BackupTests
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IFileCollectorServiceFactory fileCollectorServiceFactory;
    private IVersionQueryRepository versionQueryRepository;
    private IBackupMutationRepository backupMutationRepository;
    private VssClientMock vssClient;

    private IBackupService backupService;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            this.dbClientFactory = null;
        }

        // start with clean database
        if (File.Exists("testdb.db"))
        {
            File.Delete("testdb.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Environment.CurrentDirectory + "\\testdb.db");

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        vssClient = new VssClientMock();
        versionQueryRepository = new VersionQueryRepository();
        backupMutationRepository = new BackupMutationRepository(dbClientFactory);

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            new System.Collections.Generic.List<Brightbits.BSH.Engine.Models.FolderTableRow>(),
            new System.Collections.Generic.List<Brightbits.BSH.Engine.Models.FileTableRow>()
            {
                new Brightbits.BSH.Engine.Models.FileTableRow()
                {
                    FileName = "test.txt",
                    FilePath = "D:\\Meine Dokumente\\test.txt",
                    FileRoot = "D:\\Meine Dokumente",
                    FileSize = 1024,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            }
        );

        backupService = new BackupService(configurationManager, queryManager, dbClientFactory, storageFactory, vssClient, fileCollectorServiceFactory, versionQueryRepository, backupMutationRepository);
    }

    [Test]
    public void TestEmptySources()
    {
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        Assert.ThrowsAsync<NoSourceFolderSelectedException>(async () => await backupJob.BackupAsync(token));
    }

    [Test]
    public void TestFailMedium()
    {
        var fs = new StorageMock(failCheckMedium: true);
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        Assert.ThrowsAsync<DeviceNotReadyException>(async () => await backupJob.BackupAsync(token));
    }

    [Test]
    public async Task TestSimpleFullAndIncremental()
    {
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version.Id, Is.EqualTo("1"));

        // start second backup
        await backupJob.BackupAsync(token);

        // check version
        version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version.Id, Is.EqualTo("2"));
    }

    [Test]
    public async Task TestCompressedFull()
    {
        // set compressed state
        this.configurationManager.Compression = 1;

        // generate backup job
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version.Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task TestUsesCompressedCopyWhenCompressionIsEnabled()
    {
        configurationManager.Compression = 1;

        var fs = new StorageMock();
        var backupJob = CreateBackupJobForSingleTempFile(fs, new VssClientMock(), ".txt");

        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.That(fs.CopyFileToStorageCompressedCalls, Is.EqualTo(1));
        Assert.That(fs.CopyFileToStorageEncryptedCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task TestUsesEncryptedCopyWhenEncryptionIsEnabled()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = "cc03e747a6afbbcbf8be7668acfebee5";

        var fs = new StorageMock();
        var backupJob = CreateBackupJobForSingleTempFile(fs, new VssClientMock(), ".txt");
        backupJob.Password = "test123";

        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.That(fs.CopyFileToStorageEncryptedCalls, Is.EqualTo(1));
        Assert.That(fs.CopyFileToStorageCompressedCalls, Is.EqualTo(0));
    }

    [Test]
    public async Task TestUsesLongFilePathFallbackWhenPathIsTooLong()
    {
        var fs = new StorageMock(pathTooLong: true);
        var backupJob = CreateBackupJobForSingleTempFile(fs, new VssClientMock(), ".txt");

        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.That(fs.LastRemoteFile, Does.Contain("_LONGFILES_"));
    }

    [Test]
    public async Task TestRetriesWithVssAfterIoException()
    {
        var vss = new VssClientMock(shouldCopy: true);
        var fs = new StorageMock(throwIoOnFirstRegularCopy: true);
        var backupJob = CreateBackupJobForSingleTempFile(fs, vss, ".txt");

        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.That(vss.CopyCalls, Is.EqualTo(1));
        Assert.That(fs.CopyFileToStorageCalls, Is.EqualTo(2));
        Assert.That(backupJob.FileErrorList, Is.Empty);
    }

    [Test]
    public async Task TestEncryptedFull()
    {
        // set compressed state
        this.configurationManager.Encrypt = 1;
        this.configurationManager.EncryptPassMD5 = "cc03e747a6afbbcbf8be7668acfebee5";

        // generate backup job
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";
        backupJob.Password = "test123";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version.Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task TestFullFail()
    {
        // generate backup job
        var fs = new StorageMock(false, true);

        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.That(backupJob.FileErrorList.Count, Is.Not.Zero);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version, Is.Null);
    }

    [Test]
    public async Task TestFullDrive()
    {
        // generate backup job
        var fs = new StorageMock();

        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version.Id, Is.EqualTo("1"));
    }

    [Test]
    public async Task TestCancellation()
    {
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;
        await tokenSource.CancelAsync();

        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.That(version, Is.Null);
    }

    [Test]
    public async Task EstimateBackupSpaceAppliesSafetyMargin()
    {
        configurationManager.SourceFolder = "D:\\Meine Dokumente";

        var storageFactory = new StorageFactoryMock(() => new StorageMock(freeSpace: 1000));
        var service = new BackupService(configurationManager, queryManager, dbClientFactory, storageFactory, vssClient, fileCollectorServiceFactory, versionQueryRepository, backupMutationRepository);

        var result = await service.EstimateBackupSpaceAsync();

        Assert.That(result.AvailableSpace, Is.EqualTo(1000));
        Assert.That(result.EstimatedRequiredSpace, Is.EqualTo(1229));
        Assert.That(result.ShouldWarn, Is.True);
    }

    [Test]
    public async Task EstimateBackupSpaceSkipsUnchangedFiles()
    {
        configurationManager.SourceFolder = "D:\\Meine Dokumente";

        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        var storageFactory = new StorageFactoryMock(() => new StorageMock(freeSpace: 2000));
        var service = new BackupService(configurationManager, queryManager, dbClientFactory, storageFactory, vssClient, fileCollectorServiceFactory, versionQueryRepository, backupMutationRepository);

        var result = await service.EstimateBackupSpaceAsync();

        Assert.That(result.EstimatedRequiredSpace, Is.EqualTo(0));
        Assert.That(result.ShouldWarn, Is.False);
    }

    private BackupJob CreateBackupJobForSingleTempFile(StorageMock storage, VssClientMock vss, string extension)
    {
        var filePath = CreateTempFile(extension);
        var fileInfo = new FileInfo(filePath);

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            new List<FolderTableRow>(),
            new List<FileTableRow>
            {
                new FileTableRow
                {
                    FileName = fileInfo.Name,
                    FilePath = "",
                    FileRoot = fileInfo.DirectoryName,
                    FileSize = fileInfo.Length,
                    FileDateCreated = fileInfo.CreationTime,
                    FileDateModified = fileInfo.LastWriteTime,
                }
            });

        var backupJob = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vss, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = fileInfo.DirectoryName,
            Title = "Blub",
            Description = "",
        };

        return backupJob;
    }

    private static string CreateTempFile(string extension)
    {
        var testFilesDir = Path.Combine(Environment.CurrentDirectory, "testfiles");
        Directory.CreateDirectory(testFilesDir);

        var testFile = Path.Combine(testFilesDir, $"{Guid.NewGuid()}{extension}");
        var content = new string('a', 256);
        File.WriteAllText(testFile, content);
        return testFile;
    }
}
