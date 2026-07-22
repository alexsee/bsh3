// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public async Task TestDeleteSingleFromSelectedVersionsKeepsSharedStorageWhenOtherLinksRemain()
    {
        // Same content version linked into backups 1, 2, and 3 (unchanged file).
        await SeedVersionAsync(1, "01-01-2021 00-00-00");
        await SeedVersionAsync(2, "02-01-2021 00-00-00");
        await SeedVersionAsync(3, "03-01-2021 00-00-00");
        await SeedSharedFileAsync(1, 1, [1, 2, 3], "report.txt", @"\docs\", 1, "");

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteSingleJob(storage);

        await deleteJob.DeleteSingleAsync("report.txt", @"\docs\", [2, 3]);

        Assert.That(storage.DeletedPlain, Is.Empty);

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE fileversionID = 1")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT versionID FROM filelink WHERE fileversionID = 1")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable")), Is.EqualTo(1));
    }

    [Test]
    public async Task TestDeleteSingleFromSelectedVersionsDeletesStorageWhenLastLinksRemoved()
    {
        await SeedVersionAsync(1, "01-01-2021 00-00-00");
        await SeedVersionAsync(2, "02-01-2021 00-00-00");
        await SeedSharedFileAsync(1, 1, [1, 2], "report.txt", @"\docs\", 1, "");

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteSingleJob(storage);

        await deleteJob.DeleteSingleAsync("report.txt", @"\docs\", [1, 2]);

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedPlain[0], Is.EqualTo(Path.Combine("01-01-2021 00-00-00" + @"\docs\", "report.txt")));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable")), Is.EqualTo(0));
    }

    [Test]
    public async Task TestDeleteSingleFromSelectedVersionsOnlyRemovesUniqueContentVersions()
    {
        // Version 1 has content A; versions 2+3 share content B. Purging 2+3 deletes only B.
        await SeedVersionAsync(1, "01-01-2021 00-00-00");
        await SeedVersionAsync(2, "02-01-2021 00-00-00");
        await SeedVersionAsync(3, "03-01-2021 00-00-00");
        await SeedFileForVersionAsync(1, 1, 1, "report.txt", @"\docs\", 1, "");
        await SeedFileVersionLinkAsync(1, 2, 2, "");
        await SeedFileLinkAsync(2, 3);

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteSingleJob(storage);

        await deleteJob.DeleteSingleAsync("report.txt", @"\docs\", [2, 3]);

        Assert.That(storage.DeletedPlain, Has.Count.EqualTo(1));
        Assert.That(storage.DeletedPlain[0], Is.EqualTo(Path.Combine("02-01-2021 00-00-00" + @"\docs\", "report.txt")));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileversionID FROM filelink")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(1));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filetable")), Is.EqualTo(1));
    }

    [Test]
    public async Task TestDeleteSingleFolderFilterRespectsSelectedVersions()
    {
        await SeedVersionAsync(1, "01-01-2021 00-00-00");
        await SeedVersionAsync(2, "02-01-2021 00-00-00");
        await SeedSharedFileAsync(1, 1, [1, 2], "a.txt", @"\docs\", 1, "");
        await SeedSharedFileAsync(2, 2, [1, 2], "b.txt", @"\docs\sub\", 1, "");

        var storage = new RecordingDeleteStorage();
        var deleteJob = CreateDeleteSingleJob(storage);

        await deleteJob.DeleteSingleAsync(string.Empty, @"\docs\%", [2]);

        Assert.That(storage.DeletedPlain, Is.Empty);

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE versionID = 2")), Is.EqualTo(0));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM filelink WHERE versionID = 1")), Is.EqualTo(2));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(2));
    }

    [Test]
    public void TestVersionSelectionSelectsLastNAndSinceDate()
    {
        var versions = new List<VersionDetails>
        {
            new() { Id = "1", CreationDate = new DateTime(2021, 1, 1) },
            new() { Id = "2", CreationDate = new DateTime(2021, 1, 10) },
            new() { Id = "3", CreationDate = new DateTime(2021, 1, 20) },
        };

        Assert.That(VersionSelection.SelectLastN(versions, 2), Is.EqualTo(new[] { 3, 2 }));
        Assert.That(VersionSelection.SelectSinceDate(versions, new DateTime(2021, 1, 10)), Is.EqualTo(new[] { 2, 3 }));
        Assert.That(VersionSelection.SelectLastN(versions, 0), Is.Empty);
        Assert.That(VersionSelection.SelectLastN(null!, 2), Is.Empty);
    }

    [Test]
    public void TestDeleteSingleScopeResolvesAllLastNLastDaysAndSelected()
    {
        var versions = new List<VersionDetails>
        {
            new() { Id = "1", CreationDate = new DateTime(2021, 1, 1) },
            new() { Id = "2", CreationDate = new DateTime(2021, 1, 10) },
            new() { Id = "3", CreationDate = new DateTime(2021, 1, 20) },
        };
        var now = new DateTime(2021, 1, 20, 12, 0, 0);

        var all = DeleteSingleScope.Resolve(DeleteSingleScopeMode.AllVersions, versions, 2, 5, Array.Empty<string>(), now);
        Assert.That(all.DeleteFromAllVersions, Is.True);
        Assert.That(all.HasTargetVersions, Is.True);

        var lastN = DeleteSingleScope.Resolve(DeleteSingleScopeMode.LastN, versions, 2, 5, Array.Empty<string>(), now);
        Assert.That(lastN.VersionIds, Is.EqualTo(new[] { 3, 2 }));

        var lastDays = DeleteSingleScope.Resolve(DeleteSingleScopeMode.LastDays, versions, 2, 11, Array.Empty<string>(), now);
        Assert.That(lastDays.VersionIds, Is.EqualTo(new[] { 2, 3 }));

        var selected = DeleteSingleScope.Resolve(DeleteSingleScopeMode.SelectedVersions, versions, 2, 5, ["1", "bogus", "3"], now);
        Assert.That(selected.VersionIds, Is.EqualTo(new[] { 1, 3 }));

        var none = DeleteSingleScope.Resolve(DeleteSingleScopeMode.SelectedVersions, versions, 2, 5, Array.Empty<string>(), now);
        Assert.That(none.HasTargetVersions, Is.False);
    }

    [Test]
    public async Task TestDeleteSingleCandidateVersionsPrefersAvailableVersionsForFiles()
    {
        var allVersions = new List<VersionDetails>
        {
            new() { Id = "1", CreationDate = new DateTime(2021, 1, 1) },
            new() { Id = "2", CreationDate = new DateTime(2021, 1, 10) },
        };
        var available = new List<VersionDetails>
        {
            new() { Id = "2", CreationDate = new DateTime(2021, 1, 10) },
        };
        var queryManager = new CandidateQueryManager(new FileDetails { AvailableVersions = available });

        var forFile = await DeleteSingleCandidateVersions.ResolveAsync(queryManager, allVersions, "2", "a.txt", @"\docs\", isFile: true);
        Assert.That(forFile.Select(v => v.Id), Is.EqualTo(new[] { "2" }));

        var forFolder = await DeleteSingleCandidateVersions.ResolveAsync(queryManager, allVersions, "2", "", @"\docs\", isFile: false);
        Assert.That(forFolder.Select(v => v.Id), Is.EqualTo(new[] { "1", "2" }));
    }

    private sealed class CandidateQueryManager : IQueryManager
    {
        private readonly FileDetails details;

        public CandidateQueryManager(FileDetails details)
        {
            this.details = details;
        }

        public Task<FileDetails> GetFileDetailsAsync(string version, string fileName, string filePath) => Task.FromResult(details);
        public Task<string> GetBackVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetBackVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public string GetFileNameFromDrive(FileTableRow file) => file.FileName;
        public Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password) => Task.FromResult((fileName, false));
        public Task<List<FileTableRow>> GetFilesByVersionAsync(string version, string path) => Task.FromResult(new List<FileTableRow>());
        public Task<List<string>> GetFolderListAsync(string version, string path) => Task.FromResult(new List<string>());
        public Task<string> GetFullRestoreFolderAsync(string folder, string version) => Task.FromResult(folder);
        public Task<VersionDetails> GetLastBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<string> GetLocalizedPathAsync(string path) => Task.FromResult(path);
        public Task<string> GetNextVersionWhereFileAsync(string startVersion, string searchString) => Task.FromResult<string>(null);
        public Task<string> GetNextVersionWhereFilesInFolderAsync(string startVersion, string path) => Task.FromResult<string>(null);
        public Task<int> GetNumberOfVersionsAsync() => Task.FromResult(0);
        public Task<int> GetNumberOfFilesAsync() => Task.FromResult(0);
        public Task<double> GetTotalFileSizeAsync() => Task.FromResult(0d);
        public Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult<VersionDetails>(null);
        public Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult<VersionDetails>(null);
        public List<VersionDetails> GetVersions(bool desc = true) => [];
        public Task<List<FileTableRow>> GetVersionsByFileAsync(string fileName, string filePath) => Task.FromResult(new List<FileTableRow>());
        public Task<List<FileTableRow>> SearchFilesByVersionAsync(string version, string searchTerm, int limit = 500) => Task.FromResult(new List<FileTableRow>());
        public Task<bool> HasChangesOrNewAsync(string path, string versionId) => Task.FromResult(false);
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
        await SeedFileVersionLinkAsync(fileId, fileVersionId, versionId, longFileName, fileType);
    }

    private async Task SeedSharedFileAsync(
        int fileId,
        int fileVersionId,
        IReadOnlyList<int> versionIds,
        string fileName,
        string filePath,
        int fileType,
        string longFileName)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO filetable (fileID, fileName, filePath) VALUES ({fileId}, '{fileName}', '{filePath}')");
        await SeedFileVersionLinkAsync(fileId, fileVersionId, versionIds[0], longFileName, fileType);
        for (var i = 1; i < versionIds.Count; i++)
        {
            await SeedFileLinkAsync(fileVersionId, versionIds[i]);
        }
    }

    private async Task SeedFileVersionLinkAsync(
        int fileId,
        int fileVersionId,
        int packageVersionId,
        string longFileName,
        int fileType = 1)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) VALUES " +
            $"({fileVersionId}, 1, {fileType}, '', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 123, {packageVersionId}, {fileId}, '{longFileName}')");
        await SeedFileLinkAsync(fileVersionId, packageVersionId);
    }

    private async Task SeedFileLinkAsync(int fileVersionId, int versionId)
    {
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
