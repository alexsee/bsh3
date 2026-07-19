// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

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
    /// Estimates bytes that will actually be written to the backup device.
    /// Full backups copy every collected file; incrementals only copy files without
    /// a matching existing version (unchanged files are link-only and need no space).
    /// </summary>
    public static long EstimateRequiredCopyBytes(
        bool fullBackup,
        IEnumerable<(double FileSize, bool HasMatchingVersion)> files,
        long slackBytes = DefaultSlackBytes)
    {
        ArgumentNullException.ThrowIfNull(files);

        var sizesToCopy = fullBackup
            ? files.Select(file => file.FileSize)
            : files.Where(file => !file.HasMatchingVersion).Select(file => file.FileSize);

        return EstimateRequiredBytes(sizesToCopy, slackBytes);
    }

    /// <summary>
    /// True only when free space is known (positive) and below the estimate.
    /// Zero or negative free space means unknown (FTP/WebDAV/error) — soft skip.
    /// </summary>
    public static bool IsClearlyInsufficient(long freeSpaceBytes, long estimatedRequiredBytes)
        => freeSpaceBytes > 0 && estimatedRequiredBytes > freeSpaceBytes;
}
