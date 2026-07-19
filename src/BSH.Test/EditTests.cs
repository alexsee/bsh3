// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types.Exceptions;
using Brightbits.BSH.Engine.Service.Jobs;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Providers.Storage;
using NUnit.Framework;

namespace BSH.Test;

public class EditTests
{
    private const string TestDbName = "testdb_edit.db";

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

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();

        if (File.Exists(TestDbName))
        {
            File.Delete(TestDbName);
        }
    }

    [Test]
    public async Task TestEditClearsEncryptionMetadataAfterSuccessfulRun()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = "existing-hash";

        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedEditableFileAsync(1, 1, 1, "plain-encrypted.txt", @"\docs\", 5, "");
        await SeedEditableFileAsync(2, 2, 1, "compressed-encrypted.txt", @"\docs\", 6, "long-name.bin");

        var storage = new RecordingEditStorage();
        var editJob = CreateEditJob(storage);
        editJob.Password = "test123";

        await editJob.EditAsync();

        Assert.That(editJob.FileErrorList, Is.Empty);
        Assert.That(configurationManager.Encrypt, Is.EqualTo(0));
        Assert.That(configurationManager.EncryptPassMD5, Is.Empty);
        Assert.That(storage.DecryptedFiles, Has.Count.EqualTo(2));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileType FROM fileversiontable WHERE fileversionID = 1")), Is.EqualTo(3));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileType FROM fileversiontable WHERE fileversionID = 2")), Is.EqualTo(1));
    }

    [Test]
    public async Task TestEditPreservesEncryptionMetadataWhenAnyFileFails()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = "existing-hash";

        const string versionDate = "01-01-2021 00-00-00";
        await SeedVersionAsync(1, versionDate);
        await SeedEditableFileAsync(1, 1, 1, "fails.txt", @"\docs\", 5, "");
        await SeedEditableFileAsync(2, 2, 1, "succeeds.txt", @"\docs\", 6, "");

        var failingRemoteFile = versionDate + @"\docs\" + "fails.txt";
        var storage = new RecordingEditStorage([failingRemoteFile]);
        var editJob = CreateEditJob(storage);
        editJob.Password = "test123";

        await editJob.EditAsync();

        Assert.That(editJob.FileErrorList.Count, Is.EqualTo(1));
        Assert.That(configurationManager.Encrypt, Is.EqualTo(1));
        Assert.That(configurationManager.EncryptPassMD5, Is.EqualTo("existing-hash"));
        Assert.That(storage.DecryptedFiles, Has.Count.EqualTo(1));
        Assert.That(storage.DecryptedFiles, Does.Not.Contain(failingRemoteFile));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileType FROM fileversiontable WHERE fileversionID = 1")), Is.EqualTo(5));
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT fileType FROM fileversiontable WHERE fileversionID = 2")), Is.EqualTo(1));
    }

    private EditJob CreateEditJob(IStorageProvider storage)
    {
        return new EditJob(
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

    private async Task SeedEditableFileAsync(
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

    private sealed class RecordingEditStorage : IStorageProvider
    {
        private readonly HashSet<string> filesToFail;

        public RecordingEditStorage(IEnumerable<string> filesToFail = null)
        {
            this.filesToFail = filesToFail == null ? [] : new HashSet<string>(filesToFail, StringComparer.OrdinalIgnoreCase);
        }

        public StorageProviderKind Kind => StorageProviderKind.LocalFileSystem;
        public List<string> DecryptedFiles { get; } = [];

        public Task<bool> CheckMedium(bool quickCheck = false) => Task.FromResult(true);
        public void Open() { }
        public bool CanWriteToStorage() => true;
        public bool CopyFileToStorage(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageCompressed(string localFile, string remoteFile) => true;
        public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password) => true;
        public bool CopyFileFromStorage(string localFile, string remoteFile) => true;
        public bool CopyFileFromStorageCompressed(string localFile, string remoteFile) => true;
        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password) => true;

        public bool DecryptOnStorage(string remoteFile, string password)
        {
            if (filesToFail.Contains(remoteFile))
            {
                throw new IOException("Decrypt failed.");
            }

            DecryptedFiles.Add(remoteFile);
            return true;
        }

        public bool DeleteFileFromStorage(string remoteFile) => true;
        public bool DeleteFileFromStorageCompressed(string remoteFile) => true;
        public bool DeleteFileFromStorageEncrypted(string remoteFile) => true;
        public bool DeleteDirectory(string remoteDirectory) => true;
        public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget) => true;
        public bool UploadDatabaseFile(string databaseFile) => true;
        public void UpdateStorageVersion(int versionId) { }
        public bool IsPathTooLong(string path, bool compression, bool encryption) => false;
        public long GetFreeSpace() => 42;
        public void Dispose() { }
    }
}
