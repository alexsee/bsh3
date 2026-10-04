// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.Test.Fakes;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class QueryManagerVersionStatisticsTests
{
    private string databaseFile = null!;
    private DbClientFactory factory = null!;

    [SetUp]
    public async Task Setup()
    {
        databaseFile = Path.Combine(Path.GetTempPath(), $"bsh-version-statistics-{Guid.NewGuid():N}.db");
        factory = new DbClientFactory();
        await factory.InitializeAsync(databaseFile);
        await factory.ExecuteNonQueryAsync("""
            INSERT INTO versiontable (versionID, versionStatus) VALUES (1, 0), (2, 1), (3, 0), (4, 0), (5, 0);
            INSERT INTO filetable (fileID, fileName, filePath) VALUES
                (1, 'unchanged.txt', '\source\'), (2, 'changed.txt', '\source\'),
                (3, 'deleted.txt', '\source\'), (4, 'copied-again.txt', '\source\'), (5, 'added.txt', '\source\');
            INSERT INTO fileversiontable (fileversionID, fileID, fileSize, fileDateModified) VALUES
                (11, 1, 10, '2027-05-30T15:00:00Z'), (12, 2, 20, '2027-05-30T15:00:00Z'),
                (13, 3, 30, '2027-05-30T15:00:00Z'), (14, 4, 40, '2027-05-30T15:00:00Z'),
                (22, 2, 25, '2027-05-30T15:00:00Z'), (24, 4, 40, '2027-05-30T15:00:00Z'),
                (25, 5, 50, '2027-05-30T15:00:00Z'), (31, 1, 10, '2027-05-30T15:00:00Z'),
                (32, 2, 25, '2027-05-30T15:00:00Z'), (34, 4, 40, '2027-05-30T15:00:00Z'),
                (35, 5, 50, '2027-05-30T17:00:00Z');
            INSERT INTO filelink (fileversionID, versionID) VALUES
                (11, 1), (12, 1), (13, 1), (14, 1),
                (25, 2),
                (11, 3), (22, 3), (24, 3), (25, 3),
                (31, 4), (32, 4), (34, 4), (35, 4);
            """);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        File.Delete(databaseFile);
    }

    [TestCase("1", 4, 0, 0, TestName = "FirstBackupCountsAllFilesAsAdded")]
    [TestCase("3", 1, 1, 1, TestName = "SnapshotChangesSkipIncompleteBackupAndIgnoreUnchangedCopies")]
    [TestCase("4", 0, 1, 0, TestName = "FullBackupCountsTimestampChangesButIgnoresUnchangedCopies")]
    [TestCase("5", 0, 0, 4, TestName = "EmptyBackupCountsPreviousFilesAsDeleted")]
    public async Task ChangesMatchPreviousAvailableSnapshot(string version, long added, long modified, long deleted)
    {
        var queryManager = new QueryManager(factory, new FakeConfigurationManager(), new StorageFactoryMock());

        var changes = await queryManager.GetVersionChangeStatisticsAsync(version);

        Assert.That(changes, Is.EqualTo(new VersionChangeStatistics(added, modified, deleted)));
    }
}
