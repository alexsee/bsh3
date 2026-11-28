// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

public class LegacyBrowserFavoriteTests
{
    [TestCase("")]
    [TestCase("Old merged label")]
    public async Task RepairsMissingNamesWithoutShiftingTheFavoritePositions(string savedNames)
    {
        var names = await frmBrowser.GetFavoriteNamesAsync(
            [@"\Old documents\", "", @"\Old pictures\"],
            savedNames,
            path => Task.FromResult(path.Replace("Old ", "", StringComparison.Ordinal)));

        Assert.That(names, Is.EqualTo(new[] { "documents", "", "pictures" }));
    }

    [Test]
    public async Task KeepsSavedLabelsWhenTheyMatchTheFavoritePositions()
    {
        var names = await frmBrowser.GetFavoriteNamesAsync(
            [@"\Documents\", @"\Pictures\"],
            "Custom label|Pictures",
            _ => throw new InvalidOperationException("Saved labels should be reused."));

        Assert.That(names, Is.EqualTo(new[] { "Custom label", "Pictures" }));
    }
}
