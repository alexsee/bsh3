// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// Schema versions understood by this build. Stable 3.9.5 and the WinUI Preview
/// both stop at version 9.
/// </summary>
public static class SchemaCompatibility
{
    public const int SupportedSchemaVersion = 9;

    public static bool IsNewerThanSupported(string dbVersion)
    {
        return int.TryParse(dbVersion, out var version) && version > SupportedSchemaVersion;
    }

    public static bool WillChangeSchema(string dbVersion)
    {
        if (string.IsNullOrEmpty(dbVersion))
        {
            return true;
        }

        return int.TryParse(dbVersion, out var version) && version < SupportedSchemaVersion;
    }
}
