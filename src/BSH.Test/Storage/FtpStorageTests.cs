// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
using BSH.Test.Helpers;
using NUnit.Framework;

namespace BSH.Test.Storage;

/// <summary>
/// Covers the hardened <see cref="FtpStorage"/> construction and temporary-file
/// handling without requiring a live FTP server.
/// </summary>
public class FtpStorageTests
{
    private string temporaryDirectory;

    [SetUp]
    public void SetUp()
    {
        temporaryDirectory = Path.Combine(Path.GetTempPath(), "bsh-ftp-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Test]
    public async Task CheckMediumValidatesDownloadedVersionMarker()
    {
        // Exercise the real FTP transport, including controls that prove validation
        // reached the downloaded marker rather than failing during connection setup.
        foreach (var marker in new[] { "7", "", "8", "corrupt", "2147483648" })
        {
            await using var server = new VersionMarkerFtpServer(marker);
            using var storage = new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 7);
            if (marker is "7" or "")
            {
                Assert.That(await storage.CheckMedium(), Is.True, marker);
            }
            else
            {
                Assert.ThrowsAsync<DeviceContainsWrongStateException>(async () => await storage.CheckMedium(), marker);
            }
            Assert.That(server.MarkerDownloaded, Is.True, "The check must reach the remote version marker.");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task EncryptedRestoreReportsInvalidEncryptedData(bool destinationExists)
    {
        await using var server = new VersionMarkerFtpServer("invalid encrypted data");
        using var storage = new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 7);
        storage.Open();
        var destination = Path.Combine(temporaryDirectory, "restored.txt");
        if (destinationExists)
        {
            await File.WriteAllTextAsync(destination, "original contents");
        }

        var restored = storage.CopyFileFromStorageEncrypted(destination, "file", "password");

        Assert.That(server.MarkerDownloaded, Is.True, "The encrypted payload must reach the decryption step.");
        Assert.That(restored, Is.False);
        if (destinationExists)
        {
            Assert.That(await File.ReadAllTextAsync(destination), Is.EqualTo("original contents"));
        }
        else
        {
            Assert.That(File.Exists(destination), Is.False);
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task VersionUpdateReportsTransferResult(bool rejectUpload)
    {
        await using var server = new VersionMarkerFtpServer("7", rejectUploads: rejectUpload);
        using var storage = new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 7);
        storage.Open();

        if (rejectUpload)
        {
            Assert.Throws<IOException>(() => storage.UpdateStorageVersion(8));
        }
        else
        {
            Assert.DoesNotThrow(() => storage.UpdateStorageVersion(8));
        }
        Assert.That(server.UploadReceived, Is.True, "The version marker must reach the FTP server.");
    }

    [Test]
    public async Task EncryptedRestoreReplacesDestinationAfterSuccessfulDecryption()
    {
        var source = Path.Combine(temporaryDirectory, "source.txt");
        var encrypted = Path.Combine(temporaryDirectory, "source.enc");
        var destination = Path.Combine(temporaryDirectory, "restored.txt");
        await File.WriteAllTextAsync(source, "restored contents");
        await File.WriteAllTextAsync(destination, "original contents");
        Assert.That(new Encryption().Encode(source, encrypted, "password"), Is.True);
        await using var server = new VersionMarkerFtpServer(await File.ReadAllBytesAsync(encrypted));
        using var storage = new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 7);
        storage.Open();

        Assert.That(storage.CopyFileFromStorageEncrypted(destination, "file", "password"), Is.True);
        Assert.That(await File.ReadAllTextAsync(destination), Is.EqualTo("restored contents"));
    }

    [Test]
    public void ConstructorFallsBackToDefaultsOnInvalidConfig()
    {
        using var storage = new FtpStorage(new FakeConfigurationManager
        {
            FtpHost = "example.org",
            FtpPort = "not-a-port",
            OldBackupPrevent = "not-a-version"
        });

        Assert.That(GetPrivateField<int>(storage, "serverPort"), Is.EqualTo(21));
        Assert.That(GetPrivateField<int>(storage, "currentStorageVersion"), Is.EqualTo(0));
    }

    [Test]
    public void ConstructorHonorsValidConfig()
    {
        using var storage = new FtpStorage(new FakeConfigurationManager
        {
            FtpHost = "example.org",
            FtpPort = "2121",
            OldBackupPrevent = "7"
        });

        Assert.That(GetPrivateField<int>(storage, "serverPort"), Is.EqualTo(2121));
        Assert.That(GetPrivateField<int>(storage, "currentStorageVersion"), Is.EqualTo(7));
    }

    [Test]
    public void CreateUniqueTempFileReturnsUniqueNonExistentPaths()
    {
        var first = InvokePrivate<string>("CreateUniqueTempFile", ".zip");
        var second = InvokePrivate<string>("CreateUniqueTempFile", ".zip");

        Assert.That(first, Does.EndWith(".zip"));
        Assert.That(second, Does.EndWith(".zip"));
        Assert.That(first, Is.Not.EqualTo(second));
        Assert.That(File.Exists(first), Is.False);
        Assert.That(File.Exists(second), Is.False);
    }

    [Test]
    public void TryDeleteTempFileRemovesExistingFilesAndIgnoresMissingOnes()
    {
        var existing = Path.Combine(temporaryDirectory, "stale.tmp");
        File.WriteAllText(existing, "stale");

        Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", Path.Combine(temporaryDirectory, "missing.tmp")));
        Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", existing));
        Assert.That(File.Exists(existing), Is.False);
    }

    [Test]
    public void TryDeleteTempFileSurvivesLockedFiles()
    {
        var locked = Path.Combine(temporaryDirectory, "locked.tmp");
        File.WriteAllText(locked, "locked");

        using (new FileStream(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // deletion fails on Windows (sharing violation); elsewhere it succeeds silently
            Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", locked));
        }

        TryDeleteBestEffort(locked);
    }

    [Test]
    public void DisposeWithoutOpenDoesNotThrow()
    {
        using var storage = CreateUnopenedStorage();

        Assert.DoesNotThrow(() => storage.Dispose());
    }

    [Test]
    public void DisposeWithFinalizerPathIsNoOp()
    {
        using var storage = CreateUnopenedStorage();
        var dispose = typeof(FtpStorage).GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(bool)], null);
        Assert.That(dispose, Is.Not.Null);

        Assert.DoesNotThrow(() => dispose.Invoke(storage, [false]));
    }

    [Test]
    public void CopyOperationsRequireAnOpenConnection()
    {
        var source = Path.Combine(temporaryDirectory, "source.bin");
        File.WriteAllText(source, "content");
        var localTarget = Path.Combine(temporaryDirectory, "target.bin");

        using var storage = CreateUnopenedStorage();

        // ftpClient is only created in Open(); without it every operation fails fast
        // while still cleaning up its unique temporary files via finally.
        Assert.Throws<NullReferenceException>(() => storage.CopyFileToStorageCompressed(source, "remote"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileToStorageEncrypted(source, "remote", "password"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileFromStorageCompressed(localTarget, "remote"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileFromStorageEncrypted(localTarget, "remote", "password"));
    }

    private static FtpStorage CreateUnopenedStorage()
    {
        return new FtpStorage("example.org", 21, "user", "pass", "/backups", "UTF-8", false, 0);
    }

    private static T GetPrivateField<T>(FtpStorage storage, string name)
    {
        var field = typeof(FtpStorage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null);
        return (T)field.GetValue(storage);
    }

    private static T InvokePrivate<T>(string name, params object[] args)
    {
        var method = typeof(FtpStorage).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        return (T)method.Invoke(null, args);
    }

    private static void TryDeleteBestEffort(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignore; teardown removes the directory
        }
    }

}
