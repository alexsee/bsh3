// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Security;
using NUnit.Framework;

namespace BSH.Test.Security;
public class HashTests
{
    [Test]
    public void TestMd5()
    {
        var hash = Hash.GetMD5Hash("test");
        Assert.That(hash, Is.EqualTo("098f6bcd4621d373cade4e832627b4f6"));
    }
}
