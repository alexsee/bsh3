// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Services.FileCollector;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test.Integration;

/// <summary>
/// Real filesystem backup ↔ restore round-trips via <see cref="FileSystemStorage"/>.
/// </summary>
[Category("Integration")]
public class FileSystemRestoreIntegrationTests
{
    private const string TestDbName = "testdb_fs_restore.db";
    private const string SampleContent = "backup-service-home-integration-content-0123456789";
    private const string Password = "test123";

    private string rootDir;
    private string sourceDir;
    private string backupDir;
    private string restoreDir;
    private string dbPath;

    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;
    private IBackupMutationRepository backupMutationRepository;
    private IFileCollectorServiceFactory fileCollectorServiceFactory;
    private VssClientMock vssClient;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            dbClientFactory = null;
        }

        rootDir = Path.Combine(Path.GetTempPath(), "BSH.Test", "fs-" + Guid.NewGuid().ToString("N"));
        sourceDir = Path.Combine(rootDir, "source");
        backupDir = Path.Combine(rootDir, "backup");
        restoreDir = Path.Combine(rootDir, "restore");
        dbPath = Path.Combine(rootDir, TestDbName);

        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(backupDir);
        Directory.CreateDirectory(restoreDir);

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(dbPath);

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.BackupFolder = backupDir;
        configurationManager.SourceFolder = sourceDir;
        configurationManager.MediaVolumeSerial = "";
        configurationManager.OldBackupPrevent = "0";

        versionQueryRepository = new VersionQueryRepository();
        backupMutationRepository = new BackupMutationRepository(dbClientFactory);
        fileCollectorServiceFactory = new FileCollectorServiceFactory();
        vssClient = new VssClientMock();

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();

        try
        {
            if (!string.IsNullOrEmpty(rootDir) && Directory.Exists(rootDir))
            {
                Directory.Delete(rootDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup on Windows file locks.
        }
    }

    [Test]
    public async Task BackupAndRestore_PlainContentMatches()
    {
        var sourceFile = WriteSourceFile("notes.txt", SampleContent);

        await RunBackupAsync();
        await RunRestoreAsync(overwrite: FileOverwrite.Overwrite);

        var restoredFile = Path.Combine(restoreDir, Path.GetFileName(sourceFile));
        Assert.That(File.Exists(restoredFile), Is.True);
        Assert.That(File.ReadAllText(restoredFile), Is.EqualTo(SampleContent));
    }

    [Test]
    public async Task BackupAndRestore_CompressedContentMatches()
    {
        configurationManager.Compression = 1;
        WriteSourceFile("report.txt", SampleContent + "-compressed");

        await RunBackupAsync();
        await RunRestoreAsync(overwrite: FileOverwrite.Overwrite);

        var restoredFile = Path.Combine(restoreDir, "report.txt");
        Assert.That(File.ReadAllText(restoredFile), Is.EqualTo(SampleContent + "-compressed"));
    }

    [Test]
    public async Task BackupAndRestore_EncryptedContentMatches()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = Brightbits.BSH.Engine.Security.Hash.GetMD5Hash(Password);
        WriteSourceFile("secret.txt", SampleContent + "-encrypted");

        await RunBackupAsync(password: Password);

        var restoreJob = CreateRestoreJob(CreateStorage(), password: Password);
        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList, Is.Empty);
        Assert.That(File.ReadAllText(Path.Combine(restoreDir, "secret.txt")), Is.EqualTo(SampleContent + "-encrypted"));
    }

    [Test]
    public async Task IncrementalBackup_RestoreOlderAndNewerVersions()
    {
        var sourceFile = WriteSourceFile("doc.txt", "version-one-content-abcdefghijklmnopqrstuvwxyz");

        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();
        Assert.That(version1, Is.Not.Null);

        // Version dates are second-precision; wait so storage folders stay unique.
        await Task.Delay(1100);

        File.WriteAllText(sourceFile, "version-two-content-abcdefghijklmnopqrstuvwxyz");
        File.SetLastWriteTimeUtc(sourceFile, DateTime.UtcNow.AddMinutes(1));

        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();
        Assert.That(version2, Is.Not.Null);
        Assert.That(version2.Id, Is.Not.EqualTo(version1.Id));

        var restoreV1Dir = Path.Combine(restoreDir, "v1");
        var restoreV2Dir = Path.Combine(restoreDir, "v2");
        Directory.CreateDirectory(restoreV1Dir);
        Directory.CreateDirectory(restoreV2Dir);

        var storage = CreateStorage();
        var restoreV1 = CreateRestoreJob(storage, version: int.Parse(version1.Id), destination: restoreV1Dir);
        await restoreV1.RestoreAsync(CancellationToken.None);
        Assert.That(restoreV1.FileErrorList, Is.Empty);
        Assert.That(File.ReadAllText(Path.Combine(restoreV1Dir, "doc.txt")), Is.EqualTo("version-one-content-abcdefghijklmnopqrstuvwxyz"));

        var restoreV2 = CreateRestoreJob(storage, version: int.Parse(version2.Id), destination: restoreV2Dir);
        await restoreV2.RestoreAsync(CancellationToken.None);
        Assert.That(restoreV2.FileErrorList, Is.Empty);
        Assert.That(File.ReadAllText(Path.Combine(restoreV2Dir, "doc.txt")), Is.EqualTo("version-two-content-abcdefghijklmnopqrstuvwxyz"));
    }

    private async Task RunBackupAsync(string password = null)
    {
        var backupJob = new BackupJob(
            CreateStorage(),
            dbClientFactory,
            queryManager,
            configurationManager,
            fileCollectorServiceFactory,
            vssClient,
            versionQueryRepository,
            backupMutationRepository)
        {
            SourceFolder = sourceDir,
            Title = "Integration",
            Description = "",
            Password = password,
        };

        await backupJob.BackupAsync(CancellationToken.None);

        Assert.That(backupJob.FileErrorList, Is.Empty, "Backup reported file errors.");
        var version = await queryManager.GetLastBackupAsync();
        Assert.That(version, Is.Not.Null, "Backup did not create a version.");
    }

    private async Task RunRestoreAsync(FileOverwrite overwrite)
    {
        var restoreJob = CreateRestoreJob(CreateStorage(), overwrite: overwrite);
        await restoreJob.RestoreAsync(CancellationToken.None);
        Assert.That(restoreJob.FileErrorList, Is.Empty, "Restore reported file errors.");
    }

    private RestoreJob CreateRestoreJob(
        IStorageProvider storage,
        int version = 1,
        string destination = null,
        string password = null,
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
            File = @"\",
            Destination = destination ?? restoreDir,
            FileOverwrite = overwrite,
            Password = password,
        };
    }

    private IStorageProvider CreateStorage() => new FileSystemStorage(configurationManager);

    private string WriteSourceFile(string fileName, string content)
    {
        var path = Path.Combine(sourceDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
