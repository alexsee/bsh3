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
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Services;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Black-box tests for <see cref="BackupService"/> orchestration (password gate, job dispatch).
/// </summary>
public class BackupServiceTests
{
    private string dbPath;
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;
    private IBackupMutationRepository backupMutationRepository;
    private StorageMock storage;
    private StorageFactoryMock storageFactory;
    private FileCollectorServiceFactoryMock fileCollectorFactory;
    private VssClientMock vssClient;
    private BackupService backupService;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            dbClientFactory = null;
        }

        dbPath = Path.Combine(Path.GetTempPath(), "BSH.Test", "backup-service-" + Guid.NewGuid().ToString("N") + ".db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(dbPath);

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.SourceFolder = @"D:\Meine Dokumente";
        configurationManager.OldBackupPrevent = "0";

        versionQueryRepository = new VersionQueryRepository();
        backupMutationRepository = new BackupMutationRepository(dbClientFactory);
        storage = new StorageMock();
        storageFactory = new StorageFactoryMock(storage);
        vssClient = new VssClientMock();

        fileCollectorFactory = new FileCollectorServiceFactoryMock(
            [],
            [
                new FileTableRow
                {
                    FileName = "test.txt",
                    FilePath = "",
                    FileRoot = @"D:\Meine Dokumente",
                    FileSize = 128,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            ]);

        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
        backupService = new BackupService(
            configurationManager,
            queryManager,
            dbClientFactory,
            storageFactory,
            vssClient,
            fileCollectorFactory,
            versionQueryRepository,
            backupMutationRepository);
    }

    [TearDown]
    public void Cleanup()
    {
        storage?.Dispose();
        storage = null;

        DbClientFactory.ClosePool();

        try
        {
            if (!string.IsNullOrEmpty(dbPath) && File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
        catch
        {
            // ignore
        }
    }

    [Test]
    public void StartBackup_RequiresPasswordWhenEncryptionEnabled()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = "hash";

        Assert.Throws<PasswordRequiredException>(() =>
            backupService.StartBackup("t", "", new JobReportStub(), CancellationToken.None));
    }

    [Test]
    public async Task StartBackup_RunsWhenPasswordProvided()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = "cc03e747a6afbbcbf8be7668acfebee5";
        backupService.SetPassword("test123");

        var observer = new JobReportStub();
        await backupService.StartBackup("encrypted", "", observer, CancellationToken.None);

        Assert.That(observer.ReportedStates, Does.Contain(JobState.FINISHED));
        Assert.That(storage.CopyFileToStorageEncryptedCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task StartDelete_DispatchesDeleteJob()
    {
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, "01-01-2021 00-00-00");
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "a.txt", @"\docs\", 1, "");

        var observer = new JobReportStub();
        await backupService.StartDelete("1", observer, CancellationToken.None);

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(observer.ReportedStates, Does.Contain(JobState.FINISHED));
    }

    [Test]
    public async Task SetStableAsync_PersistsStableFlag()
    {
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, "01-01-2021 00-00-00");
        await backupService.SetStableAsync("1", stable: false);

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT versionStable FROM versiontable WHERE versionID = 1")), Is.EqualTo(0));
    }

    [Test]
    public async Task UpdateVersionAsync_UpdatesTitleAndDescription()
    {
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, "01-01-2021 00-00-00");
        await backupService.UpdateVersionAsync("1", new VersionDetails
        {
            Title = "Renamed",
            Description = "Updated description",
        });

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(await dbClient.ExecuteScalarAsync("SELECT versionTitle FROM versiontable WHERE versionID = 1"), Is.EqualTo("Renamed"));
        Assert.That(await dbClient.ExecuteScalarAsync("SELECT versionDescription FROM versiontable WHERE versionID = 1"), Is.EqualTo("Updated description"));
    }
}
