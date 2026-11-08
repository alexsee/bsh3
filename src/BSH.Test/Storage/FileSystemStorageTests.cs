// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test.Storage;

public class FileSystemStorageTests
{
    private string temporaryDirectory;

    [SetUp]
    public void SetUp()
    {
        temporaryDirectory = Path.Combine(Path.GetTempPath(), "bsh-storage-tests-" + Guid.NewGuid().ToString("N"));
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
    public void DecryptOnStorage_WhenDecodeFails_PreservesEncryptedAndExistingPlaintextFiles()
    {
        const string remoteFile = "payload.bin";
        var encryptedFile = Path.Combine(temporaryDirectory, remoteFile + ".enc");
        var plaintextFile = Path.Combine(temporaryDirectory, remoteFile);
        byte[] encryptedContent = [1, 2, 3];

        File.WriteAllBytes(encryptedFile, encryptedContent);
        File.WriteAllText(plaintextFile, "existing plaintext");

        using var storage = CreateStorage();
        var result = storage.DecryptOnStorage(remoteFile, "wrong-password");

        Assert.That(result, Is.False);
        Assert.That(File.ReadAllBytes(encryptedFile), Is.EqualTo(encryptedContent));
        Assert.That(File.ReadAllText(plaintextFile), Is.EqualTo("existing plaintext"));
        Assert.That(Directory.EnumerateFiles(temporaryDirectory, "*.tmp"), Is.Empty);
    }

    [Test]
    public void DecryptOnStorage_WhenDecodeSucceeds_ReplacesPlaintextThenDeletesEncryptedFile()
    {
        const string remoteFile = "payload.bin";
        const string password = "password";
        var sourceFile = Path.Combine(temporaryDirectory, "source.bin");
        var encryptedFile = Path.Combine(temporaryDirectory, remoteFile + ".enc");
        var plaintextFile = Path.Combine(temporaryDirectory, remoteFile);

        File.WriteAllText(sourceFile, "decrypted content");
        File.WriteAllText(plaintextFile, "old content");
        Assert.That(new Encryption().Encode(sourceFile, encryptedFile, password), Is.True);

        using var storage = CreateStorage();
        var result = storage.DecryptOnStorage(remoteFile, password);

        Assert.That(result, Is.True);
        Assert.That(File.Exists(encryptedFile), Is.False);
        Assert.That(File.ReadAllText(plaintextFile), Is.EqualTo("decrypted content"));
        Assert.That(Directory.EnumerateFiles(temporaryDirectory, "*.tmp"), Is.Empty);
    }

    [Test]
    public void DecryptOnStorage_WhenPlaintextCannotBePublished_PreservesEncryptedFileAndCleansStagingFile()
    {
        const string remoteFile = "payload.bin";
        const string password = "password";
        var sourceFile = Path.Combine(temporaryDirectory, "source.bin");
        var encryptedFile = Path.Combine(temporaryDirectory, remoteFile + ".enc");
        var plaintextPath = Path.Combine(temporaryDirectory, remoteFile);

        File.WriteAllText(sourceFile, "decrypted content");
        Assert.That(new Encryption().Encode(sourceFile, encryptedFile, password), Is.True);
        Directory.CreateDirectory(plaintextPath);

        using var storage = CreateStorage();

        // File.Move onto a directory path fails with IOException on Unix and
        // UnauthorizedAccessException on Windows (ERROR_ACCESS_DENIED).
        Assert.Throws(
            Is.InstanceOf<IOException>().Or.InstanceOf<UnauthorizedAccessException>(),
            () => storage.DecryptOnStorage(remoteFile, password));
        Assert.That(File.Exists(encryptedFile), Is.True);
        Assert.That(Directory.Exists(plaintextPath), Is.True);
        Assert.That(Directory.EnumerateFiles(temporaryDirectory, "*.tmp"), Is.Empty);
    }

    private FileSystemStorage CreateStorage()
    {
        return new FileSystemStorage(new FakeConfigurationManager
        {
            BackupFolder = temporaryDirectory,
            OldBackupPrevent = "1"
        });
    }

    [Test]
    public void CopyFileFromStorageCompressedReturnsFalseWhenEntryIsMissing()
    {
        const string remoteFile = "payload.bin";
        using (var zipFile = ZipFile.Open(Path.Combine(temporaryDirectory, remoteFile + ".zip"), ZipArchiveMode.Create))
        {
            var entry = zipFile.CreateEntry("other.bin");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("other content");
        }

        using var storage = CreateStorage();
        var localFile = Path.Combine(temporaryDirectory, "restored", remoteFile);

        Assert.That(storage.CopyFileFromStorageCompressed(localFile, remoteFile), Is.False);
    }

    [Test]
    public void CopyFromStorageByType_WhenPasswordIsWrong_ThrowsAndLeavesNoRestoredFile()
    {
        const string remoteFile = "payload.bin";
        const string password = "password";
        const string content = "secret content that spans more than one AES block of sixteen bytes.";
        var encryptedFile = WriteEncryptedPayload(remoteFile, password, content);
        var encryptedContent = File.ReadAllBytes(encryptedFile);
        var localFile = Path.Combine(temporaryDirectory, "restored", remoteFile);

        using var storage = CreateStorage();

        Assert.That(storage.CopyFileFromStorageEncrypted(localFile, remoteFile, "wrong-password"), Is.False);
        AssertNoRestoredFile(localFile);

        var exception = Assert.Throws<IOException>(() =>
            StoragePath.CopyFromStorageByType(storage, FileTypeKind.Encrypted, localFile, remoteFile, "wrong-password"));

        Assert.That(exception.Message, Does.Contain(remoteFile).And.Contain(localFile));
        AssertNoRestoredFile(localFile);
        Assert.That(File.ReadAllBytes(encryptedFile), Is.EqualTo(encryptedContent));
    }

    [Test]
    public void CopyFromStorageByType_WhenPayloadIsCorrupt_ThrowsAndLeavesNoRestoredFile()
    {
        const string remoteFile = "payload.bin";
        var encryptedFile = Path.Combine(temporaryDirectory, remoteFile + ".enc");
        var localFile = Path.Combine(temporaryDirectory, "restored", remoteFile);
        byte[] corruptPayload = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        File.WriteAllBytes(encryptedFile, corruptPayload);

        using var storage = CreateStorage();

        Assert.That(storage.CopyFileFromStorageEncrypted(localFile, remoteFile, "password"), Is.False);
        AssertNoRestoredFile(localFile);

        Assert.Throws<IOException>(() =>
            StoragePath.CopyFromStorageByType(storage, FileTypeKind.Encrypted, localFile, remoteFile, "password"));

        AssertNoRestoredFile(localFile);
        Assert.That(File.ReadAllBytes(encryptedFile), Is.EqualTo(corruptPayload));
    }

    [Test]
    public void CopyFromStorageByType_WhenPasswordIsWrongAndDestinationExists_ThrowsAndDoesNotLeaveGarbage()
    {
        const string remoteFile = "payload.bin";
        const string password = "password";
        const string previous = "previous contents";
        WriteEncryptedPayload(remoteFile, password, "secret content that spans more than one AES block of sixteen bytes.");
        var localFile = Path.Combine(temporaryDirectory, "restored", remoteFile);
        Directory.CreateDirectory(Path.GetDirectoryName(localFile));
        File.WriteAllText(localFile, previous);

        using var storage = CreateStorage();

        Assert.Throws<IOException>(() =>
            StoragePath.CopyFromStorageByType(storage, FileTypeKind.Encrypted, localFile, remoteFile, "wrong-password"));

        if (File.Exists(localFile))
        {
            Assert.That(File.ReadAllText(localFile), Is.EqualTo(previous));
        }

        foreach (var entry in Directory.EnumerateFiles(Path.GetDirectoryName(localFile)))
        {
            Assert.That(entry, Is.EqualTo(localFile));
        }
    }

    [Test]
    public void CopyFileFromStorageEncrypted_WhenPasswordMatches_RestoresPlaintext()
    {
        const string remoteFile = "payload.bin";
        const string password = "password";
        const string content = "secret content that spans more than one AES block of sixteen bytes.";
        var localFile = Path.Combine(temporaryDirectory, "restored", remoteFile);
        WriteEncryptedPayload(remoteFile, password, content);

        using var storage = CreateStorage();

        Assert.That(storage.CopyFileFromStorageEncrypted(localFile, remoteFile, password), Is.True);
        Assert.That(File.ReadAllText(localFile), Is.EqualTo(content));

        StoragePath.CopyFromStorageByType(storage, FileTypeKind.Encrypted, localFile, remoteFile, password);
        Assert.That(File.ReadAllText(localFile), Is.EqualTo(content));
    }

    private string WriteEncryptedPayload(string remoteFile, string password, string content)
    {
        var sourceFile = Path.Combine(temporaryDirectory, "source.bin");
        var encryptedFile = Path.Combine(temporaryDirectory, remoteFile + ".enc");
        File.WriteAllText(sourceFile, content);
        Assert.That(new Encryption().Encode(sourceFile, encryptedFile, password), Is.True);
        return encryptedFile;
    }

    private static void AssertNoRestoredFile(string localFile)
    {
        Assert.That(File.Exists(localFile), Is.False);
        var restoreDirectory = Path.GetDirectoryName(localFile);
        if (Directory.Exists(restoreDirectory))
        {
            Assert.That(Directory.EnumerateFileSystemEntries(restoreDirectory), Is.Empty);
        }
    }

    [Test]
    public void DisposeWithFinalizerPathIsNoOp()
    {
        using var storage = CreateStorage();
        var dispose = typeof(FileSystemStorage).GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(bool)], null);
        Assert.That(dispose, Is.Not.Null);

        Assert.DoesNotThrow(() => dispose.Invoke(storage, [false]));
    }
}
