// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Repo;

public class VersionQueryRepository : IVersionQueryRepository
{
    public async Task<string> GetLastVersionDateAsync(DbClient dbClient)
    {
        var result = await dbClient.ExecuteScalarAsync("SELECT versionDate FROM versiontable ORDER BY versionID DESC LIMIT 1");
        return result?.ToString();
    }

    public async Task<int> CountRestoreFilesByPathAsync(DbClient dbClient, int versionId, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("version", versionId),
            ("path", filePath + "%")
        };

        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT COUNT(*) FROM filetable, fileversiontable, filelink " +
            "WHERE filelink.fileversionID = fileversiontable.fileversionID " +
            "AND fileversiontable.fileID = filetable.fileID " +
            "AND filelink.versionID = @version " +
            "AND filetable.filePath LIKE @path",
            parameters);

        return int.Parse(result?.ToString() ?? "0");
    }

    public async Task<DbDataReader> GetRestoreSingleFileAsync(DbClient dbClient, int versionId, string fileName, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("version", versionId),
            ("fileName", fileName),
            ("filePath", filePath)
        };

        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT " +
            "fileversiontable.*, filetable.*, versiontable.versionDate " +
            "FROM filetable, fileversiontable, versiontable, filelink " +
            "WHERE filelink.fileversionID = fileversiontable.fileversionID " +
            "AND filelink.versionID = @version " +
            "AND fileversiontable.filePackage = versiontable.versionID " +
            "AND filetable.fileName = @fileName " +
            "AND filetable.filePath = @filePath " +
            "AND fileversiontable.fileID = filetable.fileID LIMIT 1",
            parameters);
    }

    public async Task<DbDataReader> GetRestoreFilesByPathAsync(DbClient dbClient, int versionId, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("version", versionId),
            ("path", filePath + "%")
        };

        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT filetable.*, versiontable.versionDate, fileversiontable.* " +
            "FROM filetable, versiontable, fileversiontable, filelink " +
            "WHERE filelink.fileversionID = fileversiontable.fileversionID " +
            "AND fileversiontable.fileID = filetable.fileID " +
            "AND filePackage = versiontable.versionID " +
            "AND filelink.versionID = @version " +
            "AND filePath LIKE @path",
            parameters);
    }

    public async Task<DbDataReader> GetRestoreFoldersByPathAsync(DbClient dbClient, int versionId, string filePath)
    {
        var parameters = new (string, object)[]
        {
            ("version", versionId),
            ("path", filePath + "%")
        };

        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT folder FROM foldertable, folderlink " +
            "WHERE foldertable.id = folderlink.folderid " +
            "AND folderlink.versionid = @version " +
            "AND foldertable.folder LIKE @path",
            parameters);
    }

    public DataSet GetFilesToDeleteForVersion(DbClient dbClient, int versionId)
    {
        var parameters = new (string, object)[]
        {
            ("version", versionId)
        };

        return dbClient.ExecuteDataSet(
            CommandType.Text,
            "SELECT b.fileName, b.filePath, c.fileversionid, c.fileType, d.versionDate, c.longfilename " +
            "FROM filelink a, filetable b, fileversiontable c, versiontable d " +
            "WHERE a.versionID = @version AND a.fileversionid NOT IN " +
            "   (SELECT fileversionid FROM filelink WHERE (versionID <> @version)) " +
            "AND a.fileversionid = c.fileversionid " +
            "AND c.fileid = b.fileid " +
            "AND d.versionid = c.filepackage",
            parameters);
    }

    public async Task<DbDataReader> GetOrphanedVersionDatesAsync(DbClient dbClient)
    {
        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT versiondate FROM versiontable WHERE versionid NOT IN (SELECT filepackage FROM fileversiontable)",
            null);
    }

    public async Task<IReadOnlyList<int>> GetFileIdsForDeleteSingleAsync(DbClient dbClient, string fileFilter, string pathFilter)
    {
        string sql;
        (string, object)[] parameters;

        if (!string.IsNullOrEmpty(fileFilter) && !string.IsNullOrEmpty(pathFilter))
        {
            sql = "SELECT ft.fileID FROM fileTable AS ft WHERE fileName = @fileName AND filePath = @filePath";
            parameters =
            [
                ("fileName", fileFilter),
                ("filePath", pathFilter)
            ];
        }
        else
        {
            sql = "SELECT ft.fileID FROM fileTable AS ft WHERE filePath LIKE @filePath";
            parameters =
            [
                ("filePath", pathFilter)
            ];
        }

        var result = new List<int>();
        using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, sql, parameters);

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetInt32(0));
        }

        await reader.CloseAsync();
        return result;
    }

    public async Task<DbDataReader> GetFileVersionsForDeleteSingleAsync(DbClient dbClient, int fileId)
    {
        var parameters = new (string, object)[]
        {
            ("fileId", fileId)
        };

        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT * FROM fileVersionTable AS fvt " +
            "INNER JOIN fileTable AS ft ON " +
            "  ft.fileID = fvt.fileID " +
            "INNER JOIN versionTable AS vt ON " +
            "  fvt.filePackage = vt.versionID " +
            "WHERE fvt.fileID = @fileId",
            parameters);
    }

    public async Task<DbDataReader> GetFileVersionsExclusiveToVersionsForDeleteSingleAsync(
        DbClient dbClient,
        int fileId,
        IReadOnlyList<int> versionIds)
    {
        ArgumentNullException.ThrowIfNull(versionIds);
        if (versionIds.Count == 0)
        {
            throw new ArgumentException("At least one version ID is required.", nameof(versionIds));
        }

        var (inClause, inParameters) = SqlParameterBuilder.BuildInClause("v", versionIds);
        var parameters = SqlParameterBuilder.Combine([("fileId", fileId)], inParameters);

        // Physical payloads that are linked only within the target version set (no remaining outside links).
        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT DISTINCT ft.fileName, ft.filePath, fvt.fileversionid, fvt.fileType, vt.versionDate, fvt.longfilename " +
            "FROM filelink AS fl " +
            "INNER JOIN fileversiontable AS fvt ON fl.fileversionID = fvt.fileversionID " +
            "INNER JOIN filetable AS ft ON fvt.fileID = ft.fileID " +
            "INNER JOIN versiontable AS vt ON fvt.filePackage = vt.versionID " +
            "WHERE fvt.fileID = @fileId " +
            "AND fl.versionID IN (" + inClause + ") " +
            "AND fvt.fileversionid NOT IN (" +
            "  SELECT fileversionid FROM filelink WHERE versionID NOT IN (" + inClause + ")" +
            ")",
            parameters);
    }

    public async Task<int> CountEditableFilesAsync(DbClient dbClient)
    {
        var result = await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT COUNT(1) FROM filetable a, fileversiontable b, filelink c, versiontable d " +
            "WHERE(c.fileversionid = b.fileversionid And a.fileid = b.fileid) " +
            "and d.versionid = b.filepackage",
            null);

        return int.Parse(result?.ToString() ?? "0");
    }

    public async Task<DbDataReader> GetEditableFilesAsync(DbClient dbClient)
    {
        return await dbClient.ExecuteDataReaderAsync(
            CommandType.Text,
            "SELECT * FROM filetable a, fileversiontable b, filelink c, versiontable d " +
            "WHERE(c.fileversionid = b.fileversionid And a.fileid = b.fileid) " +
            "and d.versionid = b.filepackage",
            null);
    }

    public async Task<double> GetTotalBackupFileSizeAsync(DbClient dbClient)
    {
        var result = await dbClient.ExecuteScalarAsync("SELECT SUM(FileSize) FROM fileversiontable");
        if (result == null)
        {
            return 0;
        }

        return double.TryParse(result.ToString(), out var parsed) ? parsed : 0;
    }
}
