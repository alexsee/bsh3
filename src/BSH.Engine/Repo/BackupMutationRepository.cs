// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Repo;

public class BackupMutationRepository : IBackupMutationRepository
{
    private readonly IDbClientFactory dbClientFactory;

    public BackupMutationRepository(IDbClientFactory dbClientFactory)
    {
        this.dbClientFactory = dbClientFactory;
    }

    public async Task<long> CreateVersionAsync(DbClient dbClient, string newVersionDate, string title, string description, bool fullBackup, string sources)
    {
        var backupParameters = new (string, object)[]
        {
            ("newVersionDate", newVersionDate),
            ("title", title),
            ("description", description),
            ("type", fullBackup ? "2" : "1"),
            ("sources", sources)
        };

        return (long)await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "INSERT INTO versiontable (versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable, versionSources) VALUES (" +
            "@newVersionDate, @title, @description, @type, '0', '0', @sources); select last_insert_rowid()",
            backupParameters);
    }

    public async Task<long?> GetFileIdAsync(DbClient dbClient, string fileName, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("fileName", fileName),
            ("filePath", filePath)
        };

        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT fileID FROM filetable WHERE fileName = @fileName AND filePath = @filePath LIMIT 1",
            parameters);

        if (result == null)
        {
            return null;
        }

        return long.TryParse(result.ToString(), out var fileId) ? fileId : null;
    }

    public async Task<long> CreateFileAsync(DbClient dbClient, string fileName, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("fileName", fileName),
            ("filePath", filePath)
        };

        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "INSERT INTO filetable ( fileName, filePath ) VALUES ( @fileName, @filePath ); SELECT last_insert_rowid()",
            parameters);

        return long.Parse(result?.ToString() ?? "0");
    }

    public async Task<long?> GetMatchingFileVersionIdAsync(DbClient dbClient, long fileId, double fileSize, DateTime fileDateModified)
    {
        var parameters = new (string, object)[]
        {
            ("fileID", fileId),
            ("fileSize", fileSize),
            ("fileDateModified", fileDateModified)
        };

        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT fileversionID FROM fileversiontable WHERE" +
            " fileID = @fileID AND fileStatus = 1 AND fileSize = @fileSize AND datetime(fileDateModified) = datetime(@fileDateModified) ORDER BY fileversionID DESC LIMIT 1",
            parameters);

        if (result == null)
        {
            return null;
        }

        return long.TryParse(result.ToString(), out var fileVersionId) ? fileVersionId : null;
    }

    public async Task AddFileLinkAsync(DbClient dbClient, long fileVersionId, long versionId)
    {
        var parameters = new (string, object)[]
        {
            ("fileversionID", fileVersionId),
            ("versionID", versionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "INSERT INTO filelink ( fileversionID, versionID ) VALUES ( @fileversionID, @versionID )",
            parameters);
    }

    public async Task<long> AddOrGetFolderIdAsync(DbClient dbClient, string folder)
    {
        var parameters = new (string, object)[]
        {
            ("folder", folder)
        };

        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "INSERT OR IGNORE INTO foldertable ( folder ) VALUES ( @folder ); SELECT id FROM foldertable WHERE folder = @folder",
            parameters);

        return long.Parse(result?.ToString() ?? "0");
    }

    public async Task AddFolderLinkAsync(DbClient dbClient, long folderId, long versionId)
    {
        var parameters = new (string, object)[]
        {
            ("folderid", folderId),
            ("versionID", versionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "INSERT INTO folderlink ( folderid, versionid ) VALUES ( @folderid, @versionID )",
            parameters);
    }

    public async Task AddFolderJunctionAsync(DbClient dbClient, string junctionPath, string displayName)
    {
        var parameters = new (string, object)[]
        {
            ("path", junctionPath),
            ("displayName", displayName)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "INSERT OR IGNORE INTO folderjunctiontable VALUES (@path, @displayName)",
            parameters);
    }

    public async Task AddFileVersionWithLinkAsync(DbClient dbClient, long fileId, long newVersionId, double fileSize, DateTime fileDateCreated, DateTime fileDateModified, int fileType, string longFileName)
    {
        var fileVersionParameters = new (string, object)[]
        {
            ("fileID", fileId),
            ("filePackage", newVersionId),
            ("fileSize", fileSize),
            ("fileDateCreated", fileDateCreated),
            ("fileDateModified", fileDateModified),
            ("fileHash", string.Empty),
            ("fileType", fileType),
            ("fileStatus", 1),
            ("longfilename", longFileName)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "INSERT INTO fileversiontable " +
            "( fileID, filePackage, fileSize, fileDateCreated, fileDateModified, fileHash, fileType, fileStatus, longfilename ) VALUES " +
            "( @fileID, @filePackage, @fileSize, @fileDateCreated, @fileDateModified, @fileHash, @fileType, @fileStatus, @longfilename )",
            fileVersionParameters);

        var fileLinkParameters = new (string, object)[]
        {
            ("versionID", newVersionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "INSERT INTO filelink ( fileversionID, versionID ) VALUES ( last_insert_rowid(), @versionID )",
            fileLinkParameters);
    }

    public async Task RenameVersionDateAsync(DbClient dbClient, long versionId, string newVersionDate)
    {
        var parameters = new (string, object)[]
        {
            ("newVersionDate", newVersionDate),
            ("versionID", versionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "UPDATE versiontable SET versionDate = @newVersionDate WHERE versionID = @versionID",
            parameters);
    }

    public async Task DeleteFileVersionAsync(DbClient dbClient, long fileVersionId)
    {
        var parameters = new (string, object)[]
        {
            ("id", fileVersionId)
        };

        await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM fileversiontable WHERE fileversionid = @id", parameters);
    }

    public async Task DeleteVersionMetadataAsync(DbClient dbClient, long versionId)
    {
        var parameters = new (string, object)[]
        {
            ("versionID", versionId)
        };

        await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM filelink WHERE versionID = @versionID", parameters);
        await dbClient.ExecuteNonQueryAsync("DELETE FROM filetable WHERE fileid NOT IN (SELECT fileid FROM fileversiontable)");
        await dbClient.ExecuteNonQueryAsync(CommandType.Text, "UPDATE versiontable SET versionStatus = 1 WHERE versionID = @versionID", parameters);

        await dbClient.ExecuteNonQueryAsync(CommandType.Text, "DELETE FROM folderlink WHERE versionID = @versionID", parameters);
        await dbClient.ExecuteNonQueryAsync("DELETE FROM foldertable WHERE id NOT IN (SELECT folderid FROM folderlink)");
    }

    public async Task DeleteSingleFileMetadataAsync(DbClient dbClient, string fileFilter, string pathFilter)
    {
        string whereClause;
        var parameters = new System.Collections.Generic.List<(string, object)>();

        if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
        {
            whereClause = "fileName = @fileName AND filePath = @filePath";
            parameters.Add(("fileName", fileFilter));
            parameters.Add(("filePath", pathFilter));
        }
        else
        {
            whereClause = "filePath LIKE @filePath";
            parameters.Add(("filePath", pathFilter));
        }

        var subQuerySql = "SELECT ft.fileID FROM fileTable AS ft WHERE " + whereClause;

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "DELETE FROM fileLink WHERE fileversionid IN (SELECT fileversionid FROM fileversiontable AS fvt WHERE fvt.fileID IN (" + subQuerySql + "))",
            parameters.ToArray());
        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "DELETE FROM fileversiontable WHERE fileID IN (" + subQuerySql + ")",
            parameters.ToArray());
        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "DELETE FROM fileTable AS ft WHERE " + whereClause,
            parameters.ToArray());
    }

    public async Task UpdateFileVersionTypeAsync(DbClient dbClient, long fileVersionId, int fileType)
    {
        var parameters = new (string, object)[]
        {
            ("fileType", fileType),
            ("fileversionid", fileVersionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "UPDATE fileversiontable SET fileType = @fileType WHERE fileversionid = @fileversionid",
            parameters);
    }

    public async Task SetVersionStableAsync(string version, bool stable)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var parameters = new (string, object)[]
        {
            ("stable", stable ? 1 : 0),
            ("version", version)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "UPDATE versiontable SET versionStable = @stable WHERE versionID = @version",
            parameters);
    }

    public async Task UpdateVersionDetailsAsync(long versionId, string title, string description)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        var parameters = new (string, object)[]
        {
            ("title", title ?? string.Empty),
            ("description", description ?? string.Empty),
            ("versionID", versionId)
        };

        await dbClient.ExecuteNonQueryAsync(
            CommandType.Text,
            "UPDATE versiontable SET versionTitle = @title, versionDescription = @description WHERE versionID = @versionID",
            parameters);
    }
}
