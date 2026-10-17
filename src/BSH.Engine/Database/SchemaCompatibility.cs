// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Exceptions;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// Schema versions understood by this build. Stable 3.9.5 and the WinUI Preview
/// both stop at version 9.
/// </summary>
public static class SchemaCompatibility
{
    public const int SupportedSchemaVersion = 9;

    public static async Task CheckExistingDatabaseAsync(string databaseFile)
    {
        if (!File.Exists(databaseFile))
        {
            return;
        }

        var connectionString = new SQLiteConnectionStringBuilder
        {
            DataSource = databaseFile,
            ReadOnly = true,
            FailIfMissing = true
        };
        using var dbClient = new DbClient(connectionString.ToString());
        var value = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT confValue FROM configuration WHERE confProperty = 'dbversion' LIMIT 1",
            null);
        ParseSupportedVersion(value?.ToString());
    }

    public static int ParseSupportedVersion(string dbVersion)
    {
        if (string.IsNullOrEmpty(dbVersion))
        {
            return 1;
        }

        if (!int.TryParse(dbVersion, out var version) || version > SupportedSchemaVersion)
        {
            throw new DatabaseIncompatibleException();
        }

        return version;
    }
}
