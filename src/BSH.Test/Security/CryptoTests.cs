// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Security.Cryptography;
using Brightbits.BSH.Engine.Utils.Security;
using NUnit.Framework;

namespace BSH.Test.Security;

public class CryptoTests
{
    [Test]
    public void EncryptString_RoundTrips_WithCurrentUserScope()
    {
        var encrypted = Crypto.EncryptString("secret-value", DataProtectionScope.CurrentUser);
        Assert.That(encrypted, Is.Not.Empty);

        var decrypted = Crypto.DecryptString(encrypted, DataProtectionScope.CurrentUser);
        Assert.That(decrypted, Is.EqualTo("secret-value"));
    }

    [Test]
    public void DecryptString_ReturnsEmpty_ForInvalidPayload()
    {
        Assert.That(Crypto.DecryptString("not-valid-base64!!!"), Is.EqualTo(""));
    }

    [Test]
    public void EncryptString_DefaultOverload_RoundTrips()
    {
        var encrypted = Crypto.EncryptString("abc");
        Assert.That(Crypto.DecryptString(encrypted), Is.EqualTo("abc"));
    }
}
