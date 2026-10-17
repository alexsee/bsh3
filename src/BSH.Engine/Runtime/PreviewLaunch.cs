// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace Brightbits.BSH.Engine.Runtime;

public static class PreviewLaunch
{
    public const string AutoStartArgument = "-autostart";

    public static bool IsAutoStart(IEnumerable<string> args)
    {
        if (args == null)
        {
            return false;
        }

        foreach (var arg in args)
        {
            if (string.Equals(arg, AutoStartArgument, StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "/autostart", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
