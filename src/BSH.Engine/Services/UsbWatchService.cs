// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Management;
using Serilog;

namespace Brightbits.BSH.Engine.Services;

public class UsbWatchService
{
    private ManagementEventWatcher watcher;

    public event EventHandler<string> DeviceAdded;

    public UsbWatchService()
    {

    }

    public void StartWatching()
    {
        try
        {
            var arriveQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");

            watcher = new ManagementEventWatcher
            {
                Query = arriveQuery
            };
            watcher.EventArrived += WatcherDeviceChange;

            watcher.Start();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB device watcher could not be initialized.");
        }
    }

    public void StopWatching()
    {
        this.watcher?.Stop();
    }

    public void WatcherDeviceChange(object sender, EventArrivedEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        try
        {
            var driveLetter = e.NewEvent.Properties["DriveName"]?.Value?.ToString();
            DeviceAdded?.Invoke(this, driveLetter);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "USB arrival event could not be fired.");
        }
    }
}
