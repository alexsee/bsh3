// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;

namespace Brightbits.BSH.Engine.Utils;

/// <summary>
/// Rough disk-space estimate and gate before a backup starts copying files.
/// Exact size is hard (compression); this only aborts when free space is known
/// and clearly below the estimated need.
/// </summary>
public static class DiskSpacePreflight
{
    /// <summary>
    /// Small overhead for database updates, directory entries, and rounding.
    /// </summary>
    public const long DefaultSlackBytes = 16L * 1024 * 1024;

    public static long EstimateRequiredBytes(IEnumerable<double> fileSizes, long slackBytes = DefaultSlackBytes)
    {
        ArgumentNullException.ThrowIfNull(fileSizes);

        long total = slackBytes > 0 ? slackBytes : 0;
        foreach (var size in fileSizes)
        {
            if (size > 0)
            {
                total += (long)size;
            }
        }

        return total;
    }

    /// <summary>
    /// True only when free space is known (positive) and below the estimate.
    /// Zero or negative free space means unknown (FTP/WebDAV/error) — soft skip.
    /// </summary>
    public static bool IsClearlyInsufficient(long freeSpaceBytes, long estimatedRequiredBytes)
        => freeSpaceBytes > 0 && estimatedRequiredBytes > freeSpaceBytes;
}
