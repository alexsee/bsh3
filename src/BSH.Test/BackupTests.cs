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
    public async Task TestIncrementalCreatesNewFileVersionForSameSizeEditWithinSameSecond()
    {
        var firstModified = new DateTime(2024, 1, 2, 3, 4, 5, 100, DateTimeKind.Local);
        var secondModified = new DateTime(2024, 1, 2, 3, 4, 5, 900, DateTimeKind.Local);
        var created = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Local);

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            new List<FolderTableRow>(),
            new List<FileTableRow>
            {
                new FileTableRow
                {
                    FileName = "test.txt",
                    FilePath = "",
                    FileRoot = "D:\\Meine Dokumente",
                    FileSize = 1024,
                    FileDateCreated = created,
                    FileDateModified = firstModified,
                }
            });

        var firstBackupJob = new BackupJob(new StorageMock(), dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = "D:\\Meine Dokumente",
            Title = "Blub",
            Description = "",
        };

        var token = new CancellationTokenSource().Token;
        await firstBackupJob.BackupAsync(token);

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            new List<FolderTableRow>(),
            new List<FileTableRow>
            {
                new FileTableRow
                {
                    FileName = "test.txt",
                    FilePath = "",
                    FileRoot = "D:\\Meine Dokumente",
                    FileSize = 1024,
                    FileDateCreated = created,
                    FileDateModified = secondModified,
                }
            });

        var secondBackupJob = new BackupJob(new StorageMock(), dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = "D:\\Meine Dokumente",
            Title = "Blub",
            Description = "",
        };

        await secondBackupJob.BackupAsync(token);

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(2));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileversionID FROM filelink WHERE versionID = 2")), Is.EqualTo(2));
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
    public async Task FullBackup_ForcesNewPackagesForUnchangedFiles()
    {
        var modified = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Local);
        var created = new DateTime(2024, 1, 2, 3, 0, 0, DateTimeKind.Local);

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            [],
            [
                new FileTableRow
                {
                    FileName = "stable.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 2048,
                    FileDateCreated = created,
                    FileDateModified = modified,
                }
            ]);

        var storage = new StorageMock();
        var first = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "first",
            Description = "",
        };
        await first.BackupAsync(CancellationToken.None);

        var second = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "full",
            Description = "",
            FullBackup = true,
        };
        await second.BackupAsync(CancellationToken.None);

        Assert.That(storage.CopyFileToStorageCalls, Is.EqualTo(2));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(2));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink")), Is.EqualTo(2));
    }

    [Test]
    public async Task Backup_PersistsEmptyFolders()
    {
        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            [new FolderTableRow(@"D:\Meine Dokumente\empty", @"D:\Meine Dokumente")],
            [
                new FileTableRow
                {
                    FileName = "keep.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 10,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            ]);

        var storage = new StorageMock();
        var backupJob = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "folders",
            Description = "",
        };
        await backupJob.BackupAsync(CancellationToken.None);

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM foldertable")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM folderlink")), Is.EqualTo(1));
    }

    [Test]
    public async Task Backup_NoChanges_RefreshesVersionDirectoryViaRename()
    {
        var modified = new DateTime(2024, 5, 1, 12, 0, 0, DateTimeKind.Local);
        FileTableRow UnchangedFile() => new()
        {
            FileName = "same.txt",
            FilePath = "",
            FileRoot = @"D:\Meine Dokumente",
            FileSize = 100,
            FileDateCreated = modified,
            FileDateModified = modified,
        };

        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock([], [UnchangedFile()]);

        var storage = new StorageMock();
        var first = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "first",
            Description = "",
        };
        await first.BackupAsync(CancellationToken.None);
        var version1 = await queryManager.GetLastBackupAsync();
        Assert.That(version1, Is.Not.Null);

        // Fresh row instances: BackupJob mutates FilePath on the collected rows.
        await Task.Delay(1100);
        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock([], [UnchangedFile()]);
        var second = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "refresh",
            Description = "",
        };
        await second.BackupAsync(CancellationToken.None);

        Assert.That(storage.CopyFileToStorageCalls, Is.EqualTo(1), "Unchanged file must not be recopied.");
        Assert.That(storage.RenamedDirectories, Has.Count.EqualTo(1));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM versiontable WHERE versionStatus = 0")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(1));
    }

    [Test]
    public async Task Backup_AbortsWhenFreeSpaceClearlyInsufficient()
    {
        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            [],
            [
                new FileTableRow
                {
                    FileName = "big.bin",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 5_000_000,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            ]);

        var storage = new StorageMock(freeSpaceBytes: 1000);
        var observer = new Helpers.JobReportStub();
        var backupJob = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "disk",
            Description = "",
        };
        backupJob.AddObserver(observer);

        await backupJob.BackupAsync(CancellationToken.None);

        Assert.That(observer.ReportedStates, Does.Contain(JobState.ERROR));
        Assert.That(observer.InsufficientDiskSpaceCalls, Is.EqualTo(1));
        Assert.That(storage.CopyFileToStorageCalls, Is.EqualTo(0));
        Assert.That(await queryManager.GetLastBackupAsync(), Is.Null);
    }

    [Test]
    public async Task Backup_UnchangedFile_LinksExistingPackageOnIncremental()
    {
        var modified = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Local);
        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            [],
            [
                new FileTableRow
                {
                    FileName = "link.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 512,
                    FileDateCreated = modified,
                    FileDateModified = modified,
                }
            ]);

        var storage = new StorageMock();
        var first = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "v1",
            Description = "",
        };
        await first.BackupAsync(CancellationToken.None);

        // Force a distinct version date so we get a real second version with a new file (plus the unchanged one).
        await Task.Delay(1100);
        fileCollectorServiceFactory = new FileCollectorServiceFactoryMock(
            [],
            [
                new FileTableRow
                {
                    FileName = "link.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 512,
                    FileDateCreated = modified,
                    FileDateModified = modified,
                },
                new FileTableRow
                {
                    FileName = "new.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 64,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            ]);

        var second = new BackupJob(storage, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory, vssClient, versionQueryRepository, backupMutationRepository)
        {
            SourceFolder = @"D:\Meine Dokumente",
            Title = "v2",
            Description = "",
        };
        await second.BackupAsync(CancellationToken.None);

        Assert.That(storage.CopyFileToStorageCalls, Is.EqualTo(2), "Only the new file should be copied on the second run.");

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(2));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM filelink WHERE fileversionID = 1 AND versionID = 2")), Is.EqualTo(1));
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
