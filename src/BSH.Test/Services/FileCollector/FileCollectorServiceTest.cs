// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services.FileCollector;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test.Services.FileCollector;
public class FileExclusionTests
{
    private FakeConfigurationManager configurationManager;

    [SetUp]
    public void Setup()
    {
        configurationManager = new FakeConfigurationManager();
    }

    [Test]
    public void PathFileExclusionMatchesConfiguredFolder()
    {
        configurationManager.ExcludeFolder = @"\Meine Dokumente\Sub directory";
        var file = CreateWindowsFile("test.docx", @"Sub directory");
        var otherFile = CreateWindowsFile("test.docx", @"Other directory");

        Assert.That(new PathFileExclusion(configurationManager).IsFileExcluded(file), Is.True);
        Assert.That(new PathFileExclusion(configurationManager).IsFileExcluded(otherFile), Is.False);
    }

    [Test]
    public void TypeFileExclusionMatchesConfiguredExtension()
    {
        configurationManager.ExcludeFileTypes = "docx";
        var file = CreateWindowsFile("test.docx");
        var otherFile = CreateWindowsFile("test.txt");

        Assert.That(new TypeFileExclusion(configurationManager).IsFileExcluded(file), Is.True);
        Assert.That(new TypeFileExclusion(configurationManager).IsFileExcluded(otherFile), Is.False);
    }

    [Test]
    public void SizeFileExclusionMatchesConfiguredLimit()
    {
        configurationManager.ExcludeFileBigger = "10240";
        var file = CreateWindowsFile("test.bin", fileSize: 10241);
        var otherFile = CreateWindowsFile("test.bin", fileSize: 10240);

        Assert.That(new SizeFileExclusion(configurationManager).IsFileExcluded(file), Is.True);
        Assert.That(new SizeFileExclusion(configurationManager).IsFileExcluded(otherFile), Is.False);
    }

    [Test]
    public void NameFileExclusionMatchesConfiguredPath()
    {
        configurationManager.ExcludeFile = @"\Meine Dokumente\Sub directory\test.docx";
        var file = CreateWindowsFile("test.docx", @"Sub directory");
        var otherFile = CreateWindowsFile("other.docx", @"Sub directory");

        Assert.That(new NameFileExclusion(configurationManager).IsFileExcluded(file), Is.True);
        Assert.That(new NameFileExclusion(configurationManager).IsFileExcluded(otherFile), Is.False);
    }

    [Test]
    public void MaskFileExclusionMatchesConfiguredPattern()
    {
        configurationManager.ExcludeMask = @".*\.docx";
        var file = CreateWindowsFile("test.docx");
        var otherFile = CreateWindowsFile("test.txt");

        Assert.That(new MaskFileExclusion(configurationManager).IsFileExcluded(file), Is.True);
        Assert.That(new MaskFileExclusion(configurationManager).IsFileExcluded(otherFile), Is.False);
    }

    private static FileTableRow CreateWindowsFile(string fileName, string filePath = "", double fileSize = 1024)
    {
        return new FileTableRow
        {
            FileName = fileName,
            FilePath = filePath,
            FileRoot = @"D:\Meine Dokumente",
            FileSize = fileSize,
        };
    }
}

public class FileCollectorServiceTraversalTests
{
    private FakeConfigurationManager configurationManager;
    private FileCollectorService fileCollectorService;
    private string root;

    [SetUp]
    public void Setup()
    {
        configurationManager = new FakeConfigurationManager();
        root = Path.Combine(Path.GetTempPath(), "bsh-file-collector-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        fileCollectorService = new FileCollectorService();
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void GetLocalFileListTraversesNestedFiles()
    {
        File.WriteAllText(Path.Combine(root, "root.txt"), "root");
        var nested = Directory.CreateDirectory(Path.Combine(root, "nested", "deeper"));
        File.WriteAllText(Path.Combine(nested.FullName, "nested.txt"), "nested");

        var result = fileCollectorService.GetLocalFileList(root);

        Assert.That(result.Select(file => file.FileName), Is.EquivalentTo(["root.txt", "nested.txt"]));
        Assert.That(result.Single(file => file.FileName == "nested.txt").FilePath, Does.EndWith(Path.Combine("nested", "deeper")));
    }

    [Test]
    public void GetLocalFileListAppliesFileExclusions()
    {
        File.WriteAllText(Path.Combine(root, "keep.txt"), "keep");
        File.WriteAllText(Path.Combine(root, "skip.tmp"), "skip");
        configurationManager.ExcludeFileTypes = "tmp";
        fileCollectorService.FileExclusionHandlers.Add(new TypeFileExclusion(configurationManager));

        var result = fileCollectorService.GetLocalFileList(root);

        Assert.That(result.Select(file => file.FileName), Is.EqualTo(["keep.txt"]));
    }

    [Test]
    public void GetLocalFileListAppliesFolderExclusions()
    {
        File.WriteAllText(Path.Combine(root, "keep.txt"), "keep");
        var skipped = Directory.CreateDirectory(Path.Combine(root, "skipped"));
        File.WriteAllText(Path.Combine(skipped.FullName, "skip.txt"), "skip");
        configurationManager.ExcludeFolder = @"\skipped";
        fileCollectorService.FolderExclusionHandlers.Add(new PathFolderExclusion(configurationManager));

        var result = fileCollectorService.GetLocalFileList(root);

        Assert.That(result.Select(file => file.FileName), Is.EqualTo(["keep.txt"]));
    }

    [Test]
    public void GetLocalFileListTracksEmptyFoldersAndCanSkipRecursion()
    {
        File.WriteAllText(Path.Combine(root, "root.txt"), "root");
        var nested = Directory.CreateDirectory(Path.Combine(root, "nested"));
        File.WriteAllText(Path.Combine(nested.FullName, "nested.txt"), "nested");
        var empty = Directory.CreateDirectory(Path.Combine(root, "empty"));

        var withoutRecursion = fileCollectorService.GetLocalFileList(root, subFolders: false);
        Assert.That(withoutRecursion.Select(file => file.FileName), Is.EqualTo(["root.txt"]));

        var withRecursion = fileCollectorService.GetLocalFileList(root);
        Assert.That(withRecursion.Select(file => file.FileName), Is.EquivalentTo(["root.txt", "nested.txt"]));
        Assert.That(fileCollectorService.EmptyFolders.Select(folder => folder.Folder), Is.EqualTo([empty.FullName]));
    }

    [TestCase(typeof(IOException))]
    [TestCase(typeof(UnauthorizedAccessException))]
    public void GetLocalFileListSkipsSubFolderWhenExclusionThrows(Type failureType)
    {
        File.WriteAllText(Path.Combine(root, "root.txt"), "root");
        var skipped = Directory.CreateDirectory(Path.Combine(root, "skipped"));
        File.WriteAllText(Path.Combine(skipped.FullName, "skip.txt"), "skip");
        fileCollectorService.FolderExclusionHandlers.Add(
            new ThrowingFolderExclusion((Exception)Activator.CreateInstance(failureType, "simulated failure")));

        var result = fileCollectorService.GetLocalFileList(root);

        Assert.That(result.Select(file => file.FileName), Is.EqualTo(["root.txt"]));
    }

    [Test]
    public void GetLocalFileListReturnsEmptyWhenRootIsGone()
    {
        var staleRoot = Path.Combine(root, "stale");
        Directory.CreateDirectory(staleRoot);
        Directory.Delete(staleRoot);

        var result = fileCollectorService.GetLocalFileList(staleRoot);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetLocalFileListSkipsInaccessibleRoot()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Ignore("Directory ACLs can only be tested on Windows.");
        }

        var locked = Directory.CreateDirectory(Path.Combine(root, "locked"));
        SetReadAccess(locked.FullName, deny: true);
        try
        {
            var result = fileCollectorService.GetLocalFileList(locked.FullName);

            Assert.That(result, Is.Empty);
        }
        finally
        {
            SetReadAccess(locked.FullName, deny: false);
        }
    }

    private static void SetReadAccess(string path, bool deny)
    {
        var directory = new DirectoryInfo(path);
        var security = directory.GetAccessControl();
        var rule = new FileSystemAccessRule(
            WindowsIdentity.GetCurrent().User!,
            FileSystemRights.Read,
            AccessControlType.Deny);

        if (deny)
        {
            security.AddAccessRule(rule);
        }
        else
        {
            security.RemoveAccessRule(rule);
        }

        directory.SetAccessControl(security);
    }

    private sealed class ThrowingFolderExclusion(Exception failure) : IFolderExclusion
    {
        public bool IsFolderFiltered(string root, DirectoryInfo directory) => throw failure;
    }
}
