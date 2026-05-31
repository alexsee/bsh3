// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using Windows.System.Power;

namespace BSH.MainApp.Services;

public class PowerStatusService : IPowerStatusService
{
    public bool IsRunningOnBattery
    {
        get
        {
            try
            {
                return DetermineIsRunningOnBattery(PowerManager.BatteryStatus, PowerManager.PowerSupplyStatus);
            }
            catch
            {
                return false;
            }
        }
    }

    public static bool DetermineIsRunningOnBattery(BatteryStatus batteryStatus, PowerSupplyStatus powerSupplyStatus)
    {
        if (batteryStatus == BatteryStatus.NotPresent)
        {
            return false;
        }

        return batteryStatus == BatteryStatus.Discharging ||
            powerSupplyStatus == PowerSupplyStatus.NotPresent ||
            powerSupplyStatus == PowerSupplyStatus.Inadequate;
    }
}
