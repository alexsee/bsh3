// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Providers.Ports;

public interface ISchedulerAdapter
{
    DateTime GetNextRun();
    void ScheduleAutoBackup(Action action);
    void ScheduleDaily(Action action, DateTime time);
    void ScheduleHourly(Action action, DateTime time);
    void ScheduleMonthly(Action action, DateTime time);
    void ScheduleOnce(Action action, DateTime time);
    void ScheduleWeekly(Action action, DateTime time);
    void Start();
    void Stop();
}
