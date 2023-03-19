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

using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Services
{
    public class EngineService
    {
        public string DatabaseFile { get; set; }

        public DbClientFactory DbClientFactory { get; set; }

        public QueryManager QueryManager { get; set; }

        public ConfigurationManager ConfigurationManager { get; set; }

        public BackupService BackupService { get; set; }

        public EngineService(string databaseFile)
        {
            // set the database file
            this.DatabaseFile = databaseFile;
            this.DbClientFactory = new DbClientFactory(this.DatabaseFile);
        }

        public async Task InitAsync()
        {
            // check database existence
            if (!File.Exists(this.DatabaseFile))
            {
                await CreateDatabaseAsync();
            }

            // load engine system
            this.ConfigurationManager = new ConfigurationManager(DbClientFactory);
            await this.ConfigurationManager.LoadConfigurationAsync();

            this.QueryManager = new QueryManager(DbClientFactory, ConfigurationManager);

            await UpdateDbVersionAsync();

            // load main system
            this.BackupService = new BackupService(
                ConfigurationManager,
                QueryManager,
                DbClientFactory);
        }

        private async Task UpdateDbVersionAsync()
        {
            using var dbClient = DbClientFactory.CreateDbClient();
            
            // no version at all
            if (string.IsNullOrEmpty(ConfigurationManager.DBVersion))
            {
                ConfigurationManager.DBVersion = "1";
            }

            // check if we have a higher db version than supported
            int dbVersion = int.Parse(ConfigurationManager.DBVersion);

            if (dbVersion > 8)
            {
                throw new DatabaseIncompatibleException();
            }

            // Version 1 auf 2 aktualisieren
            if (ConfigurationManager.DBVersion == "1")
            {
                await dbClient.ExecuteNonQueryAsync("ALTER TABLE versiontable ADD versionSources TEXT");
                await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderjunctiontable (junction TEXT PRIMARY KEY, folder TEXT);");
                ConfigurationManager.DBVersion = "2";
            }

            // Version 2 auf 3 aktualisieren
            if (ConfigurationManager.DBVersion == "2")
            {
                await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS fileHash");
                await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS filePath");
                await dbClient.ExecuteNonQueryAsync("DROP INDEX IF EXISTS fileName");

                await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX fileTableIndex ON filetable (fileName, filePath)");
                await dbClient.ExecuteNonQueryAsync("CREATE INDEX fileverstionIndex ON fileversiontable (fileSize, fileDateModified)");
                ConfigurationManager.DBVersion = "3";
            }

            // Version 3 auf 4 aktualisieren
            if (ConfigurationManager.DBVersion == "3")
            {
                await dbClient.ExecuteNonQueryAsync("CREATE TABLE foldertable (id INTEGER PRIMARY KEY, folder TEXT)");
                await dbClient.ExecuteNonQueryAsync("CREATE TABLE folderlink (folderid NUMERIC, versionid NUMERIC)");
                await dbClient.ExecuteNonQueryAsync("CREATE UNIQUE INDEX folderTableIndex ON foldertable (folder ASC)");
                ConfigurationManager.DBVersion = "4";
            }

            // Version 4 auf 5 aktualisieren
            if (ConfigurationManager.DBVersion == "4")
            {
                using (var dbClient2 = DbClientFactory.CreateDbClient())
                {
                    dbClient2.BeginTransaction();

                    using (var sqlRead = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT fileversionid, filedatecreated, filedatemodified FROM fileversiontable", null))
                    {
                        while (sqlRead.Read())
                        {
                            var parameters = new IDataParameter[] {
                                    dbClient2.CreateParameter("fileversionid", DbType.Int32, 32, sqlRead.GetInt32(sqlRead.GetOrdinal("fileversionid"))),
                                    dbClient2.CreateParameter("filedatecreated", DbType.DateTime, 0,sqlRead.GetDateTime (sqlRead.GetOrdinal("filedatecreated"))),
                                    dbClient2.CreateParameter("filedatemodified", DbType.DateTime, 0, sqlRead.GetDateTime(sqlRead.GetOrdinal("filedatemodified")))
                                };

                            await dbClient2.ExecuteNonQueryAsync(CommandType.Text, "UPDATE fileversiontable SET filedatecreated = @filedatecreated, filedatemodified = @filedatemodified WHERE fileversionid = @fileversionid", parameters);
                        }

                        sqlRead.Close();
                    }

                    dbClient2.CommitTransaction();
                }

                ConfigurationManager.DBVersion = "5";
            }

            // Version 5 auf 6 aktualisieren
            if (ConfigurationManager.DBVersion == "5")
            {
                await dbClient.ExecuteNonQueryAsync("ALTER TABLE fileversiontable ADD longfilename TEXT");
                ConfigurationManager.DBVersion = "6";
            }

            // Version 6 auf 7 aktualisieren
            if (ConfigurationManager.DBVersion == "6")
            {
                await dbClient.ExecuteNonQueryAsync("CREATE INDEX filePackageIndex ON fileversiontable (filePackage)");
                ConfigurationManager.DBVersion = "7";
            }

            // Version 7 auf 8 aktualisieren
            if (ConfigurationManager.DBVersion == "7")
            {
                // set timezone
                await dbClient.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileDateModified = fileDateModified || \"Z\", fileDateCreated = fileDateCreated || \"Z\" WHERE fileDateModified NOT LIKE \"%Z\"");
                ConfigurationManager.DBVersion = "8";
            }
        }

        private async Task CreateDatabaseAsync()
        {
            // generate database file
            SQLiteConnection.CreateFile(DatabaseFile);

            // create tables
            using var dbClient = DbClientFactory.CreateDbClient();
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

        public async Task<int> ExecuteNonQueryAsync(string query)
        {
            using var dbClient = DbClientFactory.CreateDbClient();
            return await dbClient.ExecuteNonQueryAsync(query);
        }
    }
}
