// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Contracts.Database;

public interface IDbMigrationService
{
    Task InitializeAsync();
}