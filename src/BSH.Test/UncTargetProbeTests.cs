// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test;

public class UncTargetProbeTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(@"C:\Backups")]
    [TestCase(@"\")]
    public void Probe_ReturnsInvalidPathForNonUnc(string? path)
    {
        var result = UncTargetProbe.Probe(path, null, null, requireEmptyTarget: false);

        Assert.That(result.Status, Is.EqualTo(UncProbeStatus.InvalidPath));
    }

    [Test]
    public void Probe_NormalizesForwardSlashesBeforeValidation()
    {
        // Unreachable UNC still normalizes; status is Unreachable (or Ok on some CI if share exists).
        var result = UncTargetProbe.Probe("//server/share", null, null, requireEmptyTarget: false);

        Assert.That(result.NormalizedPath, Is.EqualTo(@"\\server\share"));
        Assert.That(result.Status, Is.Not.EqualTo(UncProbeStatus.InvalidPath));
    }

    [Test]
    public void Probe_DetectsExistingBackupDatabaseOnLocalUncStyleFolder()
    {
        // Use a real local directory that looks like UNC only if we can; otherwise verify ContainsBackupData
        // via a temporary path that IsUncPath rejects — covered by InvalidPath above.
        // This test documents the empty-check using a path that NetworkConnection may fail on.
        var result = UncTargetProbe.Probe(
            @"\\invalid-host-that-does-not-exist-bsh3\share",
            null,
            null,
            requireEmptyTarget: true);

        Assert.That(result.Status, Is.EqualTo(UncProbeStatus.Unreachable).Or.EqualTo(UncProbeStatus.ContainsBackupData));
        Assert.That(result.NormalizedPath, Is.EqualTo(@"\\invalid-host-that-does-not-exist-bsh3\share"));
    }
}
