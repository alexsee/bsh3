// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Service.Jobs;
using Brightbits.BSH.Engine.Types;
using Brightbits.BSH.Engine.Types.Exceptions;
using Brightbits.BSH.Engine.Providers.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class RestoreJobTests
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;

    [SetUp]
    public async Task SetUp()
    {
        if (File.Exists("testdb_restore.db"))
        {
            DbClientFactory.ClosePool();
            File.Delete("testdb_restore.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_restore.db"));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.BackupFolder = @"X:\Backups";
        configurationManager.SourceFolder = @"Y:\source";
        configurationManager.OldBackupPrevent = "1";

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
        versionQueryRepository = new VersionQueryRepository();

        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionStable, versionSources, versionStatus, versionType) " +
            "VALUES (1, '01-01-2021 00-00-00', 'v1', 'desc', 1, 'Y:\\source', 0, 2)");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filetable (fileID, fileName, filePath) VALUES (1, 'notes.txt', '\\source\\')");
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) " +
            "VALUES (1, 0, 1, 'h', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 9, 1, 1, '')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 1)");
    }

    [TearDown]
    public void TearDown()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_restore.db"))
        {
            File.Delete("testdb_restore.db");
        }
    }

    [Test]
    public void RestoreAsync_Throws_WhenMediumNotReady()
    {
        var job = new RestoreJob(
            new StorageMock(failCheckMedium: true),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository)
        {
            Version = 1,
            File = @"\source\notes.txt",
            Destination = Path.Combine(Path.GetTempPath(), "bsh-restore-out"),
            FileOverwrite = FileOverwrite.Overwrite
        };

        Assert.ThrowsAsync<DeviceNotReadyException>(async () => await job.RestoreAsync(CancellationToken.None));
    }

    [Test]
    public async Task RestoreAsync_Completes_ForSingleFileWithStorageMock()
    {
        var destination = Path.Combine(Path.GetTempPath(), "bsh-restore-out-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(destination);

        try
        {
            var job = new RestoreJob(
                new StorageMock(),
                dbClientFactory,
                queryManager,
                configurationManager,
                versionQueryRepository)
            {
                Version = 1,
                File = @"\source\notes.txt",
                Destination = destination,
                FileOverwrite = FileOverwrite.Overwrite
            };

            await job.RestoreAsync(CancellationToken.None);
        }
        finally
        {
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: true);
            }
        }
    }
}
