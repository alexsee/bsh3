// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
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

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
        backupService = new BackupService(configurationManager, queryManager, dbClientFactory, storageFactory);

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
    }

    [Test]
    public void TestEmptySources()
    {
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
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
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
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
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.AreEqual("1", version.Id);

        // start second backup
        await backupJob.BackupAsync(token);

        // check version
        version = await this.queryManager.GetLastBackupAsync();
        Assert.AreEqual("2", version.Id);
    }

    [Test]
    public async Task TestCompressedFull()
    {
        // set compressed state
        this.configurationManager.Compression = 1;

        // generate backup job
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.AreEqual("1", version.Id);
    }

    [Test]
    public async Task TestEncryptedFull()
    {
        // set compressed state
        this.configurationManager.Encrypt = 1;
        this.configurationManager.EncryptPassMD5 = "cc03e747a6afbbcbf8be7668acfebee5";

        // generate backup job
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";
        backupJob.Password = "test123";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.AreEqual("1", version.Id);
    }

    [Test]
    public async Task TestFullFail()
    {
        // generate backup job
        var fs = new StorageMock(false, true);

        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
        backupJob.SourceFolder = "D:\\Meine Dokumente";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        Assert.NotZero(backupJob.FileErrorList.Count);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.Null(version);
    }

    [Test]
    public async Task TestFullDrive()
    {
        // generate backup job
        var fs = new StorageMock();

        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
        backupJob.SourceFolder = "D:\\";
        backupJob.Title = "Blub";
        backupJob.Description = "";

        // start backup
        var token = new CancellationTokenSource().Token;
        await backupJob.BackupAsync(token);

        // check version
        var version = await this.queryManager.GetLastBackupAsync();
        Assert.AreEqual("1", version.Id);
    }

    [Test]
    public async Task TestCancellation()
    {
        var fs = new StorageMock();
        var backupJob = new BackupJob(fs, dbClientFactory, queryManager, configurationManager, fileCollectorServiceFactory);
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
        Assert.Null(version);
    }
}