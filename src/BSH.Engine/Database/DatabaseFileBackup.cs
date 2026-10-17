// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using System.IO;

namespace Brightbits.BSH.Engine.Database;

public static class DatabaseFileBackup
{
    public static string CreateTimestampedCopy(string databaseFile, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(databaseFile))
        {
            throw new ArgumentException("A database path is required.", nameof(databaseFile));
        }

        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("The database file does not exist.", databaseFile);
        }

        var backupPath = NextAvailablePath(databaseFile, timestamp);
        CopyShared(databaseFile, backupPath);
        CopySidecarIfPresent(databaseFile + "-wal", backupPath + ".wal");
        CopySidecarIfPresent(databaseFile + "-shm", backupPath + ".shm");
        return backupPath;
    }

    private static string NextAvailablePath(string databaseFile, DateTime timestamp)
    {
        var directory = Path.GetDirectoryName(databaseFile);
        if (string.IsNullOrEmpty(directory))
        {
            directory = ".";
        }

        var name = Path.GetFileName(databaseFile);
        var stamp = timestamp.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var candidate = Path.Combine(directory, name + "." + stamp + ".bak");
        var suffix = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(directory, name + "." + stamp + "-" + suffix.ToString(CultureInfo.InvariantCulture) + ".bak");
            suffix++;
        }

        return candidate;
    }

    private static void CopySidecarIfPresent(string source, string destination)
    {
        if (File.Exists(source))
        {
            CopyShared(source, destination);
        }
    }

    private static void CopyShared(string source, string destination)
    {
        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        input.CopyTo(output);
    }
}
