// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
        // delete the temporary file
        File.Delete(mTempFile);
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
}
