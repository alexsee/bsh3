// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
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
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services.FileCollector;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test.Integration;

/// <summary>
/// Real filesystem lifecycle coverage for backup → restore → delete → edit via
/// <see cref="FileSystemStorage"/>. Treats the engine jobs as a black box.
/// </summary>
[Category("Integration")]
public class FileSystemEngineLifecycleTests
{
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

        rootDir = Path.Combine(Path.GetTempPath(), "BSH.Test", "lifecycle-" + Guid.NewGuid().ToString("N"));
        sourceDir = Path.Combine(rootDir, "source");
        backupDir = Path.Combine(rootDir, "backup");
        restoreDir = Path.Combine(rootDir, "restore");
        dbPath = Path.Combine(rootDir, "testdb_lifecycle.db");

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
    public async Task DeleteOlderVersion_RemainingVersionStillRestores()
    {
        WriteSourceFile("doc.txt", "version-one");
        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();

        await Task.Delay(1100);
        File.WriteAllText(Path.Combine(sourceDir, "doc.txt"), "version-two");
        File.SetLastWriteTimeUtc(Path.Combine(sourceDir, "doc.txt"), DateTime.UtcNow.AddMinutes(1));
        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();
        Assert.That(version2.Id, Is.Not.EqualTo(version1.Id));

        await RunDeleteAsync(version1.Id);

        var dest = Path.Combine(restoreDir, "after-delete");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(int.Parse(version2.Id), dest);

        Assert.That(File.ReadAllText(Path.Combine(dest, "doc.txt")), Is.EqualTo("version-two"));
        Assert.That(await queryManager.GetLastBackupAsync(), Is.Not.Null);
    }

    [Test]
    public async Task DeleteNewerVersion_OlderSharedContentStillRestores()
    {
        WriteSourceFile("stable.txt", "unchanged-content");
        WriteSourceFile("gone-later.txt", "only-in-v1");
        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();

        await Task.Delay(1100);
        File.Delete(Path.Combine(sourceDir, "gone-later.txt"));
        WriteSourceFile("new-in-v2.txt", "only-in-v2");
        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();

        await RunDeleteAsync(version2.Id);

        var dest = Path.Combine(restoreDir, "v1-after-v2-delete");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(int.Parse(version1.Id), dest);

        Assert.That(File.ReadAllText(Path.Combine(dest, "stable.txt")), Is.EqualTo("unchanged-content"));
        Assert.That(File.ReadAllText(Path.Combine(dest, "gone-later.txt")), Is.EqualTo("only-in-v1"));
        Assert.That(File.Exists(Path.Combine(dest, "new-in-v2.txt")), Is.False);
    }

    [Test]
    public async Task SharedUnchangedFile_SurvivesDeleteOfFirstVersion()
    {
        WriteSourceFile("shared.txt", "shared-bytes");
        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();

        await Task.Delay(1100);
        WriteSourceFile("extra.txt", "second-version-only");
        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();

        await RunDeleteAsync(version1.Id);

        var dest = Path.Combine(restoreDir, "shared");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(int.Parse(version2.Id), dest);

        Assert.That(File.ReadAllText(Path.Combine(dest, "shared.txt")), Is.EqualTo("shared-bytes"));
        Assert.That(File.ReadAllText(Path.Combine(dest, "extra.txt")), Is.EqualTo("second-version-only"));
    }

    [Test]
    public async Task EditDecrypt_ThenRestoreWithoutPassword()
    {
        configurationManager.Encrypt = 1;
        configurationManager.EncryptPassMD5 = Hash.GetMD5Hash(Password);
        WriteSourceFile("secret.txt", "encrypted-payload");

        await RunBackupAsync(password: Password);

        var editJob = new EditJob(
            CreateStorage(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository)
        {
            Password = Password,
        };
        await editJob.EditAsync();
        Assert.That(editJob.FileErrorList, Is.Empty);
        Assert.That(configurationManager.Encrypt, Is.EqualTo(0));

        var dest = Path.Combine(restoreDir, "decrypted");
        Directory.CreateDirectory(dest);
        var restoreJob = CreateRestoreJob(CreateStorage(), version: 1, destination: dest, password: null);
        await restoreJob.RestoreAsync(CancellationToken.None);

        Assert.That(restoreJob.FileErrorList, Is.Empty);
        Assert.That(File.ReadAllText(Path.Combine(dest, "secret.txt")), Is.EqualTo("encrypted-payload"));
    }

    [Test]
    public async Task EmptyFolder_IsRestored()
    {
        var emptyDir = Path.Combine(sourceDir, "hollow");
        Directory.CreateDirectory(emptyDir);
        WriteSourceFile("anchor.txt", "keep");

        await RunBackupAsync();

        var dest = Path.Combine(restoreDir, "with-empty");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(1, dest);

        Assert.That(Directory.Exists(Path.Combine(dest, "hollow")), Is.True);
        Assert.That(File.ReadAllText(Path.Combine(dest, "anchor.txt")), Is.EqualTo("keep"));
    }

    [Test]
    public async Task NestedAndUnicodePaths_RoundTrip()
    {
        var nested = Path.Combine(sourceDir, "äöü", "深层");
        Directory.CreateDirectory(nested);
        File.WriteAllText(Path.Combine(nested, "файл.txt"), "unicode-content");

        await RunBackupAsync();

        var dest = Path.Combine(restoreDir, "unicode");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(1, dest);

        var restored = Directory.GetFiles(dest, "файл.txt", SearchOption.AllDirectories).SingleOrDefault();
        Assert.That(restored, Is.Not.Null);
        Assert.That(File.ReadAllText(restored), Is.EqualTo("unicode-content"));
    }

    [Test]
    public async Task DeleteSingleFile_RemovesItFromAllVersions()
    {
        WriteSourceFile("keep.txt", "keep-me");
        WriteSourceFile("drop.txt", "drop-me");
        await RunBackupAsync();

        await Task.Delay(1100);
        File.WriteAllText(Path.Combine(sourceDir, "drop.txt"), "drop-me-v2");
        File.SetLastWriteTimeUtc(Path.Combine(sourceDir, "drop.txt"), DateTime.UtcNow.AddMinutes(1));
        await RunBackupAsync();

        var sourceLeaf = Path.GetFileName(sourceDir);
        var deleteJob = new DeleteSingleJob(
            CreateStorage(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository);
        await deleteJob.DeleteSingleAsync("drop.txt", $@"\{sourceLeaf}\");

        Assert.That(deleteJob.FileErrorList, Is.Empty);

        var dest = Path.Combine(restoreDir, "after-single-delete");
        Directory.CreateDirectory(dest);
        var version2 = await queryManager.GetLastBackupAsync();
        await RunRestoreAsync(int.Parse(version2.Id), dest);

        Assert.That(File.Exists(Path.Combine(dest, "keep.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(dest, "drop.txt")), Is.False);
    }

    [Test]
    public async Task CompressedBackup_DeleteVersion_AndRestoreSibling()
    {
        configurationManager.Compression = 1;
        WriteSourceFile("a.txt", "alpha");
        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();

        await Task.Delay(1100);
        WriteSourceFile("b.txt", "beta");
        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();

        await RunDeleteAsync(version1.Id);

        var dest = Path.Combine(restoreDir, "compressed");
        Directory.CreateDirectory(dest);
        await RunRestoreAsync(int.Parse(version2.Id), dest);

        Assert.That(File.ReadAllText(Path.Combine(dest, "a.txt")), Is.EqualTo("alpha"));
        Assert.That(File.ReadAllText(Path.Combine(dest, "b.txt")), Is.EqualTo("beta"));
    }

    [Test]
    public async Task CancelledBackup_LeavesNoVersion()
    {
        WriteSourceFile("one.txt", "content");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var backupJob = CreateBackupJob(CreateStorage());
        await backupJob.BackupAsync(cts.Token);

        Assert.That(await queryManager.GetLastBackupAsync(), Is.Null);
    }

    [Test]
    public async Task FullBackupFlag_CopiesUnchangedFileAgain()
    {
        WriteSourceFile("same.txt", "payload");
        await RunBackupAsync();

        await Task.Delay(1100);
        var before = CountFilesRecursive(backupDir);

        var backupJob = CreateBackupJob(CreateStorage());
        backupJob.FullBackup = true;
        await backupJob.BackupAsync(CancellationToken.None);
        Assert.That(backupJob.FileErrorList, Is.Empty);

        var after = CountFilesRecursive(backupDir);
        Assert.That(after, Is.GreaterThan(before));

        using var dbClient = dbClientFactory.CreateDbClient();
        Assert.That(Convert.ToInt32(await dbClient.ExecuteScalarAsync("SELECT COUNT(*) FROM fileversiontable")), Is.EqualTo(2));
    }

    [Test]
    public async Task AddedAndRemovedFiles_AcrossIncrementals()
    {
        WriteSourceFile("stay.txt", "stay");
        WriteSourceFile("leave.txt", "leave");
        await RunBackupAsync();
        var version1 = await queryManager.GetLastBackupAsync();

        await Task.Delay(1100);
        File.Delete(Path.Combine(sourceDir, "leave.txt"));
        WriteSourceFile("arrive.txt", "arrive");
        await RunBackupAsync();
        var version2 = await queryManager.GetLastBackupAsync();

        var dest1 = Path.Combine(restoreDir, "v1");
        var dest2 = Path.Combine(restoreDir, "v2");
        Directory.CreateDirectory(dest1);
        Directory.CreateDirectory(dest2);

        await RunRestoreAsync(int.Parse(version1.Id), dest1);
        await RunRestoreAsync(int.Parse(version2.Id), dest2);

        Assert.That(File.Exists(Path.Combine(dest1, "leave.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(dest1, "arrive.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(dest2, "leave.txt")), Is.False);
        Assert.That(File.Exists(Path.Combine(dest2, "arrive.txt")), Is.True);
        Assert.That(File.ReadAllText(Path.Combine(dest2, "stay.txt")), Is.EqualTo("stay"));
    }

    private async Task RunBackupAsync(string password = null)
    {
        var backupJob = CreateBackupJob(CreateStorage());
        backupJob.Password = password;
        await backupJob.BackupAsync(CancellationToken.None);
        Assert.That(backupJob.FileErrorList, Is.Empty, "Backup reported file errors.");
        Assert.That(await queryManager.GetLastBackupAsync(), Is.Not.Null, "Backup did not create a version.");
    }

    private async Task RunRestoreAsync(int version, string destination)
    {
        var restoreJob = CreateRestoreJob(CreateStorage(), version, destination);
        await restoreJob.RestoreAsync(CancellationToken.None);
        Assert.That(restoreJob.FileErrorList, Is.Empty, "Restore reported file errors.");
    }

    private async Task RunDeleteAsync(string versionId)
    {
        var deleteJob = new DeleteJob(
            CreateStorage(),
            dbClientFactory,
            queryManager,
            configurationManager,
            versionQueryRepository,
            backupMutationRepository)
        {
            Version = versionId,
        };
        await deleteJob.DeleteAsync();
        Assert.That(deleteJob.FileErrorList, Is.Empty, "Delete reported file errors.");
    }

    private BackupJob CreateBackupJob(IStorageProvider storage)
    {
        return new BackupJob(
            storage,
            dbClientFactory,
            queryManager,
            configurationManager,
            fileCollectorServiceFactory,
            vssClient,
            versionQueryRepository,
            backupMutationRepository)
        {
            SourceFolder = sourceDir,
            Title = "Lifecycle",
            Description = "",
        };
    }

    private RestoreJob CreateRestoreJob(
        IStorageProvider storage,
        int version,
        string destination,
        string password = null)
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
            Destination = destination,
            FileOverwrite = FileOverwrite.Overwrite,
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

    private static int CountFilesRecursive(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return 0;
        }

        return Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length;
    }
}
