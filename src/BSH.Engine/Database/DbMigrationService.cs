// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Exceptions;

namespace Brightbits.BSH.Engine.Database;

public class DbMigrationService : IDbMigrationService
{
    private readonly IDbClientFactory dbClientFactory;
    private readonly IConfigurationManager configurationManager;

    public DbMigrationService(IDbClientFactory dbClientFactory, IConfigurationManager configurationManager)
    {
        this.dbClientFactory = dbClientFactory;
        this.configurationManager = configurationManager;
    }

    public async Task InitializeAsync()
    {
        using var dbClient = dbClientFactory.CreateDbClient();

        // no version at all
        if (string.IsNullOrEmpty(configurationManager.DBVersion))
        {
            configurationManager.DBVersion = "1";
        }

        // check if we have a higher db version than supported
        var dbVersion = int.Parse(configurationManager.DBVersion);

        if (dbVersion > 9)
        {
            throw new DatabaseIncompatibleException();
        }

        // Version 1 auf 2 aktualisieren
        if (configurationManager.DBVersion == "1")
        {
            await dbClient.ExecuteNonQueryAsync("ALTER TABLE versiontable ADD versionSources TEXT");
            await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderjunctiontable (junction TEXT PRIMARY KEY, folder TEXT);");
            configurationManager.DBVersion = "2";
        }

        // Version 2 auf 3 aktualisieren
        if (configurationManager.DBVersion == "2")
        {
            await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS fileHash");
            await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS filePath");
            await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS fileName");

            await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX fileTableIndex ON filetable (fileName, filePath)");
            await dbClient.ExecuteNonQueryAsync("CREATE INDEX fileverstionIndex ON fileversiontable (fileSize, fileDateModified)");
            configurationManager.DBVersion = "3";
        }

        // Version 3 auf 4 aktualisieren
        if (configurationManager.DBVersion == "3")
        {
            await dbClient.ExecuteNonQueryAsync("CREATE TABLE foldertable (id INTEGER PRIMARY KEY, folder TEXT)");
            await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderlink (folderid NUMERIC, versionid NUMERIC)");
            await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX folderTableIndex ON foldertable (folder ASC)");
            configurationManager.DBVersion = "4";
        }

        // Version 4 auf 5 aktualisieren
        if (configurationManager.DBVersion == "4")
        {
            using (var dbClient2 = dbClientFactory.CreateDbClient())
            {
                dbClient2.BeginTransaction();

                using (var sqlRead = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT fileversionid, filedatecreated, filedatemodified FROM fileversiontable", null))
                {
                    while (await sqlRead.ReadAsync())
                    {
                        var parameters = new (string, object)[] {
                            ("fileversionid", sqlRead.GetInt32(sqlRead.GetOrdinal("fileversionid"))),
                            ("filedatecreated", sqlRead.GetDateTime(sqlRead.GetOrdinal("filedatecreated"))),
                            ("filedatemodified", sqlRead.GetDateTime(sqlRead.GetOrdinal("filedatemodified")))
                        };

                        await dbClient2.ExecuteNonQueryAsync(CommandType.Text, "UPDATE fileversiontable SET filedatecreated = @filedatecreated, filedatemodified = @filedatemodified WHERE fileversionid = @fileversionid", parameters);
                    }

                    await sqlRead.CloseAsync();
                }

                dbClient2.CommitTransaction();
            }

            configurationManager.DBVersion = "5";
        }

        // Version 5 auf 6 aktualisieren
        if (configurationManager.DBVersion == "5")
        {
            await dbClient.ExecuteNonQueryAsync("ALTER TABLE fileversiontable ADD longfilename TEXT");
            configurationManager.DBVersion = "6";
        }

        // Version 6 auf 7 aktualisieren
        if (configurationManager.DBVersion == "6")
        {
            await dbClient.ExecuteNonQueryAsync("CREATE INDEX filePackageIndex ON fileversiontable (filePackage)");
            configurationManager.DBVersion = "7";
        }

        // Version 7 auf 8 aktualisieren
        if (configurationManager.DBVersion == "7")
        {
            // set timezone
            await dbClient.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileDateModified = fileDateModified || \"Z\", fileDateCreated = fileDateCreated || \"Z\" WHERE fileDateModified NOT LIKE \"%Z\"");
            configurationManager.DBVersion = "8";
        }

        // Version 8 auf 9 aktualisieren
        if (configurationManager.DBVersion == "8")
        {
            await dbClient.ExecuteNonQueryAsync("PRAGMA journal_mode=WAL;");
            configurationManager.DBVersion = "9";
        }
    }
}
