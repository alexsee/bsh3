// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
    public void EstimateRequiredBytes_TreatsNegativeSlackAsZero()
    {
        var estimate = DiskSpacePreflight.EstimateRequiredBytes(new[] { 100d }, slackBytes: -5);

        Assert.That(estimate, Is.EqualTo(100));
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
}
