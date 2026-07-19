// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types;

namespace Brightbits.BSH.Engine.Repo;

public static class VersionDetailsMapper
{
    public static VersionDetails FromReader(IDataReader reader)
    {
        return new VersionDetails
        {
            Id = reader.GetInt32("versionID").ToString(),
            CreationDate = reader.GetDateTimeParsed("versionDate", "dd-MM-yyyy HH-mm-ss"),
            Title = reader.GetString("versionTitle"),
            Description = reader.GetString("versionDescription"),
            Stable = reader.GetBoolean(reader.GetOrdinal("versionStable")),
            Sources = reader.GetString("versionSources")
        };
    }

    public static VersionDetails FromReaderDetailed(IDataReader reader)
    {
        var result = FromReader(reader);

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
