// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;

namespace BSH.Test.Helpers;

/// <summary>
/// Builds on-disk backup databases that match historical schemas inferred from
/// <see cref="DbMigrationService"/>, so upgrade tests open real prior-version files.
/// </summary>
public static class LegacyBackupDatabaseBuilder
{
    public const string SourceFolder = @"C:\Users\alex\Documents";

    public const string BackupFolder = @"D:\Backups";

    public const string VersionDate = "01-01-2021 00-00-00";

    public const string VersionTitle = "Initial full version";

    public const string VersionDescription = "Schema upgrade fixture";

    public const string CurrentSchemaVersion = "9";

    public static async Task CreateAsync(string databaseFile, string schemaVersion)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databaseFile)!);
        await File.WriteAllBytesAsync(databaseFile, []);

        var dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(databaseFile);

        if (schemaVersion == "1")
        {
            await ApplyVersion1SchemaAsync(dbClientFactory);
        }
        else
        {
            await ApplyVersion8SchemaAsync(dbClientFactory, schemaVersion);
        }

        DbClientFactory.ClosePool();
    }

    private static async Task CreateBaseTablesAsync(IDbClientFactory dbClientFactory)
    {
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE configuration (confProperty NVARCHAR(20) PRIMARY KEY,confValue NVARCHAR(255));");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE filelink (fileversionID INTEGER, versionID INTEGER);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE filetable (fileID INTEGER PRIMARY KEY, fileName TEXT, filePath TEXT);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE schedule (timType INT,timDate TEXT);");
    }

    private static async Task CreateLegacyFileIndexesAsync(IDbClientFactory dbClientFactory)
    {
        await dbClientFactory.ExecuteNonQueryAsync("CREATE INDEX fileHash ON fileversiontable(fileHash ASC);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE INDEX fileName ON filetable(fileName);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE INDEX filePath ON filetable(filePath);");
    }

    private static async Task ApplyVersion1SchemaAsync(IDbClientFactory dbClientFactory)
    {
        await CreateBaseTablesAsync(dbClientFactory);
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE fileversiontable (fileversionID INTEGER PRIMARY KEY, fileStatus INTEGER, fileType INTEGER, fileHash VARCHAR(255), fileDateModified DATE, fileDateCreated DATE, fileSize DOUBLE, filePackage INTEGER, fileID INTEGER);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE versiontable (versionID INTEGER PRIMARY KEY AUTOINCREMENT,versionDate VARCHAR(255),versionTitle VARCHAR(255),versionDescription TEXT,versionType INT,versionStatus INT,versionStable INT);");
        await CreateLegacyFileIndexesAsync(dbClientFactory);

        await SeedConfigurationAsync(dbClientFactory, "1");
        await SeedVersion1ContentsAsync(dbClientFactory);
    }

    private static async Task ApplyVersion8SchemaAsync(IDbClientFactory dbClientFactory, string schemaVersion)
    {
        await dbClientFactory.ExecuteNonQueryAsync("PRAGMA journal_mode=DELETE;");
        await dbClientFactory.ExecuteNonQueryAsync("PRAGMA page_size=4096;");
        await CreateBaseTablesAsync(dbClientFactory);
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE fileversiontable (fileversionID INTEGER PRIMARY KEY, fileStatus INTEGER, fileType INTEGER, fileHash VARCHAR(255), fileDateModified DATE, fileDateCreated DATE, fileSize DOUBLE, filePackage INTEGER, fileID INTEGER, longfilename TEXT);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE versiontable (versionID INTEGER PRIMARY KEY AUTOINCREMENT,versionDate VARCHAR(255),versionTitle VARCHAR(255),versionDescription TEXT,versionType INT,versionStatus INT,versionStable INT,versionSources TEXT);");
        await CreateLegacyFileIndexesAsync(dbClientFactory);
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE folderjunctiontable (junction TEXT PRIMARY KEY, folder TEXT);");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE UNIQUE INDEX fileTableIndex ON filetable (fileName, filePath)");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE INDEX fileverstionIndex ON fileversiontable (fileSize, fileDateModified)");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE foldertable (id INTEGER PRIMARY KEY, folder TEXT)");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE TABLE folderlink (folderid NUMERIC, versionid NUMERIC)");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE UNIQUE INDEX folderTableIndex ON foldertable (folder ASC)");
        await dbClientFactory.ExecuteNonQueryAsync("CREATE INDEX filePackageIndex ON fileversiontable (filePackage)");

        await SeedConfigurationAsync(dbClientFactory, schemaVersion);
        await SeedVersion8ContentsAsync(dbClientFactory);
    }

    private static async Task SeedConfigurationAsync(IDbClientFactory dbClientFactory, string schemaVersion)
    {
        await dbClientFactory.ExecuteNonQueryAsync($"INSERT INTO configuration VALUES ('dbversion', '{schemaVersion}');");
        await dbClientFactory.ExecuteNonQueryAsync($"INSERT INTO configuration VALUES ('sourcefolder', '{SourceFolder}');");
        await dbClientFactory.ExecuteNonQueryAsync($"INSERT INTO configuration VALUES ('backupfolder', '{BackupFolder}');");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO configuration VALUES ('isconfigured', '1');");
    }

    private static async Task SeedVersion1ContentsAsync(IDbClientFactory dbClientFactory)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable) " +
            $"VALUES (1, '{VersionDate}', '{VersionTitle}', '{VersionDescription}', 2, 0, 1)");
        await SeedFileTableAndLinkAsync(dbClientFactory);
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID) " +
            "VALUES (1, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, 1)");
    }

    private static async Task SeedVersion8ContentsAsync(IDbClientFactory dbClientFactory)
    {
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO versiontable (versionID, versionDate, versionTitle, versionDescription, versionType, versionStatus, versionStable, versionSources) " +
            $"VALUES (1, '{VersionDate}', '{VersionTitle}', '{VersionDescription}', 2, 0, 1, '{SourceFolder}')");
        await SeedFileTableAndLinkAsync(dbClientFactory);
        await dbClientFactory.ExecuteNonQueryAsync(
            "INSERT INTO fileversiontable (fileversionID, fileStatus, fileType, fileHash, fileDateModified, fileDateCreated, fileSize, filePackage, fileID, longfilename) " +
            "VALUES (1, 0, 1, 'hash1', '2021-01-01 00:00:00', '2021-01-01 00:00:00', 100, 1, 1, NULL)");
    }

    private static async Task SeedFileTableAndLinkAsync(IDbClientFactory dbClientFactory)
    {
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filetable (fileID, fileName, filePath) VALUES (1, 'notes.txt', '\\Documents\\')");
        await dbClientFactory.ExecuteNonQueryAsync("INSERT INTO filelink (fileversionID, versionID) VALUES (1, 1)");
    }
}
