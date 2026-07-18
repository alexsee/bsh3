// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IPowerStatusService
{
    bool HasBattery
    {
        get;
    }

    bool IsRunningOnBattery
    {
        get;
    }

    void StartPowerStatusMonitoring(EventHandler powerStatusChanged);

    void StopPowerStatusMonitoring(EventHandler powerStatusChanged);
}
