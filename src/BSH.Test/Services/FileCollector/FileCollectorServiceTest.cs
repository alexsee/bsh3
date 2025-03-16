// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Services;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Services.FileCollector;
using Brightbits.BSH.Engine.Utils;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test.Services.FileCollector;
public class FileCollectorServiceTest
{
    private IDbClientFactory dbClientFactory;
    private IConfigurationManager configurationManager;
    private IFileCollectorService fileCollectorService;

    private string root;

    [SetUp]
    public async Task Setup()
    {
        if (dbClientFactory != null)
        {
            DbClientFactory.ClosePool();
            this.dbClientFactory = null;
        }

        // start with clean database
        if (File.Exists("testdb.db"))
        {
            File.Delete("testdb.db");
        }

        dbClientFactory = new DbClientFactory();
        await dbClientFactory.InitializeAsync(Environment.CurrentDirectory + "\\testdb.db");

        configurationManager = new ConfigurationManager(dbClientFactory);
        await configurationManager.InitializeAsync();

        // setup file collector
        root = "D:\\Meine Dokumente";
        fileCollectorService = new FileCollectorServiceMock(
            [],
            [
                new()
                {
                    FileName = "test_1.txt",
                    FilePath = IOUtils.GetRelativeFolder("D:\\Meine Dokumente",root),
                    FileRoot = "D:\\Meine Dokumente",
                    FileSize = 1024,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                },
                new()
                {
                    FileName = "test_2.docx",
                    FilePath = IOUtils.GetRelativeFolder("D:\\Meine Dokumente\\Sub directory", root),
                    FileRoot = "D:\\Meine Dokumente",
                    FileSize = 1024 * 20,
                    FileDateCreated = DateTime.Now,
                    FileDateModified = DateTime.Now,
                }
            ]);
    }

    [Test]
    public void TestPathFileExclusion()
    {
        fileCollectorService.FileExclusionHandlers.Add(new PathFileExclusion(configurationManager));

        // check files - non excluded
        var result = fileCollectorService.GetLocalFileList("D:\\Meine Dokumente");
        Assert.AreEqual(2, result.Count);

        // check files - excluded
        configurationManager.ExcludeFolder = "\\Meine Dokumente\\Sub directory";
        result = fileCollectorService.GetLocalFileList("D:\\Meine Dokumente");
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void TestTypeFileExclusion()
    {
        fileCollectorService.FileExclusionHandlers.Add(new TypeFileExclusion(configurationManager));

        // check files - non excluded
        var result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(2, result.Count);

        // check files - excluded
        configurationManager.ExcludeFileTypes = "docx";
        result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void TestSizeFileExclusion()
    {
        fileCollectorService.FileExclusionHandlers.Add(new SizeFileExclusion(configurationManager));

        // check files - non excluded
        var result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(2, result.Count);

        // check files - excluded
        configurationManager.ExcludeFileBigger = (1024 * 10).ToString();
        result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void TestNameFileExclusion()
    {
        fileCollectorService.FileExclusionHandlers.Add(new NameFileExclusion(configurationManager));

        // check files - non excluded
        var result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(2, result.Count);

        // check files - excluded
        configurationManager.ExcludeFile = "\\Meine Dokumente\\Sub directory\\test_2.docx";
        result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(1, result.Count);
    }

    [Test]
    public void TestMaskFileExclusion()
    {
        fileCollectorService.FileExclusionHandlers.Add(new MaskFileExclusion(configurationManager));

        // check files - non excluded
        var result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(2, result.Count);

        // check files - excluded
        configurationManager.ExcludeMask = ".*\\.docx";
        result = fileCollectorService.GetLocalFileList(root);
        Assert.AreEqual(1, result.Count);
    }
}
