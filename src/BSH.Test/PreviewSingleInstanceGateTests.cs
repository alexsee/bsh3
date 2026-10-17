// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewSingleInstanceGateTests
{
    [Test]
    public void SecondPreviewLaunchSignalsTheRunningInstance()
    {
        var names = UniqueNames();
        var sync = new InMemoryPreviewInstanceSync();
        var activated = false;
        var first = PreviewSingleInstanceGate.Acquire(
            () => activated = true,
            out var gate,
            names.Preview,
            names.Engine,
            names.EventName,
            sync);

        Assert.That(first, Is.EqualTo(PreviewStartupResult.Started));
        Assert.That(gate, Is.Not.Null);

        try
        {
            var second = PreviewSingleInstanceGate.Acquire(
                () => { },
                out _,
                names.Preview,
                names.Engine,
                names.EventName,
                sync);

            Assert.That(second, Is.EqualTo(PreviewStartupResult.ActivateExisting));
            Assert.That(activated, Is.True);
        }
        finally
        {
            gate.Dispose();
        }
    }

    [Test]
    public void PreviewDoesNotStartWhenTheSharedEngineMutexIsHeld()
    {
        var names = UniqueNames();
        var sync = new InMemoryPreviewInstanceSync();
        Assert.That(sync.TryAcquireMutex(names.Engine, out var stable), Is.True);

        try
        {
            var result = PreviewSingleInstanceGate.Acquire(
                () => { },
                out var gate,
                names.Preview,
                names.Engine,
                names.EventName,
                sync);

            Assert.That(result, Is.EqualTo(PreviewStartupResult.BlockedByOtherEngine));
            Assert.That(gate, Is.Null);
        }
        finally
        {
            stable.Dispose();
        }
    }

    [Test]
    public void ReleasingTheGateLetsAnotherPreviewStart()
    {
        var names = UniqueNames();
        var sync = new InMemoryPreviewInstanceSync();
        var first = PreviewSingleInstanceGate.Acquire(
            () => { },
            out var gate,
            names.Preview,
            names.Engine,
            names.EventName,
            sync);
        Assert.That(first, Is.EqualTo(PreviewStartupResult.Started));
        gate.Dispose();

        var second = PreviewSingleInstanceGate.Acquire(
            () => { },
            out var next,
            names.Preview,
            names.Engine,
            names.EventName,
            sync);
        Assert.That(second, Is.EqualTo(PreviewStartupResult.Started));
        next.Dispose();
    }

    [Test]
    public void MissingPipeFileKeepsTheStablePipe()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.StablePipeName));
        }
        finally
        {
            Directory.Delete(directory);
        }
    }

    [Test]
    public void PipeFileSelectsThePreviewPipe()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "vss.pipe"), "backupservicehome-preview\n");

        try
        {
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.PreviewPipeName));
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.Not.EqualTo(VssConnection.StablePipeName));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void UnsafePipeFileFallsBackToTheStablePipe()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "vss.pipe"), "bad name");

        try
        {
            Assert.That(VssConnection.ResolveInstalledPipe(directory), Is.EqualTo(VssConnection.StablePipeName));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static (string Preview, string Engine, string EventName) UniqueNames()
    {
        var id = Guid.NewGuid().ToString("N");
        return ("preview-" + id, "engine-" + id, "activate-" + id);
    }
}
