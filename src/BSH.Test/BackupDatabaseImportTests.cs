// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class BackupDatabaseImportTests
{
    [Test]
    public void SuccessfulDownloadReplacesDatabaseAndRemovesOldSidecars()
    {
        var directory = Directory.CreateTempSubdirectory("bsh-test-import-").FullName;
        try
        {
            var destination = Path.Combine(directory, "backup.bshdb");
            File.WriteAllText(destination, "old database");
            File.WriteAllText(destination + "-wal", "old journal");
            File.WriteAllText(destination + "-shm", "old shared memory");
            using var storage = new StorageMock();

            BackupDatabaseImport.DownloadFromStorage(storage, destination);

            Assert.That(File.ReadAllText(destination), Is.Empty);
            Assert.That(storage.CopiedFromStorageRemoteFiles, Is.EqualTo(new[] { "backup.bshdb" }));
            Assert.That(Directory.GetFiles(directory), Is.EqualTo(new[] { destination }));
        }
        finally
        {
            TempDirectory.DeleteBestEffort(directory);
        }
    }

    [Test]
    public async Task FailedDownloadPreservesTheCurrentDatabase()
    {
        var directory = Directory.CreateTempSubdirectory("bsh-test-import-").FullName;
        try
        {
            var destination = Path.Combine(directory, "backup.bshdb");
            await File.WriteAllTextAsync(destination, "current backup");
            using var storage = new StorageMock(failAllCopies: true);

            Assert.Throws<IOException>(() => BackupDatabaseImport.DownloadFromStorage(storage, destination));
            Assert.That(await File.ReadAllTextAsync(destination), Is.EqualTo("current backup"));
            Assert.That(Directory.GetFiles(directory), Is.EqualTo(new[] { destination }));
        }
        finally
        {
            TempDirectory.DeleteBestEffort(directory);
        }
    }

    [Test]
    public async Task RemapVersionSourcesAppliesTheMostSpecificMappingOnce()
    {
        var directory = Directory.CreateTempSubdirectory("bsh-test-import-").FullName;
        try
        {
            var dbClientFactory = new DbClientFactory();
            await dbClientFactory.InitializeAsync(Path.Combine(directory, "backup.bshdb"));
            const string sources = @"C:\|c:\Docs\Child|C:\DocsArchive|E:\Photos\Trip|E:\PhotosOld|C:\Music\Albums|Z:\Unmapped";
            using var dbClient = dbClientFactory.CreateDbClient();
            await dbClient.ExecuteNonQueryAsync(CommandType.Text,
                "INSERT INTO versiontable (versionID, versionSources) VALUES (1, @sources)", [("sources", sources)]);

            await BackupDatabaseImport.RemapVersionSourcesAsync(dbClientFactory,
                [new VersionDetails { Id = "1", Sources = sources }],
                [(@"C:\", @"D:\"), (@"C:\Docs", @"E:\Docs"), (@"E:\", @"F:\"), (@"E:\Photos", @"G:\Photos"),
                 (@"C:\Music", @"C:\Music")]);

            var remappedSources = await dbClient.ExecuteScalarAsync("SELECT versionSources FROM versiontable WHERE versionID = 1");
            Assert.That(remappedSources, Is.EqualTo(@"D:\|E:\Docs\Child|D:\DocsArchive|G:\Photos\Trip|F:\PhotosOld|C:\Music\Albums|Z:\Unmapped"));
        }
        finally
        {
            DbClientFactory.ClosePool();
            TempDirectory.DeleteBestEffort(directory);
        }
    }
}
