// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine.Repo.Database;

namespace Brightbits.BSH.Engine.Repo.Contracts;

public interface IDbClientFactory
{
    string DatabaseFile
    {
        get;
    }

    DbClient CreateDbClient();

    Task InitializeAsync(string databaseFile);

    Task ExecuteNonQueryAsync(string query);
}