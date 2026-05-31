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
                return PowerManager.PowerSupplyStatus == PowerSupplyStatus.Inadequate;
            }
            catch
            {
                return false;
            }
        }
    }
}
