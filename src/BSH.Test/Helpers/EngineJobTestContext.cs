// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Repo;
using Brightbits.BSH.Engine.Storage;

namespace BSH.Test.Helpers;

/// <summary>
/// Owns a unique temporary SQLite database and wired engine collaborators for job tests.
/// </summary>
public sealed class EngineJobTestContext : IAsyncDisposable
{
    public string RootDir { get; }

    public string DbPath { get; }

    public IDbClientFactory DbFactory { get; private set; }

    public IConfigurationManager ConfigurationManager { get; private set; }

    public IQueryManager QueryManager { get; private set; }

    public IVersionQueryRepository VersionQueryRepository { get; private set; }

    public IBackupMutationRepository BackupMutationRepository { get; private set; }

    private EngineJobTestContext(string rootDir, string dbPath)
    {
        RootDir = rootDir;
        DbPath = dbPath;
    }

    public static async Task<EngineJobTestContext> CreateAsync(string namePrefix = "engine")
    {
        var rootDir = Path.Combine(Path.GetTempPath(), "BSH.Test", $"{namePrefix}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootDir);
        var dbPath = Path.Combine(rootDir, "testdb.db");

        var context = new EngineJobTestContext(rootDir, dbPath);
        await context.InitializeAsync();
        return context;
    }

    private async Task InitializeAsync()
    {
        DbClientFactory.ClosePool();

        DbFactory = new DbClientFactory();
        await DbFactory.InitializeAsync(DbPath);

        ConfigurationManager = new ConfigurationManager(DbFactory);
        await ConfigurationManager.InitializeAsync();
        ConfigurationManager.OldBackupPrevent = "0";
        ConfigurationManager.MediaVolumeSerial = "";

        VersionQueryRepository = new VersionQueryRepository();
        BackupMutationRepository = new BackupMutationRepository(DbFactory);

        var storageFactory = new StorageFactory(ConfigurationManager);
        QueryManager = new QueryManager(DbFactory, ConfigurationManager, storageFactory);
    }

    public async ValueTask DisposeAsync()
    {
        DbClientFactory.ClosePool();

        try
        {
            if (Directory.Exists(RootDir))
            {
                Directory.Delete(RootDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup when Windows still holds file locks.
        }

        await Task.CompletedTask;
    }
}
