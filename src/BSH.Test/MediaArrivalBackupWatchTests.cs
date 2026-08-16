// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
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
        public FakeMediaWatcher Watcher { get; } = new();
        public int CreateCount { get; private set; }

        public IMediaWatcher Create()
        {
            CreateCount++;
            return Watcher;
        }
    }

    private sealed class FakeMediaWatcher : IMediaWatcher
    {
        public event EventHandler<string> DeviceAdded;
        public int StartWatchingCount { get; private set; }
        public int StopWatchingCount { get; private set; }

        public void StartWatching() => StartWatchingCount++;

        public void StopWatching() => StopWatchingCount++;

        public void Arrive(string driveLetter) => DeviceAdded?.Invoke(this, driveLetter);
    }
}
