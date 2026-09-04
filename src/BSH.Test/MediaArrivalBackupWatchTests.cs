// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Services;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test;

public class MediaArrivalBackupWatchTests
{
    [Test]
    public void Start_WhenTargetIsFtp_DoesNotCreateMediaWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.FileTransferServer, @"E:\Backups", factory);

        watch.Start(() => Task.CompletedTask);

        Assert.That(factory.CreateCount, Is.EqualTo(0));
        Assert.That(factory.Watcher.StartWatchingCount, Is.EqualTo(0));
    }

    [Test]
    public void Start_WhenTargetIsLocalVolume_StartsMediaWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);

        watch.Start(() => Task.CompletedTask);

        Assert.That(factory.CreateCount, Is.EqualTo(1));
        Assert.That(factory.Watcher.StartWatchingCount, Is.EqualTo(1));
    }

    [Test]
    public async Task MatchingDriveArrival_StartsPendingBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var backupStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var backupCalls = 0;

        watch.Start(() =>
        {
            backupCalls++;
            backupStarted.TrySetResult();
            return Task.CompletedTask;
        });

        factory.Watcher.Arrive("E:");

        await backupStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.That(backupCalls, Is.EqualTo(1));
        Assert.That(factory.Watcher.StopWatchingCount, Is.EqualTo(1));
    }

    [Test]
    public async Task OverlappingMatchingDriveArrivals_StartOnlyOnePendingBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var mediaCheckStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaCheck = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var backupStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaCheckCalls = 0;
        var backupCalls = 0;

        watch.Start(
            () =>
            {
                backupCalls++;
                backupStarted.TrySetResult();
                return Task.CompletedTask;
            },
            async () =>
            {
                mediaCheckCalls++;
                mediaCheckStarted.TrySetResult();
                return await mediaCheck.Task;
            });

        factory.Watcher.Arrive("E:");
        await mediaCheckStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        factory.Watcher.Arrive("E:");

        mediaCheck.TrySetResult(true);
        await backupStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(50);

        Assert.That(mediaCheckCalls, Is.EqualTo(1));
        Assert.That(backupCalls, Is.EqualTo(1));
        Assert.That(factory.Watcher.StopWatchingCount, Is.EqualTo(1));
    }

    [Test]
    public async Task StopDuringMediaValidation_DoesNotStartPendingBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var mediaCheckStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaCheck = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var backupCalls = 0;

        watch.Start(
            () =>
            {
                backupCalls++;
                return Task.CompletedTask;
            },
            async () =>
            {
                mediaCheckStarted.TrySetResult();
                return await mediaCheck.Task;
            });

        factory.Watcher.Arrive("E:");
        await mediaCheckStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        watch.Stop();
        mediaCheck.TrySetResult(true);
        await Task.Delay(50);

        Assert.That(backupCalls, Is.EqualTo(0));
        Assert.That(factory.Watcher.StopWatchingCount, Is.EqualTo(1));
        Assert.That(factory.Watcher.DeviceAddedSubscriberCount, Is.EqualTo(0));
    }

    [Test]
    public async Task StopDuringInitialMediaValidation_DoesNotStartWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var mediaCheckStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaCheck = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var startTask = watch.StartIfMediaMissing(
            () => Task.CompletedTask,
            async () =>
            {
                mediaCheckStarted.TrySetResult();
                return await mediaCheck.Task;
            });

        await mediaCheckStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        watch.Stop();
        mediaCheck.TrySetResult(false);

        await startTask;

        Assert.That(factory.CreateCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ArrivalFromStoppedSession_DoesNotStartLaterAutomationSession()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var oldMediaCheckStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldMediaCheck = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldBackupCalls = 0;
        var newBackupStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var newBackupCalls = 0;

        watch.Start(
            () =>
            {
                oldBackupCalls++;
                return Task.CompletedTask;
            },
            async () =>
            {
                oldMediaCheckStarted.TrySetResult();
                return await oldMediaCheck.Task;
            });

        var oldWatcher = factory.Watcher;
        oldWatcher.Arrive("E:");
        await oldMediaCheckStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        watch.Stop();
        watch.Start(
            () =>
            {
                newBackupCalls++;
                newBackupStarted.TrySetResult();
                return Task.CompletedTask;
            },
            () => Task.FromResult(true));

        oldMediaCheck.TrySetResult(true);
        await Task.Delay(50);

        Assert.That(oldBackupCalls, Is.EqualTo(0));
        Assert.That(newBackupCalls, Is.EqualTo(0));

        factory.Watchers[1].Arrive("E:");
        await newBackupStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.That(newBackupCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task NonMatchingDriveArrival_DoesNotStartBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var backupCalls = 0;

        watch.Start(() =>
        {
            backupCalls++;
            return Task.CompletedTask;
        });

        factory.Watcher.Arrive("F:");
        await Task.Delay(100);

        Assert.That(backupCalls, Is.EqualTo(0));
        Assert.That(factory.Watcher.StopWatchingCount, Is.EqualTo(0));
    }

    [Test]
    public async Task StartIfMediaMissing_WhenMediaUnavailable_StartsWatcherAndMatchingDriveRunsBackup()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);
        var backupStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var mediaAvailable = false;
        var backupCalls = 0;

        await watch.StartIfMediaMissing(
            () =>
            {
                backupCalls++;
                backupStarted.TrySetResult();
                return Task.CompletedTask;
            },
            () => Task.FromResult(mediaAvailable));

        Assert.That(factory.CreateCount, Is.EqualTo(1));

        mediaAvailable = true;
        factory.Watcher.Arrive("E:");

        await backupStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.That(backupCalls, Is.EqualTo(1));
    }

    [Test]
    public async Task StartIfMediaMissing_WhenMediaAvailable_DoesNotStartWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.LocalDevice, @"E:\Backups", factory);

        await watch.StartIfMediaMissing(() => Task.CompletedTask, () => Task.FromResult(true));

        Assert.That(factory.CreateCount, Is.EqualTo(0));
    }

    [Test]
    public async Task StartIfMediaMissing_WhenTargetIsFtp_DoesNotStartWatcher()
    {
        var factory = new FakeMediaWatcherFactory();
        var watch = CreateWatch(MediaType.FileTransferServer, @"E:\Backups", factory);

        await watch.StartIfMediaMissing(() => Task.CompletedTask, () => Task.FromResult(false));

        Assert.That(factory.CreateCount, Is.EqualTo(0));
    }

    private static MediaArrivalBackupWatch CreateWatch(
        MediaType mediaType,
        string backupFolder,
        FakeMediaWatcherFactory factory)
    {
        return new MediaArrivalBackupWatch(
            new FakeConfigurationManager
            {
                MediumType = mediaType,
                BackupFolder = backupFolder
            },
            factory);
    }

    private sealed class FakeMediaWatcherFactory : IMediaWatcherFactory
    {
        public List<FakeMediaWatcher> Watchers { get; } = new();
        private FakeMediaWatcher watcher = new();
        public FakeMediaWatcher Watcher => watcher;
        public int CreateCount { get; private set; }

        public IMediaWatcher Create()
        {
            CreateCount++;
            watcher = new FakeMediaWatcher();
            Watchers.Add(watcher);
            return watcher;
        }
    }

    private sealed class FakeMediaWatcher : IMediaWatcher
    {
        private EventHandler<string> deviceAdded;

        public event EventHandler<string> DeviceAdded
        {
            add => deviceAdded += value;
            remove => deviceAdded -= value;
        }

        public int StartWatchingCount { get; private set; }
        public int StopWatchingCount { get; private set; }
        public int DeviceAddedSubscriberCount => deviceAdded?.GetInvocationList().Length ?? 0;

        public void StartWatching() => StartWatchingCount++;

        public void StopWatching() => StopWatchingCount++;

        public void Arrive(string driveLetter) => deviceAdded?.Invoke(this, driveLetter);
    }
}
