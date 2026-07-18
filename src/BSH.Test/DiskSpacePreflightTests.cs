// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using Brightbits.BSH.Engine.Utils;
using NUnit.Framework;

namespace BSH.Test;

public class DiskSpacePreflightTests
{
    [Test]
    public void EstimateRequiredBytes_SumsFileSizesAndAddsSlack()
    {
        var estimate = DiskSpacePreflight.EstimateRequiredBytes(
            new[] { 1000d, 2000d, 3000d },
            slackBytes: 100);

        Assert.That(estimate, Is.EqualTo(6100));
    }

    [Test]
    public void EstimateRequiredBytes_IgnoresNonPositiveSizes()
    {
        var estimate = DiskSpacePreflight.EstimateRequiredBytes(
            new[] { 500d, 0d, -10d },
            slackBytes: 50);

        Assert.That(estimate, Is.EqualTo(550));
    }

    [Test]
    public void EstimateRequiredBytes_UsesDefaultSlackWhenNotSpecified()
    {
        var estimate = DiskSpacePreflight.EstimateRequiredBytes(new[] { 1024d });

        Assert.That(estimate, Is.EqualTo(1024 + DiskSpacePreflight.DefaultSlackBytes));
    }

    [Test]
    public void IsClearlyInsufficient_ReturnsFalse_WhenFreeSpaceUnknown()
    {
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(0, estimatedRequiredBytes: 1_000_000), Is.False);
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(-1, estimatedRequiredBytes: 1_000_000), Is.False);
    }

    [Test]
    public void IsClearlyInsufficient_ReturnsFalse_WhenEnoughSpace()
    {
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(10_000, estimatedRequiredBytes: 5_000), Is.False);
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(5_000, estimatedRequiredBytes: 5_000), Is.False);
    }

    [Test]
    public void IsClearlyInsufficient_ReturnsTrue_WhenClearlyShort()
    {
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(4_999, estimatedRequiredBytes: 5_000), Is.True);
        Assert.That(DiskSpacePreflight.IsClearlyInsufficient(100, estimatedRequiredBytes: 1_000_000), Is.True);
    }

    [Test]
    public void ShouldAbortBackup_CombinesEstimateAndFreeSpaceCheck()
    {
        var fileSizes = new List<double> { 8_000, 2_000 };

        Assert.That(
            DiskSpacePreflight.ShouldAbortBackup(freeSpaceBytes: 0, fileSizes, slackBytes: 100),
            Is.False,
            "Unknown free space must soft-skip");

        Assert.That(
            DiskSpacePreflight.ShouldAbortBackup(freeSpaceBytes: 50_000, fileSizes, slackBytes: 100),
            Is.False,
            "Enough free space must allow backup");

        Assert.That(
            DiskSpacePreflight.ShouldAbortBackup(freeSpaceBytes: 5_000, fileSizes, slackBytes: 100),
            Is.True,
            "Clearly insufficient free space must abort");
    }
}
