// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Models;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Helpers;

public static class ScheduleEditorDisplayText
{
    public static string GetScheduleKind(ScheduleEntryKind kind)
    {
        return $"Schedule_Kind_{kind}".GetLocalized() ?? kind.ToString();
    }
}
