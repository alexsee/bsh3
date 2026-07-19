// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Service.FileCollector;
using NUnit.Framework;

namespace BSH.Test.Services.FileCollector;

public class FileCollectorServiceRealTests
{
    private string tempRoot;

    [SetUp]
    public void SetUp()
    {
        tempRoot = Path.Combine(Path.GetTempPath(), "bsh-filecollector-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        Directory.CreateDirectory(Path.Combine(tempRoot, "sub"));
        File.WriteAllText(Path.Combine(tempRoot, "a.txt"), "a");
        File.WriteAllText(Path.Combine(tempRoot, "sub", "b.txt"), "b");
        Directory.CreateDirectory(Path.Combine(tempRoot, "empty"));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public void GetLocalFileList_FindsFilesAndEmptyFolders()
    {
        var collector = new FileCollectorService();
        var files = collector.GetLocalFileList(tempRoot, subFolders: true);

        Assert.That(files.Count, Is.EqualTo(2));
        Assert.That(files.Exists(f => f.FileName == "a.txt"), Is.True);
        Assert.That(files.Exists(f => f.FileName == "b.txt"), Is.True);
        Assert.That(collector.EmptyFolders.Exists(f => f.Folder.EndsWith("empty", StringComparison.OrdinalIgnoreCase)), Is.True);
    }

    [Test]
    public void GetLocalFileList_RespectsSubFoldersFlag()
    {
        var collector = new FileCollectorService();
        var files = collector.GetLocalFileList(tempRoot, subFolders: false);
        Assert.That(files.Count, Is.EqualTo(1));
        Assert.That(files[0].FileName, Is.EqualTo("a.txt"));
    }

    [Test]
    public void FileCollectorServiceFactory_CreatesService()
    {
        var factory = new FileCollectorServiceFactory();
        Assert.That(factory.Create(), Is.TypeOf<FileCollectorService>());
    }
}
