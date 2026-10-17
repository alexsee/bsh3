// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Services;
using BSH.Service.Shared;
using NUnit.Framework;

namespace BSH.Test;

public class VssPipeAndShadowCopyTests
{
    [Test]
    public void BlankOrOversizedPipeNamesStayOnTheStablePipe()
    {
        Assert.That(VssPipeNames.ReadInstalledPipeName(null), Is.EqualTo(VssPipeNames.Stable));
        Assert.That(VssPipeNames.ReadInstalledPipeName("   "), Is.EqualTo(VssPipeNames.Stable));

        var directory = Path.Combine(Path.GetTempPath(), "bsh-pipe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "vss.pipe"), "   \n");
            Assert.That(VssPipeNames.ReadInstalledPipeName(directory), Is.EqualTo(VssPipeNames.Stable));

            File.WriteAllText(Path.Combine(directory, "vss.pipe"), new string('a', 81));
            Assert.That(VssPipeNames.ReadInstalledPipeName(directory), Is.EqualTo(VssPipeNames.Stable));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void ALockedPipeFileFallsBackToTheStablePipe()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bsh-pipe-lock-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var pipeFile = Path.Combine(directory, "vss.pipe");
        File.WriteAllText(pipeFile, "backupservicehome-preview");

        try
        {
            using var locked = new FileStream(pipeFile, FileMode.Open, FileAccess.Read, FileShare.None);
            Assert.That(VssPipeNames.ReadInstalledPipeName(directory), Is.EqualTo(VssPipeNames.Stable));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void ShadowCopyClientUsesTheConfiguredPipeAndReportsAMissingService()
    {
        var stable = new VolumeShadowCopyClient();
        _ = new VolumeShadowCopyClient(null);
        _ = new VolumeShadowCopyClient("   ");
        var preview = new VolumeShadowCopyClient(new string('a', 81));

        Assert.Throws<ArgumentNullException>(() => stable.CopyFile(null, "dest"));
        Assert.Throws<ArgumentNullException>(() => stable.CopyFile("source", null));
        Assert.That(preview.CopyFile("source-file", "dest-file"), Is.False);
    }
}
