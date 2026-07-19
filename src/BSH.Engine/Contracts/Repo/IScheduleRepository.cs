// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Contracts.Repo;

public interface IScheduleRepository
{
    Task<bool> HasScheduleEntriesAsync();
    Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync();
    Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> schedules);
}
