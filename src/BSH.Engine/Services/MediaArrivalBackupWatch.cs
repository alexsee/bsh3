// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Providers.Ports;
using Serilog;

namespace Brightbits.BSH.Engine.Services;

/// <summary>
/// Starts a local-volume media watcher and runs a pending backup when the
/// configured target drive arrives. FTP targets are ignored.
/// </summary>
public sealed class MediaArrivalBackupWatch
{
    private readonly IConfigurationManager configurationManager;
    private readonly IMediaWatcherFactory mediaWatcherFactory;
    private readonly object sync = new();

    private IMediaWatcher watcher;
    private Func<Task> runBackup;
    private Func<Task<bool>> isMediaAvailable;

    public MediaArrivalBackupWatch(
        IConfigurationManager configurationManager,
        IMediaWatcherFactory mediaWatcherFactory)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);
        ArgumentNullException.ThrowIfNull(mediaWatcherFactory);

        this.configurationManager = configurationManager;
        this.mediaWatcherFactory = mediaWatcherFactory;
    }

    public void Start(Func<Task> runBackup, Func<Task<bool>> isMediaAvailable = null)
    {
        ArgumentNullException.ThrowIfNull(runBackup);

        lock (sync)
        {
            if (watcher != null)
            {
                return;
            }

            if (configurationManager.MediumType == MediaType.FileTransferServer)
            {
                return;
            }

            this.runBackup = runBackup;
            this.isMediaAvailable = isMediaAvailable;

            watcher = mediaWatcherFactory.Create();
            watcher.DeviceAdded += OnDeviceAdded;
            watcher.StartWatching();
        }
    }

    public async Task StartIfMediaMissing(Func<Task> runBackup, Func<Task<bool>> isMediaAvailable)
    {
        ArgumentNullException.ThrowIfNull(runBackup);
        ArgumentNullException.ThrowIfNull(isMediaAvailable);

        if (await isMediaAvailable())
        {
            return;
        }

        Start(runBackup, isMediaAvailable);
    }

    public void Stop()
    {
        IMediaWatcher current;
        lock (sync)
        {
            current = watcher;
            watcher = null;
            runBackup = null;
            isMediaAvailable = null;
        }

        if (current == null)
        {
            return;
        }

        try
        {
            current.DeviceAdded -= OnDeviceAdded;
            current.StopWatching();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher could not be stopped.");
        }
    }

    private async void OnDeviceAdded(object sender, string driveLetter)
    {
        try
        {
            Func<Task> pending;
            Func<Task<bool>> mediaCheck;
            lock (sync)
            {
                pending = runBackup;
                mediaCheck = isMediaAvailable;
            }

            if (mediaCheck != null && !await mediaCheck())
            {
                return;
            }

            if (!IsTargetDrive(configurationManager.BackupFolder, driveLetter))
            {
                return;
            }

            Stop();
            if (pending != null)
            {
                await pending();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB arrival backup could not be started.");
        }
    }

    internal static bool IsTargetDrive(string backupFolder, string driveLetter)
    {
        if (string.IsNullOrEmpty(backupFolder) || string.IsNullOrEmpty(driveLetter))
        {
            return false;
        }

        return backupFolder.StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase);
    }
}
