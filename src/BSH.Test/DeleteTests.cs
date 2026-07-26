// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test;

public class DeleteTests
{
    private const string TestDbName = "testdb.db";

    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IQueryManager queryManager;
    private IVersionQueryRepository versionQueryRepository;
    private IBackupMutationRepository backupMutationRepository;

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
        backupMutationRepository = new BackupMutationRepository(dbClientFactory);

        var storageFactory = new StorageFactory(configurationManager);
        queryManager = new QueryManager(dbClientFactory, configurationManager, storageFactory);
    }

    [Test]
    public void TestFailMedium()
    {
        var storage = new RecordingDeleteStorage(failCheckMedium: true);
        var deleteJob = CreateDeleteJob(storage, "1");

        Assert.ThrowsAsync<DeviceNotReadyException>(async () => await deleteJob.DeleteAsync());
    }

    [Test]
    public async Task TestDeleteRoutesByFileTypeAndUpdatesMetadata()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");
        await SeedFileForVersionAsync(2, 2, 1, "compressed.txt", @"\docs\", 2, "");
        await SeedFileForVersionAsync(3, 3, 1, "encrypted-long.txt", @"\docs\", 6, "very-long-file-name");

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteJob(storage, "1");

        await deleteJob.DeleteAsync();

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedCompressed, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedEncrypted, Has.Count.EqualTo(1));

        Assert.That(storage.DeletedPlain[0], Is.EqualTo(Path.Combine(versionDate + @"\docs\", "plain.txt")));
        Assert.That(storage.DeletedCompressed[0], Is.EqualTo(Path.Combine(versionDate + @"\docs\", "compressed.txt")));
        Assert.That(storage.DeletedEncrypted[0], Is.EqualTo(Path.Combine(versionDate, "_LONGFILES_", "very-long-file-name")));

        var version = await queryManager.GetLastBackupAsync();
        Assert.That(version, Is.Null);
        Assert.That(configurationManager.OldBackupPrevent, Is.EqualTo("1"));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT versionStatus FROM versiontable WHERE versionID = 1")), Is.EqualTo(1));
    }

    [Test]
    public async Task TestDeleteCollectsFileErrorsAndContinues()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var storage = new RecordingDeleteStorage(throwOnDelete: true);
        var deleteJob = CreateDeleteJob(storage, "1");

        await deleteJob.DeleteAsync();

        Assert.That(deleteJob.FileErrorList.Count, Is.EqualTo(1));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT versionStatus FROM versiontable WHERE versionID = 1")), Is.EqualTo(1));
    }

    [Test]
    public async Task TestDeleteSingleUsesLongFileDirectoryForEncryptedLongFiles()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedFileForVersionAsync(1, 1, 1, "encrypted-long.txt", @"\docs\", 6, "very-long-file-name");

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteSingleJob(storage);

        await deleteJob.DeleteSingleAsync("encrypted-long.txt", @"\docs\");

        Assert.That(storage.DeletedEncrypted, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedEncrypted[0], Is.EqualTo(Path.Combine(versionDate, "_LONGFILES_", "very-long-file-name")));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable")), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteVersion_DoesNotRemovePackagesStillLinkedByLaterVersions()
    {
        const string versionDate1 = "01-01-2021 00-00-00";
        const string versionDate2 = "02-01-2021 00-00-00";

        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate1);
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 2, versionDate2);

        // Unchanged file shared across both versions (filePackage stays on version 1).
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "shared.txt", @"\docs\", 1, "");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 2)");

        // File unique to version 2.
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 2, 2, 2, "only-v2.txt", @"\docs\", 1, "");

        var storage = new StorageMock();
        var deleteJob = CreateDeleteJob(storage, "1");
        await deleteJob.DeleteAsync();

        Assert.That(storage.DeletedPlain, Is.Empty, "Shared package must remain for version 2 restores.");

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable WHERE fileversionID = 1")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE versionID = 1")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE versionID = 2")), Is.EqualTo(2));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT versionStatus FROM versiontable WHERE versionID = 1")), Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteVersion_RemovesOnlyPackagesUniqueToThatVersion()
    {
        const string versionDate1 = "01-01-2021 00-00-00";
        const string versionDate2 = "02-01-2021 00-00-00";

        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate1);
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 2, versionDate2);

        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "shared.txt", @"\docs\", 1, "");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 2)");
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 2, 2, 2, "only-v2.txt", @"\docs\", 1, "");

        var storage = new StorageMock();
        var deleteJob = CreateDeleteJob(storage, "2");
        await deleteJob.DeleteAsync();

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedPlain[0], Does.Contain("only-v2.txt"));
        Assert.That(storage.DeletedPlain[0], Does.Not.Contain("shared.txt"));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE versionID = 1")), Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteVersion_CleansOrphanedStorageDirectory()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate);
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "plain.txt", @"\docs\", 1, "");

        var storage = new StorageMock();
        await CreateDeleteJob(storage, "1").DeleteAsync();

        Assert.That(storage.DeletedDirectories, Does.Contain(versionDate));
        Assert.That(storage.UploadDatabaseFileCalls, Is.EqualTo(1));
    }

    [Test]
    public void DeleteSingle_FailsWhenMediumUnavailable()
    {
        var storage = new StorageMock(failCheckMedium: true);
        var deleteJob = CreateDeleteSingleJob(storage);

        Assert.ThrowsAsync<DeviceNotReadyException>(async () => await deleteJob.DeleteSingleAsync("a.txt", @"\docs\"));
    }

    [Test]
    public async Task DeleteSingle_PathFilterDeletesAllFilesUnderPath()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate);
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "a.txt", @"\docs\", 1, "");
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 2, 2, 1, "b.txt", @"\docs\", 2, "");
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 3, 3, 1, "keep.txt", @"\other\", 1, "");

        var storage = new StorageMock();
        await CreateDeleteSingleJob(storage).DeleteSingleAsync(fileFilter: "", pathFilter: @"\docs\%");

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedCompressed, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedPlain[0], Does.Contain("a.txt"));
        Assert.That(storage.DeletedCompressed[0], Does.Contain("b.txt"));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable WHERE fileName = 'keep.txt'")), Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteSingle_PlainAndCompressedRouting()
    {
        const string versionDate = "01-01-2021 00-00-00";
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate);
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "plain.txt", @"\docs\", 1, "");
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 2, 2, 1, "zip.txt", @"\docs\", 2, "");

        var storage = new StorageMock();
        await CreateDeleteSingleJob(storage).DeleteSingleAsync("plain.txt", @"\docs\");

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedCompressed, Is.Empty);

        await CreateDeleteSingleJob(storage).DeleteSingleAsync("zip.txt", @"\docs\");
        Assert.That(storage.DeletedCompressed, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task DeleteVersion_RemovesFolderMetadataForDeletedVersionOnly()
    {
        const string versionDate1 = "01-01-2021 00-00-00";
        const string versionDate2 = "02-01-2021 00-00-00";

        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 1, versionDate1);
        await JobSeedHelper.SeedVersionAsync(dbClientFactory, 2, versionDate2);
        await JobSeedHelper.SeedFileForVersionAsync(dbClientFactory, 1, 1, 1, "a.txt", @"\docs\", 1, "");
        await JobSeedHelper.SeedEmptyFolderAsync(dbClientFactory, 1, 1, @"\docs\empty\");
        await JobSeedHelper.SeedEmptyFolderAsync(dbClientFactory, 1, 2, @"\docs\empty\");

        var storage = new StorageMock();
        await CreateDeleteJob(storage, "1").DeleteAsync();

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM folderlink WHERE versionid = 1")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM folderlink WHERE versionid = 2")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM foldertable")), Is.EqualTo(1));
    }

    private DeleteJob CreateDeleteJob(IStorageProvider storage, string version)
    {
        return new DeleteJob(
            storage,
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository)
        {
            Version = version
        };
    }

    private DeleteSingleJob CreateDeleteSingleJob(IStorageProvider storage)
    {
        return new DeleteSingleJob(
            storage,
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository);
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

    private sealed class RecordingDeleteStorage : IStorageProvider
    {
        private readonly bool failCheckMedium;
        private readonly bool throwOnDelete;

        public RecordingDeleteStorage(bool failCheckMedium = false, bool throwOnDelete = false)
        {
            this.failCheckMedium = failCheckMedium;
            this.throwOnDelete = throwOnDelete;
        }

        public StorageProviderKind Kind => StorageProviderKind.LocalFileSystem;
        public List<string> DeletedPlain { get; } = [];
        public List<string> DeletedCompressed { get; } = [];
        public List<string> DeletedEncrypted { get; } = [];

        public Task<bool> CheckMedium(bool quickCheck = false) => Task.FromResult(!failCheckMedium);
        public void Open() { }
        public bool CanWriteToStorage() => true;
        public bool CopyFileToStorage(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageCompressed(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password) => true;
        public bool CopyFileFromStorage(string localFile, string remoteFile) => true;
        public bool CopyFileFromStorageCompressed(string localFile, string remoteFile) => true;
        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password) => true;
        public bool DecryptOnStorage(string remoteFile, string password) => true;

        public bool DeleteFileFromStorage(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedPlain.Add(remoteFile);
            return true;
        }

        public bool DeleteFileFromStorageCompressed(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedCompressed.Add(remoteFile);
            return true;
        }

        public bool DeleteFileFromStorageEncrypted(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedEncrypted.Add(remoteFile);
            return true;
        }

        public bool DeleteDirectory(string remoteDirectory) => true;
        public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget) => true;
        public bool UploadDatabaseFile(string databaseFile) => true;
        public void UpdateStorageVersion(int versionId) { }
        public bool IsPathTooLong(string path, bool compression, bool encryption) => false;
        public long GetFreeSpace() => 42;
        public void Dispose() { }
    }
}
