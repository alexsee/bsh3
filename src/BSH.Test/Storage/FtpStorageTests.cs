// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Reflection;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
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
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore; teardown removes the directory
        }
    }
}
