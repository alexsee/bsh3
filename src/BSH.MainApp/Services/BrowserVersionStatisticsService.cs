// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using Brightbits.BSH.Engine.Contracts.Database;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;

public sealed class BrowserVersionStatisticsService(IDbClientFactory dbClientFactory)
{
    public Task<BrowserVersionChangeStatistics> GetChangesAsync(string versionId) => Task.Run(async () =>
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, """
            WITH previous_version AS (
                SELECT MAX(versionID) AS id FROM versiontable
                WHERE versionID < @version AND versionStatus = 0
            ), current_files AS (
                SELECT DISTINCT f.fileID, f.fileSize, f.fileDateModified
                FROM filelink l JOIN fileversiontable f ON f.fileversionID = l.fileversionID
                WHERE l.versionID = @version
            ), previous_files AS (
                SELECT DISTINCT f.fileID, f.fileSize, f.fileDateModified
                FROM filelink l JOIN fileversiontable f ON f.fileversionID = l.fileversionID
                WHERE l.versionID = (SELECT id FROM previous_version)
            )
            SELECT
                (SELECT COUNT(*) FROM current_files c
                 WHERE NOT EXISTS (SELECT 1 FROM previous_files p WHERE p.fileID = c.fileID)),
                (SELECT COUNT(*) FROM current_files c JOIN previous_files p ON p.fileID = c.fileID
                 WHERE c.fileSize IS NOT p.fileSize OR c.fileDateModified IS NOT p.fileDateModified),
                (SELECT COUNT(*) FROM previous_files p
                 WHERE NOT EXISTS (SELECT 1 FROM current_files c WHERE c.fileID = p.fileID))
            """, [("version", versionId)]);

        await reader.ReadAsync();
        return new BrowserVersionChangeStatistics(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
    });
}
