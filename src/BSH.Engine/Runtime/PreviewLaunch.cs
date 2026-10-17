// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightbits.BSH.Engine.Runtime;

public static class PreviewLaunch
{
    public const string AutoStartArgument = "-autostart";

    public static bool IsAutoStart(IEnumerable<string> args)
    {
        return args != null && args.Any(arg =>
            string.Equals(arg, AutoStartArgument, StringComparison.OrdinalIgnoreCase)
            || string.Equals(arg, "/autostart", StringComparison.OrdinalIgnoreCase));
    }
}
