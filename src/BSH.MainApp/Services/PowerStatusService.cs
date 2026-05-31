// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using Microsoft.Win32;
using Windows.System.Power;

namespace BSH.MainApp.Services;

public class PowerStatusService : IPowerStatusService
{
    private bool isMonitoring;
    private bool lastIsRunningOnBattery;
    private EventHandler? powerStatusChanged;

    public bool HasBattery
    {
        get
        {
            try
            {
                return PowerManager.BatteryStatus != BatteryStatus.NotPresent;
            }
            catch
            {
                return false;
            }
        }
    }

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

    public void StartPowerStatusMonitoring(EventHandler powerStatusChanged)
    {
        ArgumentNullException.ThrowIfNull(powerStatusChanged);

        this.powerStatusChanged -= powerStatusChanged;
        this.powerStatusChanged += powerStatusChanged;

        if (isMonitoring || !HasBattery)
        {
            return;
        }

        lastIsRunningOnBattery = IsRunningOnBattery;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        isMonitoring = true;
    }

    public void StopPowerStatusMonitoring(EventHandler powerStatusChanged)
    {
        ArgumentNullException.ThrowIfNull(powerStatusChanged);

        this.powerStatusChanged -= powerStatusChanged;
        if (this.powerStatusChanged != null || !isMonitoring)
        {
            return;
        }

        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        isMonitoring = false;
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

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        var isRunningOnBattery = IsRunningOnBattery;
        if (lastIsRunningOnBattery == isRunningOnBattery)
        {
            return;
        }

        lastIsRunningOnBattery = isRunningOnBattery;
        powerStatusChanged?.Invoke(this, EventArgs.Empty);
    }
}
