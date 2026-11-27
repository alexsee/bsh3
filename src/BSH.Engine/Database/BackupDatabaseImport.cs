// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Database;

public static class BackupDatabaseImport
{
    public static void DownloadFromStorage(IStorageProvider storage, string destination)
    {
        var stagingFile = Path.Combine(Path.GetDirectoryName(destination), Path.GetRandomFileName());
        try
        {
            if (!storage.CopyFileFromStorage(stagingFile, "backup.bshdb"))
            {
                throw new IOException("The backup database could not be downloaded.");
            }

            DbClientFactory.ClosePool();
            File.Delete(destination + "-wal");
            File.Delete(destination + "-shm");
            File.Move(stagingFile, destination, overwrite: true);
        }
        finally
        {
            File.Delete(stagingFile);
        }
    }

    public static async Task RemapVersionSourcesAsync(
        IDbClientFactory dbClientFactory,
        IEnumerable<VersionDetails> versions,
        IEnumerable<(string OriginalPath, string NewPath)> remaps)
    {
        var mappings = remaps
            .Where(remap => !string.IsNullOrWhiteSpace(remap.OriginalPath))
            .Select(remap => (Prefix: remap.OriginalPath.TrimEnd('\\'), remap.NewPath, Changed: remap.OriginalPath != remap.NewPath))
            .OrderByDescending(remap => remap.Prefix.Length)
            .ToArray();
        if (!mappings.Any(remap => remap.Changed))
        {
            return;
        }

        using var dbClient = dbClientFactory.CreateDbClient();
        dbClient.BeginTransaction();
        foreach (var version in versions)
        {
            var originalSources = version.Sources ?? "";
            var remappedSources = string.Join("|", originalSources.Split('|')
                .Select(source => RemapSource(source, mappings)));
            if (remappedSources != originalSources)
            {
                await dbClient.ExecuteNonQueryAsync(
                    CommandType.Text,
                    "UPDATE versiontable SET versionSources = @sources WHERE versionID = @versionId",
                    [("sources", remappedSources), ("versionId", version.Id)]);
            }
        }

        dbClient.CommitTransaction();
    }

    private static string RemapSource(string source, (string Prefix, string NewPath, bool Changed)[] mappings)
    {
        var path = source.TrimEnd('\\');
        foreach (var (prefix, newPath, changed) in mappings)
        {
            if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return changed ? newPath : source;
            }

            if (path.StartsWith(prefix + "\\", StringComparison.OrdinalIgnoreCase))
            {
                return changed ? newPath.TrimEnd('\\') + source[prefix.Length..] : source;
            }
        }

        return source;
    }
}
