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
    private long sessionId;
    private bool arrivalInProgress;

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
            StartCore(runBackup, isMediaAvailable);
        }
    }

    public async Task StartIfMediaMissing(Func<Task> runBackup, Func<Task<bool>> isMediaAvailable)
    {
        ArgumentNullException.ThrowIfNull(runBackup);
        ArgumentNullException.ThrowIfNull(isMediaAvailable);

        long startSession;
        lock (sync)
        {
            startSession = sessionId;
        }

        if (await isMediaAvailable())
        {
            return;
        }

        lock (sync)
        {
            if (sessionId != startSession)
            {
                return;
            }

            StartCore(runBackup, isMediaAvailable);
        }
    }

    private void StartCore(Func<Task> runBackup, Func<Task<bool>> isMediaAvailable)
    {
        if (watcher != null || configurationManager.MediumType == MediaType.FileTransferServer)
        {
            return;
        }

        sessionId++;
        this.runBackup = runBackup;
        this.isMediaAvailable = isMediaAvailable;
        arrivalInProgress = false;

        watcher = mediaWatcherFactory.Create();
        watcher.DeviceAdded += OnDeviceAdded;
        watcher.StartWatching();
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
            arrivalInProgress = false;
            sessionId++;
        }

        if (current == null)
        {
            return;
        }

        StopWatcher(current);
    }

    private void StopWatcher(IMediaWatcher current)
    {
        try
        {
            current.DeviceAdded -= OnDeviceAdded;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher event subscription could not be released.");
        }

        try
        {
            current.StopWatching();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher could not be stopped.");
        }
    }

    private async void OnDeviceAdded(object sender, string driveLetter)
    {
        long currentSession = 0;
        Func<Task> pending;
        Func<Task<bool>> mediaCheck;

        try
        {
            lock (sync)
            {
                if (watcher == null || !ReferenceEquals(sender, watcher))
                {
                    return;
                }

                if (!IsTargetDrive(configurationManager.BackupFolder, driveLetter) || arrivalInProgress)
                {
                    return;
                }

                currentSession = sessionId;
                arrivalInProgress = true;
                pending = runBackup;
                mediaCheck = isMediaAvailable;
            }

            if (mediaCheck != null && !await mediaCheck())
            {
                ReleaseArrival(currentSession, sender);
                return;
            }

            IMediaWatcher current;
            lock (sync)
            {
                if (sessionId != currentSession
                    || watcher == null
                    || !ReferenceEquals(sender, watcher))
                {
                    return;
                }

                if (!IsTargetDrive(configurationManager.BackupFolder, driveLetter))
                {
                    arrivalInProgress = false;
                    return;
                }

                current = watcher;
                watcher = null;
                runBackup = null;
                isMediaAvailable = null;
            }

            StopWatcher(current);

            Task backupTask = null;
            lock (sync)
            {
                if (sessionId != currentSession || !arrivalInProgress)
                {
                    return;
                }

                arrivalInProgress = false;
                // Start the callback while holding the lifecycle lock so Stop and
                // a later session cannot race ahead of an accepted arrival.
                if (pending != null)
                {
                    backupTask = pending();
                }
            }

            if (backupTask != null)
            {
                await backupTask;
            }
        }
        catch (Exception ex)
        {
            ReleaseArrival(currentSession, sender);
            Log.Warning(ex, "USB arrival backup could not be started.");
        }
    }

    private void ReleaseArrival(long currentSession, object sender)
    {
        lock (sync)
        {
            if (sessionId == currentSession
                && watcher != null
                && ReferenceEquals(sender, watcher))
            {
                arrivalInProgress = false;
            }
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
