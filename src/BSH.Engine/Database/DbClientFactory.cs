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

namespace Brightbits.BSH.Engine.Database
{
    /// <summary>
    /// Class for creating DbClients
    /// </summary>
    public class DbClientFactory
    {
        private readonly string databaseFile;

        public string DatabaseFile { get => databaseFile; }

        public DbClientFactory(string databaseFile)
        {
            this.databaseFile = databaseFile;

            DbProviderFactories.RegisterFactory("System.Data.SQLite", System.Data.SQLite.SQLiteFactory.Instance);
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
            System.Data.SQLite.SQLiteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
