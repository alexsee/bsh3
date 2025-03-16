// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
        ArgumentNullException.ThrowIfNull(reader);

        var index = reader.GetOrdinal(column);

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
        ArgumentNullException.ThrowIfNull(reader);

        var index = reader.GetOrdinal(column);
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
        ArgumentNullException.ThrowIfNull(reader);

        var index = reader.GetOrdinal(column);
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
        ArgumentNullException.ThrowIfNull(reader);

        var index = reader.GetOrdinal(column);
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
        ArgumentNullException.ThrowIfNull(reader);

        var value = GetString(reader, column);
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
        ArgumentNullException.ThrowIfNull(reader);

        var value = GetString(reader, column);
        return DateTime.Parse(value);
    }
}
