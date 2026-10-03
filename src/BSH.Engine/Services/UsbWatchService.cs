// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Management;
using Brightbits.BSH.Engine.Providers.Ports;
using Serilog;

namespace Brightbits.BSH.Engine.Services;

public class UsbWatchService : IMediaWatcher
{
    private ManagementEventWatcher watcher;

    public event EventHandler<string> DeviceAdded;

    public UsbWatchService()
    {

    }

    public void StartWatching()
    {
        // stop a previous watcher first so repeated starts don't orphan it
        StopWatching();

        ManagementEventWatcher current = null;
        try
        {
            var arriveQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            current = new ManagementEventWatcher
            {
                Query = arriveQuery
            };
            current.EventArrived += WatcherDeviceChange;

            current.Start();
            watcher = current;
        }
        catch (Exception ex)
        {
            if (current != null)
            {
                ReleaseWatcher(current, stop: false);
            }

            Log.Warning(ex, "USB device watcher could not be initialized.");
        }
    }

    public void StopWatching()
    {
        var current = watcher;
        watcher = null;

        if (current == null)
        {
            return;
        }

        ReleaseWatcher(current, stop: true);
    }

    private void ReleaseWatcher(ManagementEventWatcher current, bool stop)
    {
        try
        {
            current.EventArrived -= WatcherDeviceChange;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher event subscription could not be released.");
        }

        if (stop)
        {
            try
            {
                current.Stop();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "USB device watcher could not be stopped.");
            }
        }

        try
        {
            current.Dispose();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher could not be disposed.");
        }
    }

    private void WatcherDeviceChange(object sender, EventArrivedEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        try
        {
            var driveLetter = e.NewEvent.Properties["DriveName"]?.Value?.ToString();
            if (string.IsNullOrEmpty(driveLetter))
            {
                return;
            }

            DeviceAdded?.Invoke(this, driveLetter);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB arrival event could not be fired.");
        }
    }
}
