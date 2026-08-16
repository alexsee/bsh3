// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using BSH.MainApp.Helpers;
using NUnit.Framework;

namespace BSH.Test;

public class ProgressDisplayTests
{
    [Test]
    public void NormalizeNeverReturnsZeroMaximum()
    {
        var (maximum, value) = ProgressDisplay.Normalize(0, 0);

        Assert.That(maximum, Is.EqualTo(1));
        Assert.That(value, Is.EqualTo(0));
    }

    [Test]
    public void NormalizeClampsCurrentToMaximum()
    {
        var (maximum, value) = ProgressDisplay.Normalize(10, 50);

        Assert.That(maximum, Is.EqualTo(10));
        Assert.That(value, Is.EqualTo(10));
    }

    [Test]
    public void NormalizeClampsNegativeCurrentToZero()
    {
        var (maximum, value) = ProgressDisplay.Normalize(10, -3);

        Assert.That(maximum, Is.EqualTo(10));
        Assert.That(value, Is.EqualTo(0));
    }

    [Test]
    public void AssignLowersValueBeforeMaximumWhenMaximumWouldDropBelowValue()
    {
        var log = new List<string>();

        ProgressDisplay.Assign(
            currentMaximum: 1000,
            currentValue: 800,
            total: 10,
            current: 2,
            setMaximum: maximum => log.Add($"max={maximum}"),
            setValue: value => log.Add($"val={value}"));

        Assert.That(log, Is.EqualTo(new[] { "val=2", "max=10" }));
    }

    [Test]
    public void AssignSetsMaximumBeforeValueWhenMaximumGrows()
    {
        var log = new List<string>();

        ProgressDisplay.Assign(
            currentMaximum: 100,
            currentValue: 0,
            total: 500,
            current: 0,
            setMaximum: maximum => log.Add($"max={maximum}"),
            setValue: value => log.Add($"val={value}"));

        Assert.That(log, Is.EqualTo(new[] { "max=500", "val=0" }));
    }

    [Test]
    public void AssignFromPrepareProgressNeverLeavesZeroMaximum()
    {
        var maximum = 100;
        var value = 0;

        ProgressDisplay.Assign(
            maximum,
            value,
            total: 0,
            current: 0,
            setMaximum: next => maximum = next,
            setValue: next => value = next);

        Assert.That(maximum, Is.EqualTo(1));
        Assert.That(value, Is.EqualTo(0));
    }
}
