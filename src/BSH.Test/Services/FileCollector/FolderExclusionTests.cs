// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Service.FileCollector;
using NUnit.Framework;

namespace BSH.Test.Services.FileCollector;

public class FolderExclusionTests
{
    private IDbClientFactory dbClientFactory;
    private string tempRoot;

    [SetUp]
    public async Task Setup()
    {
        DbClientFactory.ClosePool();

        if (File.Exists("testdb_folderexclusion.db"))
        {
            File.Delete("testdb_folderexclusion.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Path.Combine(Environment.CurrentDirectory, "testdb_folderexclusion.db"));

        tempRoot = Path.Combine(Path.GetTempPath(), "bsh3-folder-exclusion-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
    }

    [TearDown]
    public void Cleanup()
    {
        DbClientFactory.ClosePool();
        if (File.Exists("testdb_folderexclusion.db"))
        {
            File.Delete("testdb_folderexclusion.db");
        }

        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task PathFolderExclusion_ReturnsFalse_WhenExcludeFolderEmpty()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeFolder = "";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "docs"));

        var exclusion = new PathFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.False);
    }

    [Test]
    public async Task PathFolderExclusion_FiltersMatchingFolderName()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeFolder = @"\Temp";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "Temp"));

        var exclusion = new PathFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.True);
    }

    [Test]
    public async Task PathFolderExclusion_DoesNotFilterUnrelatedFolder()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeFolder = @"\Temp";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "Documents"));

        var exclusion = new PathFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.False);
    }

    [Test]
    public async Task MaskFolderExclusion_ReturnsFalse_WhenExcludeMaskEmpty()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeMask = "";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "docs"));

        var exclusion = new MaskFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.False);
    }

    [Test]
    public async Task MaskFolderExclusion_FiltersMatchingRegex()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeMask = ".*cache.*";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "app-cache"));

        var exclusion = new MaskFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.True);
    }

    [Test]
    public async Task MaskFolderExclusion_DoesNotFilterNonMatchingPath()
    {
        var configurationManager = await CreateConfigurationAsync();
        configurationManager.ExcludeMask = ".*cache.*";
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "documents"));

        var exclusion = new MaskFolderExclusion(configurationManager);
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.False);
    }

    [Test]
    public void TemporaryFolderExclusion_FiltersTemporaryAttribute()
    {
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "tmp-attr"));
        folder.Attributes |= FileAttributes.Temporary;
        folder.Refresh();

        var exclusion = new TemporaryFolderExclusion();
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.True);

        folder.Attributes &= ~FileAttributes.Temporary;
        folder.Refresh();
        Assert.That(exclusion.IsFolderFiltered(tempRoot, folder), Is.False);
    }

    [Test]
    public void ReparsePointFolderExclusion_ReturnsFalse_ForNormalFolder()
    {
        var folder = Directory.CreateDirectory(Path.Combine(tempRoot, "reparse"));
        Assert.That(new ReparsePointFolderExclusion().IsFolderFiltered(tempRoot, folder), Is.False);
    }

    private async Task<IConfigurationManager> CreateConfigurationAsync()
    {
        var configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();
        return configurationManager;
    }
}
