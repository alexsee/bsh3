// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using Windows.System.Power;

namespace BSH.MainApp.Services;

public class PowerStatusService : IPowerStatusService
{
    private bool isMonitoring;
    private bool hasPowerStatusSnapshot;
    private bool hasBattery;
    private bool isRunningOnBattery;
    private EventHandler? powerStatusChanged;

    public bool HasBattery
    {
        get
        {
            EnsurePowerStatusSnapshot();
            return hasBattery;
        }
    }

    public bool IsRunningOnBattery
    {
        get
        {
            EnsurePowerStatusSnapshot();
            return isRunningOnBattery;
        }
    }

    public void StartPowerStatusMonitoring(EventHandler powerStatusChanged)
    {
        ArgumentNullException.ThrowIfNull(powerStatusChanged);

        this.powerStatusChanged -= powerStatusChanged;
        this.powerStatusChanged += powerStatusChanged;

        RefreshPowerStatus();
        if (isMonitoring || !hasBattery)
        {
            return;
        }

        PowerManager.BatteryStatusChanged += OnPowerStatusChanged;
        PowerManager.PowerSupplyStatusChanged += OnPowerStatusChanged;
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

        PowerManager.BatteryStatusChanged -= OnPowerStatusChanged;
        PowerManager.PowerSupplyStatusChanged -= OnPowerStatusChanged;
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

    private void OnPowerStatusChanged(object? sender, object e)
    {
        if (!RefreshPowerStatus())
        {
            return;
        }

        powerStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EnsurePowerStatusSnapshot()
    {
        if (hasPowerStatusSnapshot)
        {
            return;
        }

        RefreshPowerStatus();
    }

    private bool RefreshPowerStatus()
    {
        try
        {
            var batteryStatus = PowerManager.BatteryStatus;
            var powerSupplyStatus = PowerManager.PowerSupplyStatus;
            var nextHasBattery = batteryStatus != BatteryStatus.NotPresent;
            var nextIsRunningOnBattery = DetermineIsRunningOnBattery(
                batteryStatus,
                powerSupplyStatus);

            var changed = hasPowerStatusSnapshot &&
                (hasBattery != nextHasBattery || isRunningOnBattery != nextIsRunningOnBattery);

            hasBattery = nextHasBattery;
            isRunningOnBattery = nextIsRunningOnBattery;
            hasPowerStatusSnapshot = true;

            return changed;
        }
        catch
        {
            var changed = hasPowerStatusSnapshot && (hasBattery || isRunningOnBattery);

            hasBattery = false;
            isRunningOnBattery = false;
            hasPowerStatusSnapshot = true;

            return changed;
        }
    }
}
