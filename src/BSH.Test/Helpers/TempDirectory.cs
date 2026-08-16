// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace BSH.Test.Helpers;

/// <summary>
/// Best-effort cleanup for temporary test directories that SQLite may still have open.
/// </summary>
public static class TempDirectory
{
    public static void DeleteBestEffort(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Windows can keep a lock on the SQLite file after ClosePool.
        }
    }
}
