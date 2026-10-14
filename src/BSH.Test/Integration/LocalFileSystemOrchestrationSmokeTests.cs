// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Services.FileCollector;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Models;
using BSH.MainApp.Services;
using BSH.Test.Helpers;
using BSH.Test.Mocks;
using NUnit.Framework;

namespace BSH.Test.Integration;

/// <summary>
/// Verifies that <see cref="SetupService"/> configuration interoperates with local-file-system
/// engine jobs for backup, incremental, query, restore, and delete operations. This test bypasses
/// WinUI view models and XAML; see <c>docs/testing/winui-golden-path.md</c> for shell validation.
/// </summary>
[Category("Integration")]
public class LocalFileSystemOrchestrationSmokeTests
{
    [Test]
    public async Task SetupAndEngineJobs_RoundTripOnLocalFileSystem()
    {
        await using var context = await EngineJobTestContext.CreateAsync("winui-golden");
        var sourceDir = Path.Combine(context.RootDir, "source");
        var backupDir = Path.Combine(context.RootDir, "backup");
        var restoreTo = Path.Combine(context.RootDir, "restore-to");
        Directory.CreateDirectory(Path.Combine(sourceDir, "nested"));
        Directory.CreateDirectory(Path.Combine(sourceDir, "empty"));
        Directory.CreateDirectory(Path.Combine(sourceDir, "unicode", "äöü"));
        Directory.CreateDirectory(restoreTo);

        await File.WriteAllTextAsync(Path.Combine(sourceDir, "notes.txt"), "notes-v1");
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "drop.txt"), "drop-me");
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "nested", "readme.txt"), "nested-readme");
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "unicode", "äöü", "файл.txt"), "unicode-content");

        var setupService = new SetupService(context.ConfigurationManager, context.QueryManager, context.DbFactory);
        setupService.ApplyNewConfiguration(new NewSetupConfiguration
        {
            SourceFolders = [sourceDir],
            TargetKind = MediaTargetKind.LocalDrive,
            LocalBackupFolder = backupDir,
            MediaVolumeSerial = "",
            TaskType = TaskType.Manual
        });

        Assert.That(context.ConfigurationManager.IsConfigured, Is.EqualTo("1"));
        Assert.That(context.ConfigurationManager.SourceFolder, Is.EqualTo(sourceDir));
        Assert.That(context.ConfigurationManager.BackupFolder, Is.EqualTo(backupDir));

        var fileCollector = new FileCollectorServiceFactory();
        var vssClient = new VssClientMock();
        var sourceLeaf = Path.GetFileName(sourceDir);
        var notesPath = @"\" + sourceLeaf + @"\notes.txt";
        var nestedFolderPath = @"\" + sourceLeaf + @"\nested\";
        var dropFileFilter = "drop.txt";
        var dropPathFilter = @"\" + sourceLeaf + @"\";

        await RunBackupAsync(context, fileCollector, vssClient, "First");
        var version1 = await context.QueryManager.GetLastBackupAsync();
        Assert.That(version1, Is.Not.Null);

        await Task.Delay(1100);
        await File.WriteAllTextAsync(Path.Combine(sourceDir, "notes.txt"), "notes-v2");
        File.SetLastWriteTimeUtc(Path.Combine(sourceDir, "notes.txt"), DateTime.UtcNow.AddMinutes(1));
        await RunBackupAsync(context, fileCollector, vssClient, "Incremental");
        var version2 = await context.QueryManager.GetLastBackupAsync();
        Assert.That(version2.Id, Is.Not.EqualTo(version1.Id));
        Assert.That(context.QueryManager.GetVersions(), Has.Count.EqualTo(2));

        var searchHits = await context.QueryManager.SearchFilesByVersionAsync(version2.Id, "notes");
        Assert.That(searchHits.Select(file => file.FileName), Does.Contain("notes.txt"));

        await File.WriteAllTextAsync(Path.Combine(sourceDir, "notes.txt"), "tampered");
        await RunRestoreAsync(context, int.Parse(version2.Id), notesPath, destination: string.Empty);
        Assert.That(await File.ReadAllTextAsync(Path.Combine(sourceDir, "notes.txt")), Is.EqualTo("notes-v2"));

        await RunRestoreAsync(context, int.Parse(version2.Id), nestedFolderPath, restoreTo);
        Assert.That(await File.ReadAllTextAsync(Path.Combine(restoreTo, "nested", "readme.txt")), Is.EqualTo("nested-readme"));

        var unicodeRestored = Directory.GetFiles(restoreTo, "файл.txt", SearchOption.AllDirectories);
        Assert.That(unicodeRestored, Is.Empty, "Folder restore of nested/ should not pull unicode files.");

        await RunRestoreAsync(context, int.Parse(version2.Id), @"\" + sourceLeaf + @"\unicode\", restoreTo);
        var unicodeFile = Directory.GetFiles(restoreTo, "файл.txt", SearchOption.AllDirectories).SingleOrDefault();
        Assert.That(unicodeFile, Is.Not.Null);
        Assert.That(await File.ReadAllTextAsync(unicodeFile), Is.EqualTo("unicode-content"));

        await RunDeleteVersionAsync(context, version1.Id);
        Assert.That(context.QueryManager.GetVersions(), Has.Count.EqualTo(1));

        await RunDeleteSingleAsync(context, dropFileFilter, dropPathFilter);
        var remaining = Path.Combine(restoreTo, "after-delete");
        Directory.CreateDirectory(remaining);
        await RunRestoreAsync(context, int.Parse(version2.Id), @"\", remaining);

        Assert.That(File.Exists(Path.Combine(remaining, "notes.txt")), Is.True);
        Assert.That(File.Exists(Path.Combine(remaining, "drop.txt")), Is.False);
        Assert.That(Directory.Exists(Path.Combine(remaining, "empty")), Is.True);
    }

    private static async Task RunBackupAsync(
        EngineJobTestContext context,
        FileCollectorServiceFactory fileCollector,
        VssClientMock vssClient,
        string title)
    {
        var backupJob = new BackupJob(
            new FileSystemStorage(context.ConfigurationManager),
            context.DbFactory,
            context.QueryManager,
            context.ConfigurationManager,
            fileCollector,
            vssClient,
            context.VersionQueryRepository,
            context.BackupMutationRepository)
        {
            SourceFolder = context.ConfigurationManager.SourceFolder,
            Title = title,
            Description = "",
        };

        await backupJob.BackupAsync(CancellationToken.None);
        Assert.That(backupJob.FileErrorList, Is.Empty, title + " backup reported file errors.");
        Assert.That(await context.QueryManager.GetLastBackupAsync(), Is.Not.Null);
    }

    private static async Task RunRestoreAsync(
        EngineJobTestContext context,
        int versionId,
        string selectionPath,
        string destination)
    {
        var restoreJob = new RestoreJob(
            new FileSystemStorage(context.ConfigurationManager),
            context.DbFactory,
            context.QueryManager,
            context.ConfigurationManager,
            context.VersionQueryRepository)
        {
            Version = versionId,
            File = selectionPath,
            Destination = destination,
            FileOverwrite = FileOverwrite.Overwrite,
        };

        await restoreJob.RestoreAsync(CancellationToken.None);
        Assert.That(restoreJob.FileErrorList, Is.Empty, "Restore reported file errors for " + selectionPath);
    }

    private static async Task RunDeleteVersionAsync(EngineJobTestContext context, string versionId)
    {
        var deleteJob = new DeleteJob(
            new FileSystemStorage(context.ConfigurationManager),
            context.DbFactory,
            context.QueryManager,
            context.ConfigurationManager,
            context.VersionQueryRepository,
            context.BackupMutationRepository)
        {
            Version = versionId,
        };

        await deleteJob.DeleteAsync();
        Assert.That(deleteJob.FileErrorList, Is.Empty, "Delete version reported file errors.");
    }

    private static async Task RunDeleteSingleAsync(EngineJobTestContext context, string fileFilter, string pathFilter)
    {
        var deleteJob = new DeleteSingleJob(
            new FileSystemStorage(context.ConfigurationManager),
            context.DbFactory,
            context.QueryManager,
            context.ConfigurationManager,
            context.VersionQueryRepository,
            context.BackupMutationRepository);

        await deleteJob.DeleteSingleAsync(fileFilter, pathFilter);
        Assert.That(deleteJob.FileErrorList, Is.Empty, "Delete single file reported file errors.");
    }
}
