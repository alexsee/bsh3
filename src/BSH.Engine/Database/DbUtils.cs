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
using System.Globalization;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// Class for some database helper methods
/// </summary>
public static class DbUtils
{
    /// <summary>
    /// Gets the string of a column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static string GetString(this IDataReader reader, string column)
    {
        int index = reader.GetOrdinal(column);

        if (reader.IsDBNull(index))
        {
            return null;
        }

        return reader.GetString(index);
    }

    /// <summary>
    /// Gets the int of a column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static int GetInt32(this IDataReader reader, string column)
    {
        int index = reader.GetOrdinal(column);
        return reader.GetInt32(index);
    }

    /// <summary>
    /// Gets the double of a column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static double GetDouble(this IDataReader reader, string column)
    {
        int index = reader.GetOrdinal(column);
        return reader.GetDouble(index);
    }

    /// <summary>
    /// Gets the datetime of a column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static DateTime GetDateTime(this IDataReader reader, string column)
    {
        int index = reader.GetOrdinal(column);
        return reader.GetDateTime(index);
    }

    /// <summary>
    /// Gets the datetime of a string column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeParsed(this IDataReader reader, string column, string format)
    {
        string value = GetString(reader, column);
        return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the datetime of a string column from a IDatabaseReader.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeParsed(this IDataReader reader, string column)
    {
        string value = GetString(reader, column);
        return DateTime.Parse(value);
    }
}
