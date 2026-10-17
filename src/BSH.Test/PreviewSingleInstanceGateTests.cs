// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading;
using Brightbits.BSH.Engine.Runtime;
using Brightbits.BSH.Engine.Services;
using BSH.Test.Fakes;
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
        next.Dispose();
    }

    [Test]
    public void AcquireRejectsAMissingCallback()
    {
        var sync = new InMemoryPreviewInstanceSync();
        Assert.Throws<ArgumentNullException>(() =>
            PreviewSingleInstanceGate.Acquire(null, out _, "preview", "engine", "activate", sync));
        Assert.Throws<ArgumentNullException>(() =>
            PreviewSingleInstanceGate.Acquire(() => { }, out _, "preview", "engine", "activate", null));
    }

    [Test]
    [Platform("Win")]
    public void WindowsSyncSignalsTheRunningPreview()
    {
        var names = UniqueNames();
        var sync = WindowsPreviewInstanceSync.Shared;
        var activated = new ManualResetEventSlim(false);
        var first = PreviewSingleInstanceGate.Acquire(
            () => activated.Set(),
            out var gate,
            names.Preview,
            names.Engine,
            names.EventName,
            sync);
        Assert.That(first, Is.EqualTo(PreviewStartupResult.Started));

        try
        {
            // A Windows mutex is recursive for the thread that already owns it.
            // The second launch has to come from another thread, as a second process would.
            PreviewStartupResult second = default;
            Exception error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    second = PreviewSingleInstanceGate.Acquire(
                        () => { },
                        out _,
                        names.Preview,
                        names.Engine,
                        names.EventName,
                        sync);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            });
            thread.Start();
            thread.Join();

            Assert.That(error, Is.Null);
            Assert.That(second, Is.EqualTo(PreviewStartupResult.ActivateExisting));
            Assert.That(activated.Wait(TimeSpan.FromSeconds(2)), Is.True);
        }
        finally
        {
            gate.Dispose();
        }
    }

    [Test]
    [Platform("Win")]
    public void WindowsSyncBlocksAndReleasesThePreviewMutex()
    {
        var names = UniqueNames();
        var sync = WindowsPreviewInstanceSync.Shared;
        var held = new ManualResetEventSlim(false);
        var release = new ManualResetEventSlim(false);
        var thread = new Thread(() =>
        {
            Assert.That(sync.TryAcquireMutex(names.Engine, out var ownership), Is.True);
            held.Set();
            release.Wait();
            ownership.Dispose();
        });
        thread.Start();
        Assert.That(held.Wait(TimeSpan.FromSeconds(2)), Is.True);

        try
        {
            var blocked = PreviewSingleInstanceGate.Acquire(
                () => { },
                out var gate,
                names.Preview,
                names.Engine,
                names.EventName,
                sync);
            Assert.That(blocked, Is.EqualTo(PreviewStartupResult.BlockedByOtherEngine));
            Assert.That(gate, Is.Null);
        }
        finally
        {
            release.Set();
            thread.Join();
        }

        var started = PreviewSingleInstanceGate.Acquire(
            () => { },
            out var next,
            names.Preview,
            names.Engine,
            names.EventName,
            sync);
        Assert.That(started, Is.EqualTo(PreviewStartupResult.Started));
        next.Dispose();
    }

    [Test]
    [Platform("Win")]
    public void WindowsSyncReleaseFromAnotherThreadDoesNotEscape()
    {
        var name = "bsh-rel-" + Guid.NewGuid().ToString("N");
        Assert.That(WindowsPreviewInstanceSync.Shared.TryAcquireMutex(name, out var ownership), Is.True);
        var thread = new Thread(() => ownership.Dispose());
        thread.Start();
        thread.Join();
    }

    [Test]
    [Platform("Win")]
    public void WindowsSyncIgnoresActivationCallbackFailures()
    {
        var name = "bsh-ev-" + Guid.NewGuid().ToString("N");
        var called = new ManualResetEventSlim(false);
        var listener = WindowsPreviewInstanceSync.Shared.Listen(name, () =>
        {
            called.Set();
            throw new InvalidOperationException("activation failed");
        });

        try
        {
            WindowsPreviewInstanceSync.Shared.Signal(name);
            Assert.That(called.Wait(TimeSpan.FromSeconds(2)), Is.True);
            Thread.Sleep(100);
        }
        finally
        {
            listener.Dispose();
        }
    }

    [Test]
    [Platform("Win")]
    public void WindowsSyncGivesUpWhenTheActivateEventDoesNotExist()
    {
        WindowsPreviewInstanceSync.Shared.Signal("missing-" + Guid.NewGuid().ToString("N"));
    }

    [Test]
    [Platform("Win")]
    public void ProductionMutexNamesCanBeAcquired()
    {
        var result = PreviewSingleInstanceGate.Acquire(() => { }, out var gate);
        try
        {
            Assert.That(result, Is.EqualTo(PreviewStartupResult.Started));
            Assert.That(gate, Is.Not.Null);
        }
        finally
        {
            gate?.Dispose();
        }
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
