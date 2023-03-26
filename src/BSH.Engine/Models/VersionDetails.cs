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
using System.Data;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Models
{
    public class VersionDetails
    {
        public string Id
        {
            get; set;
        }

        public DateTime CreationDate
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public bool Stable
        {
            get; set;
        }

        public string Sources
        {
            get; set;
        }

        public long Size
        {
            get; set;
        }

        public static VersionDetails FromReader(IDataReader reader)
        {
            var result = new VersionDetails
            {
                Id = reader.GetInt32("versionID").ToString(),
                CreationDate = reader.GetDateTimeParsed("versionDate", "dd-MM-yyyy HH-mm-ss"),
                Title = reader.GetString("versionTitle"),
                Description = reader.GetString("versionDescription"),
                Stable = reader.GetBoolean(reader.GetOrdinal("versionStable")),
                Sources = reader.GetString("versionSources")
            };

            return result;
        }

        public static VersionDetails FromReaderDetailed(IDataReader reader)
        {
            var result = new VersionDetails
            {
                Id = reader.GetInt32("versionID").ToString(),
                CreationDate = reader.GetDateTimeParsed("versionDate", "dd-MM-yyyy HH-mm-ss"),
                Title = reader.GetString("versionTitle"),
                Description = reader.GetString("versionDescription"),
                Stable = reader.GetBoolean(reader.GetOrdinal("versionStable")),
                Sources = reader.GetString("versionSources")
            };

            try
            {
                result.Size = (long)reader.GetDouble(reader.GetOrdinal("versionSize"));
            }
            catch
            {
                result.Size = 0;
            }

            return result;
        }
    }
}