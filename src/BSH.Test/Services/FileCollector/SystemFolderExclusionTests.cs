// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Services.FileCollector;
using NUnit.Framework;

namespace BSH.Test.Services.FileCollector;

public class SystemFolderExclusionTests
{
    private IDbClientFactory dbClientFactory;
    private string tempRoot;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();

        if (File.Exists("testdb_systemfolderexclusion.db"))
        {
            File.Delete("testdb_systemfolderexclusion.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_systemfolderexclusion.db"));

        tempRoot = Path.Combine(Path.GetTempPath(), "bsh3-system-folder-exclusion-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_systemfolderexclusion.db"))
        {
            File.Delete("testdb_systemfolderexclusion.db");
        }

        ClearSystemAttributesAndDelete(tempRoot);
    }

    [Test]
    public async Task IncludeSystemFolders_DefaultsToOff()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.IncludeSystemFolders, Is.EqualTo("0"));
    }

    [Test]
    public async Task IncludeSystemFolders_PersistsWhenEnabled()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        configurationManager.IncludeSystemFolders = "1";

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        Assert.That(configurationManager.IncludeSystemFolders, Is.EqualTo("1"));
    }

    [Test]
    public async Task SystemFolderExclusion_ExcludesSystemFoldersByDefault()
    {
        var configurationManager = await CreateConfigurationAsync();
        var systemFolder = CreateSystemFolder("system-default");

        var exclusion = new SystemFolderExclusion(configurationManager);

        Assert.That(exclusion.IsFolderFiltered(tempRoot, systemFolder), Is.True);
    }

    [Test]
    public async Task SystemFolderExclusion_IncludesSystemFoldersWhenEnabled()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.IncludeSystemFolders = "1";
        var systemFolder = CreateSystemFolder("system-included");

        var exclusion = new SystemFolderExclusion(configurationManager);

        Assert.That(exclusion.IsFolderFiltered(tempRoot, systemFolder), Is.False);
    }

    [Test]
    public async Task SystemFolderExclusion_DoesNotExcludeNormalFolders()
    {
        var configurationManager = await CreateConfigurationAsync();
        var normalFolder = new DirectoryInfo(Path.Combine(tempRoot, "normal"));
        normalFolder.Create();

        var exclusion = new SystemFolderExclusion(configurationManager);

        Assert.That(exclusion.IsFolderFiltered(tempRoot, normalFolder), Is.False);
    }

    private async Task<IConfigurationManager> CreateConfigurationAsync()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        return configurationManager;
    }

    private DirectoryInfo CreateSystemFolder(string name)
    {
        var folder = new DirectoryInfo(Path.Combine(tempRoot, name));
        folder.Create();
        folder.Attributes |= FileAttributes.System;
        folder.Refresh();
        return folder;
    }

    private static void ClearSystemAttributesAndDelete(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        try
        {
            var info = new DirectoryInfo(path);
            info.Attributes &= ~(FileAttributes.System | FileAttributes.Hidden);
            foreach (var child in info.GetDirectories("*", SearchOption.AllDirectories))
            {
                child.Attributes &= ~(FileAttributes.System | FileAttributes.Hidden);
            }

            Directory.Delete(path, true);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
