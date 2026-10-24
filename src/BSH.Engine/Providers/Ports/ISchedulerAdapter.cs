// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Providers.Ports;

public interface ISchedulerAdapter
{
    DateTime GetNextRun();
    void ScheduleAutoBackup(Func<Task> action);
    void ScheduleDaily(Func<Task> action, DateTime time);
    void ScheduleHourly(Func<Task> action, DateTime time);
    void ScheduleMonthly(Func<Task> action, DateTime time);
    void ScheduleOnce(Func<Task> action, DateTime time);
    void ScheduleWeekly(Func<Task> action, DateTime time);
    void Start();
    void Stop();
}
