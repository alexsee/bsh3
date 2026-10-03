// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Security;
using NUnit.Framework;

namespace BSH.Test.Security;
public class EncryptionTests
{
    private string mTempFile;

    [TearDown]
    public void TearDown()
    {
        if (string.IsNullOrEmpty(mTempFile))
        {
            return;
        }

        File.Delete(mTempFile);
        mTempFile = null;
    }

    [Test]
    public void EncodeDecodeTest()
    {
        var secureContent = "This is a test file with some very important content.";
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, secureContent);
        mTempFile = tempFile;

        // Arrange
        var sourceFile = Path.GetTempFileName();
        var targetFile = Path.GetTempFileName();
        var password = "password";

        // Act
        var encryption = new Encryption();

        var result = encryption.Encode(tempFile, targetFile, password);
        Assert.That(result, Is.True);

        // Act
        result = encryption.Decode(targetFile, sourceFile, password);
        Assert.That(result, Is.True);

        // Assert
        var sourceContent = File.ReadAllText(sourceFile);
        Assert.That(sourceContent, Is.EqualTo(secureContent));

        // Clean up
        File.Delete(sourceFile);
        File.Delete(targetFile);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MissingSourcePreservesExistingTarget(bool decrypt)
    {
        var encryption = new Encryption();
        var missingSource = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var targetFile = Path.GetTempFileName();
        File.WriteAllText(targetFile, "existing content");

        try
        {
            var result = decrypt
                ? encryption.Decode(missingSource, targetFile, "password")
                : encryption.Encode(missingSource, targetFile, "password");
            Assert.That(result, Is.False);
            Assert.That(File.ReadAllText(targetFile), Is.EqualTo("existing content"));
        }
        finally
        {
            File.Delete(targetFile);
        }
    }

    [Test]
    public void DecodeReturnsFalseWhenPasswordIsWrong()
    {
        var secureContent = "This is a test file with some very important content.";
        var sourceFile = Path.GetTempFileName();
        var encryptedFile = Path.GetTempFileName();
        var decodedFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(sourceFile, secureContent);
            var encryption = new Encryption();

            Assert.That(encryption.Encode(sourceFile, encryptedFile, "password"), Is.True);
            Assert.That(encryption.Decode(encryptedFile, decodedFile, "wrong-password"), Is.False);
            Assert.That(File.Exists(decodedFile), Is.False);
        }
        finally
        {
            File.Delete(sourceFile);
            File.Delete(encryptedFile);
            File.Delete(decodedFile);
        }
    }

    [Test]
    public void EncodePreservesTargetWhenItCannotBeOpened()
    {
        mTempFile = Path.GetTempFileName();
        File.WriteAllText(mTempFile, "source content");
        var targetFile = Path.GetTempFileName();
        File.WriteAllText(targetFile, "stale target");

        try
        {
            using (new FileStream(targetFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.That(new Encryption().Encode(mTempFile, targetFile, "password"), Is.False);
            }
            Assert.That(File.ReadAllText(targetFile), Is.EqualTo("stale target"));
        }
        finally
        {
            TryDeleteBestEffort(targetFile);
        }
    }

    private static void TryDeleteBestEffort(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // locked files are released by the disposed stream above
        }
    }
}
