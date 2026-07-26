// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;

namespace BSH.Test.Helpers;

/// <summary>
/// SQL seed helpers for black-box job tests that inject metadata without a prior backup.
/// </summary>
public static class JobSeedHelper
{
    public static async Task SeedVersionAsync(
        IDbClientFactory dbClientFactory,
        int versionId,
        string versionDate,
        string sources = @"D:\Source",
        int versionStatus = 0)
    {
        var escapedSources = sources.Replace("'", "''");
        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable, versionSources) " +
            $"VALUES ({versionId}, '{versionDate}', 'v{versionId}', '', 2, {versionStatus}, 1, '{escapedSources}')");
    }

    public static async Task SeedFileForVersionAsync(
        IDbClientFactory dbClientFactory,
        int fileId,
        int fileVersionId,
        int versionId,
        string fileName,
        string filePath,
        int fileType,
        string longFileName,
        int filePackage = -1,
        long fileSize = 123,
        string dateModified = "2021-01-01 00:00:00",
        string dateCreated = "2021-01-01 00:00:00")
    {
        var package = filePackage < 0 ? versionId : filePackage;
        var escapedName = fileName.Replace("'", "''");
        var escapedPath = filePath.Replace("'", "''");
        var escapedLong = (longFileName ?? "").Replace("'", "''");

        // Insert filetable row only when this fileId is new.
        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            var existing = await dbClient.ExecuteScalarAsync($"SELECT COUNT(*) FROM filetable WHERE fileID = {fileId}");
            if (existing == null || System.Convert.ToInt32(existing) == 0)
            {
                await dbClientFactory.ExecuteNonQueryAsync(
                    $"INSERT INTO filetable (fileID, fileName, filePath) VALUES ({fileId}, '{escapedName}', '{escapedPath}')");
            }
        }

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            var existingVersion = await dbClient.ExecuteScalarAsync(
                $"SELECT COUNT(*) FROM fileversiontable WHERE fileversionID = {fileVersionId}");
            if (existingVersion == null || System.Convert.ToInt32(existingVersion) == 0)
            {
                await dbClientFactory.ExecuteNonQueryAsync(
                    "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) VALUES " +
                    $"({fileVersionId}, 1, {fileType}, '', '{dateModified}', '{dateCreated}', {fileSize}, {package}, {fileId}, '{escapedLong}')");
            }
        }

        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO filelink (fileversionID, versionID) VALUES ({fileVersionId}, {versionId})");
    }

    public static async Task SeedEmptyFolderAsync(
        IDbClientFactory dbClientFactory,
        int folderId,
        int versionId,
        string folderPath)
    {
        var escaped = folderPath.Replace("'", "''");

        using (var dbClient = dbClientFactory.CreateDbClient())
        {
            var existing = await dbClient.ExecuteScalarAsync($"SELECT COUNT(*) FROM foldertable WHERE id = {folderId}");
            if (existing == null || System.Convert.ToInt32(existing) == 0)
            {
                await dbClientFactory.ExecuteNonQueryAsync(
                    $"INSERT INTO foldertable (id, folder) VALUES ({folderId}, '{escaped}')");
            }
        }

        await dbClientFactory.ExecuteNonQueryAsync(
            $"INSERT INTO folderlink (folderid, versionid) VALUES ({folderId}, {versionId})");
    }
}
