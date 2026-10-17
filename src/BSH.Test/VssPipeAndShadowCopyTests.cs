// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test;

public class VssPipeAndShadowCopyTests
{
    [Test]
    public void BlankOrOversizedPipeNamesStayOnTheStablePipe()
    {
        Assert.That(VssConnection.ResolveInstalledPipe(null), Is.EqualTo(VssConnection.StablePipeName));
        Assert.That(VssConnection.ResolveInstalledPipe("   "), Is.EqualTo(VssConnection.StablePipeName));

        var directory = Path.Combine(Path.GetTempPath(), "bsh-pipe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "vss.pipe"), "   \n");
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.StablePipeName));

            File.WriteAllText(Path.Combine(directory, "vss.pipe"), new string('a', 81));
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.StablePipeName));
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
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.StablePipeName));
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
