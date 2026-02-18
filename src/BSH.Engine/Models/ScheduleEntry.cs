// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Models;

public sealed class ScheduleEntry
{
    public int Type
    {
        get; set;
    }

    public DateTime Date
    {
        get; set;
    }
}
