// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Models;

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