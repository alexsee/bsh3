// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Utils;
public static class IOUtils
{
    public static string GetRelativeFolder(string path, string rootPath)
    {
        ArgumentNullException.ThrowIfNull(path);

        var result = path;
        result = result.Replace(rootPath, "", StringComparison.OrdinalIgnoreCase);

        if (result.StartsWith('\\') && result.Length > 1)
        {
            result = result[1..];
        }
        else if (result == "\\")
        {
            return "";
        }

        return result;
    }
}
