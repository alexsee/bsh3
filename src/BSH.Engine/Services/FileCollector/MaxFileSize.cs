// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;

namespace Brightbits.BSH.Engine.Services.FileCollector;

/// <summary>
/// Reads and writes the "exclude files larger than" setting, which is stored as a number of megabytes.
/// </summary>
public static class MaxFileSize
{
    public const long BytesPerMegabyte = 1024L * 1024L;

    public static bool TryGetMegabytes(string value, out decimal megabytes)
    {
        megabytes = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out megabytes)
            && !decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out megabytes))
        {
            return false;
        }

        return megabytes > 0;
    }

    public static bool TryGetBytes(string value, out double bytes)
    {
        bytes = 0;
        if (!TryGetMegabytes(value, out var megabytes))
        {
            return false;
        }

        bytes = (double)megabytes * BytesPerMegabyte;
        return true;
    }

    public static string FromMegabytes(decimal megabytes)
    {
        return megabytes > 0 ? megabytes.ToString("0.##", CultureInfo.InvariantCulture) : string.Empty;
    }
}
