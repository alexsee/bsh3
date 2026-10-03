// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brightbits.BSH.Engine.Jobs;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Covers the hardened <see cref="RestoreJob"/> destination mapping,
/// including unmatched source roots (no <c>NullReferenceException</c>).
/// </summary>
public class RestoreJobDestinationTests
{
    [Test]
    public void MultiDestinationFallsBackToFirstDestinationForUnknownRoot()
    {
        var result = InvokeGetFileDestination(
            new List<string> { @"D:\Dest", @"E:\Other" },
            @"\Unknown\file");

        Assert.That(result, Is.EqualTo(@"D:\Dest\Unknown\file" + Path.DirectorySeparatorChar));
    }

    [Test]
    public void MultiDestinationRemapsKnownSourceRoot()
    {
        var result = InvokeGetFileDestination(
            new List<string> { @"D:\Data\A", @"D:\Data\B" },
            @"\A\sub");

        Assert.That(result, Is.EqualTo(@"D:\Data\A\sub" + Path.DirectorySeparatorChar));
    }

    [Test]
    public void NullArgumentsAreRejected()
    {
        Assert.Throws<TargetInvocationException>(() => InvokeGetFileDestination(null!, @"\A\"));
        Assert.Throws<TargetInvocationException>(() => InvokeGetFileDestination(new List<string> { @"D:\Dest" }, null!));
    }

    private static string InvokeGetFileDestination(List<string> destFolders, string fileDest)
    {
        var method = typeof(RestoreJob).GetMethod("GetFileDestination", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        return (string)method.Invoke(null, [destFolders, fileDest]);
    }
}
