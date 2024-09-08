// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
