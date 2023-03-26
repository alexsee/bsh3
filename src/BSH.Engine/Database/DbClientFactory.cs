// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// Class for creating DbClients
/// </summary>
public class DbClientFactory : IDbClientFactory
{
    private string databaseFile;

    public string DatabaseFile => databaseFile;

    public DbClientFactory()
    {
        DbProviderFactories.RegisterFactory("System.Data.SQLite", SQLiteFactory.Instance);
    }

    public async Task InitializeAsync(string databaseFile)
    {
        this.databaseFile = databaseFile;

        // check database existence
        if (!File.Exists(this.databaseFile))
        {
            await CreateDatabaseAsync();
        }

    }

    /// <summary>
    /// Creates a new DbClient for database access.
    /// </summary>
    /// <returns>A new DbClient instance.</returns>
    public DbClient CreateDbClient()
    {
        return new DbClient($"Data Source={databaseFile};Pooling=True;Max Pool Size=100;DateTimeKind=Utc");
    }

    /// <summary>
    /// Cleans up all open data pools for the database and runs the garbage collection.
    /// </summary>
    public static void ClosePool()
    {
        SQLiteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private async Task CreateDatabaseAsync()
    {
        // generate database file
        SQLiteConnection.CreateFile(this.databaseFile);

        // create tables
        using var dbClient = this.CreateDbClient();
        await dbClient.ExecuteNonQueryAsync("PRAGMA page_size=4096; CREATE TABLE configuration (confProperty NVARCHAR(20) PRIMARY KEY,confValue NVARCHAR(255));");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE filelink (fileversionID INTEGER, versionID INTEGER);");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE filetable (fileID INTEGER PRIMARY KEY, fileName TEXT, filePath TEXT);");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE fileversiontable (fileversionID INTEGER PRIMARY KEY, fileStatus INTEGER, fileType INTEGER, fileHash VARCHAR(255), fileDateModified DATE, fileDateCreated DATE, fileSize DOUBLE, filePackage INTEGER, fileID INTEGER, longfilename TEXT);");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE schedule (timType INT,timDate TEXT);");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE versiontable (versionID INTEGER PRIMARY KEY AUTOINCREMENT,versionDate VARCHAR(255),versionTitle VARCHAR(255),versionDescription TEXT,versionType INT,versionStatus INT,versionStable INT,versionSources TEXT);");
        await dbClient.ExecuteNonQueryAsync("CREATE INDEX fileHash ON fileversiontable(fileHash ASC); CREATE INDEX fileName ON filetable(fileName); CREATE INDEX filePath ON filetable(filePath);");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderjunctiontable (junction TEXT PRIMARY KEY, folder TEXT);");

        await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX fileTableIndex ON filetable (fileName, filePath)");
        await dbClient.ExecuteNonQueryAsync("CREATE INDEX fileverstionIndex ON fileversiontable (fileSize, fileDateModified)");

        await dbClient.ExecuteNonQueryAsync("CREATE TABLE foldertable (id INTEGER PRIMARY KEY, folder TEXT)");
        await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderlink (folderid NUMERIC, versionid NUMERIC)");
        await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX folderTableIndex ON foldertable (folder ASC)");

        await dbClient.ExecuteNonQueryAsync("CREATE INDEX filePackageIndex ON fileversiontable (filePackage)");

        await dbClient.ExecuteNonQueryAsync("INSERT INTO configuration VALUES (\"dbversion\", \"8\")");
    }

    public async Task ExecuteNonQueryAsync(string query)
    {
        using var dbClient = this.CreateDbClient();
        await dbClient.ExecuteNonQueryAsync(query);
    }
}
