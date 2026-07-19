// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Providers.Storage;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types.Exceptions;
using NUnit.Framework;

namespace BSH.Test.Providers;

public class FileSystemStorageTests
{
    private string tempRoot;
    private string backupFolder;
    private string sourceFile;
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;

    [SetUp]
    public async Task SetUp()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), "bsh-storage-tests-" + Guid.NewGuid().ToString("N"));
        backupFolder = Path.Combine(tempRoot, "backup");
        Directory.CreateDirectory(backupFolder);

        sourceFile = Path.Combine(tempRoot, "source.txt");
        await File.WriteAllTextAsync(sourceFile, "hello-storage");

        if (File.Exists("testdb_storage.db"))
        {
            DbClientFactory.ClosePool();
            File.Delete("testdb_storage.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_storage.db"));

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        configurationManager.BackupFolder = backupFolder;
        configurationManager.OldBackupPrevent = "1";
        configurationManager.MediaVolumeSerial = "";
        configurationManager.UNCUsername = "";
        configurationManager.UNCPassword = "";
    }

    [TearDown]
    public void TearDown()
    {
        DbClientFactory.ClosePool();
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }

        if (File.Exists("testdb_storage.db"))
        {
            File.Delete("testdb_storage.db");
        }
    }

    [Test]
    public void Kind_IsLocalFileSystem()
    {
        using var storage = new FileSystemStorage(configurationManager);
        Assert.That(storage.Kind, Is.EqualTo(StorageProviderKind.LocalFileSystem));
    }

    [Test]
    public async Task CheckMedium_ReturnsTrue_ForWritableLocalFolder()
    {
        using var storage = new FileSystemStorage(configurationManager);
        Assert.That(await storage.CheckMedium(), Is.True);
        Assert.That(storage.CanWriteToStorage(), Is.True);
    }

    [Test]
    public async Task CheckMedium_Throws_WhenVersionFileDoesNotMatch()
    {
        await File.WriteAllTextAsync(Path.Combine(backupFolder, "backup.bshv"), "99");
        using var storage = new FileSystemStorage(configurationManager);
        Assert.ThrowsAsync<DeviceContainsWrongStateException>(async () => await storage.CheckMedium());
    }

    [Test]
    public void Open_CreatesBackupFolder_WhenMissing()
    {
        configurationManager.BackupFolder = Path.Combine(tempRoot, "missing-backup");
        using var storage = new FileSystemStorage(configurationManager);
        storage.Open();
        Assert.That(Directory.Exists(configurationManager.BackupFolder), Is.True);
    }

    [Test]
    public void CopyRoundTrip_PlainCompressedAndEncrypted_Works()
    {
        using var storage = new FileSystemStorage(configurationManager);
        storage.Open();

        Assert.That(storage.CopyFileToStorage(sourceFile, @"folder\plain.txt"), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "folder", "plain.txt")), Is.True);

        var restoredPlain = Path.Combine(tempRoot, "restored-plain.txt");
        Assert.That(storage.CopyFileFromStorage(restoredPlain, @"folder\plain.txt"), Is.True);
        Assert.That(File.ReadAllText(restoredPlain), Is.EqualTo("hello-storage"));

        Assert.That(storage.CopyFileToStorageCompressed(sourceFile, @"folder\zipped.txt"), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "folder", "zipped.txt.zip")), Is.True);

        // CopyFileFromStorageCompressed looks up the zip entry by the local file's name.
        var restoredZip = Path.Combine(tempRoot, "source.txt");
        File.Delete(restoredZip);
        Assert.That(storage.CopyFileFromStorageCompressed(restoredZip, @"folder\zipped.txt"), Is.True);
        Assert.That(File.ReadAllText(restoredZip), Is.EqualTo("hello-storage"));
        File.WriteAllText(sourceFile, "hello-storage");

        Assert.That(storage.CopyFileToStorageEncrypted(sourceFile, @"folder\secret.txt", "pass"), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "folder", "secret.txt.enc")), Is.True);

        var restoredEnc = Path.Combine(tempRoot, "restored-enc.txt");
        Assert.That(storage.CopyFileFromStorageEncrypted(restoredEnc, @"folder\secret.txt", "pass"), Is.True);
        Assert.That(File.ReadAllText(restoredEnc), Is.EqualTo("hello-storage"));
    }

    [Test]
    public void DeleteAndRenameOperations_Work()
    {
        using var storage = new FileSystemStorage(configurationManager);
        storage.Open();

        storage.CopyFileToStorage(sourceFile, @"a\file.txt");
        storage.CopyFileToStorageCompressed(sourceFile, @"a\file2.txt");
        storage.CopyFileToStorageEncrypted(sourceFile, @"a\file3.txt", "pass");

        Assert.That(storage.DeleteFileFromStorage(@"a\file.txt"), Is.True);
        Assert.That(storage.DeleteFileFromStorageCompressed(@"a\file2.txt"), Is.True);
        Assert.That(storage.DeleteFileFromStorageEncrypted(@"a\file3.txt"), Is.True);

        Directory.CreateDirectory(Path.Combine(backupFolder, "old-dir"));
        Assert.That(storage.RenameDirectory("old-dir", "new-dir"), Is.True);
        Assert.That(Directory.Exists(Path.Combine(backupFolder, "new-dir")), Is.True);
        Assert.That(storage.DeleteDirectory("new-dir"), Is.True);
    }

    [Test]
    public void UploadDatabaseAndUpdateVersion_WriteExpectedFiles()
    {
        using var storage = new FileSystemStorage(configurationManager);
        storage.Open();

        var dbFile = Path.Combine(tempRoot, "local.bshdb");
        File.WriteAllText(dbFile, "db");
        Assert.That(storage.UploadDatabaseFile(dbFile), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "backup.bshdb")), Is.True);

        storage.UpdateStorageVersion(7);
        Assert.That(File.ReadAllText(Path.Combine(backupFolder, "backup.bshv")), Is.EqualTo("7"));
        Assert.That(storage.GetFreeSpace(), Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void IsPathTooLong_DetectsExcessivePaths()
    {
        using var storage = new FileSystemStorage(configurationManager);
        var longName = new string('a', 300);
        Assert.That(storage.IsPathTooLong(longName, compression: false, encryption: false), Is.True);
        Assert.That(storage.IsPathTooLong("short.txt", compression: false, encryption: false), Is.False);
    }

    [Test]
    public void DecryptOnStorage_RewritesEncryptedFile()
    {
        using var storage = new FileSystemStorage(configurationManager);
        storage.Open();
        storage.CopyFileToStorageEncrypted(sourceFile, @"enc\doc.txt", "secret");

        Assert.That(storage.DecryptOnStorage(@"enc\doc.txt", "secret"), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "enc", "doc.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(backupFolder, "enc", "doc.txt.enc")), Is.False);
    }

    [Test]
    public void StorageFactory_ReturnsFileSystemStorage_ForLocalMedia()
    {
        configurationManager.MediumType = Brightbits.BSH.Engine.Types.MediaType.LocalDevice;
        var factory = new StorageFactory(configurationManager);
        using var storage = factory.GetCurrentStorageProvider();
        Assert.That(storage, Is.TypeOf<FileSystemStorage>());
    }

    [Test]
    public void StorageFactory_ReturnsFtpStorage_ForFtpMedia()
    {
        configurationManager.MediumType = Brightbits.BSH.Engine.Types.MediaType.FileTransferServer;
        configurationManager.FtpHost = "localhost";
        configurationManager.FtpPort = "21";
        configurationManager.FtpUser = "user";
        configurationManager.FtpPass = "pass";
        configurationManager.FtpFolder = "/backup";
        configurationManager.FtpCoding = "UTF-8";
        configurationManager.FtpEncryptionMode = "0";
        configurationManager.OldBackupPrevent = "1";

        var factory = new StorageFactory(configurationManager);
        using var storage = factory.GetCurrentStorageProvider();
        Assert.That(storage, Is.TypeOf<FtpStorage>());
        Assert.That(storage.Kind, Is.EqualTo(StorageProviderKind.Ftp));
        Assert.That(storage.IsPathTooLong("x", false, false), Is.False);
        Assert.That(storage.GetFreeSpace(), Is.EqualTo(0));
        Assert.That(storage.CanWriteToStorage(), Is.False);
    }
}

public class StorageHelperTests
{
    private sealed class StorageHarness : Storage
    {
        public static string Clean(string remoteFile) => CleanRemoteFileName(remoteFile);
        public static string Local(string localFile) => GetLocalFileName(localFile);
    }

    [TestCase(@"\folder\file.txt", "folder\\file.txt")]
    [TestCase(@"\", "")]
    [TestCase("folder\\file.txt", "folder\\file.txt")]
    public void CleanRemoteFileName_StripsLeadingSlash(string input, string expected)
    {
        Assert.That(StorageHarness.Clean(input), Is.EqualTo(expected));
    }

    [Test]
    public void CleanRemoteFileName_Throws_ForNull()
    {
        Assert.Throws<ArgumentNullException>(() => StorageHarness.Clean(null));
    }

    [Test]
    public void GetLocalFileName_PrefixesExtendedPath_ForLocalFiles()
    {
        Assert.That(StorageHarness.Local(@"C:\temp\a.txt"), Is.EqualTo(@"\\?\C:\temp\a.txt"));
        Assert.That(StorageHarness.Local(@"\\server\share\a.txt"), Is.EqualTo(@"\\server\share\a.txt"));
    }
}
