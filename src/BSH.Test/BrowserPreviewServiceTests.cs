// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Models;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Services;
using BSH.Test.Fakes;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace BSH.Test;

public class BrowserPreviewServiceTests
{
    [Test]
    public async Task PreviewFileAsync_WhenMediaUnavailable_DoesNotRetrieveOrLaunch()
    {
        var jobService = new PreviewJobService { CheckMediaResult = false };
        var queryManager = new PreviewQueryManager();
        var host = new RecordingPreviewHost();
        var service = CreateService(jobService, queryManager: queryManager, previewHost: host);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(jobService.CheckMediaCalls, Is.EqualTo(new[] { ActionType.Restore }));
        Assert.That(jobService.RequestPasswordCallCount, Is.EqualTo(0));
        Assert.That(queryManager.GetFileNameCalls, Is.Empty);
        Assert.That(host.ShownFiles, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_WhenPasswordRejected_DoesNotRetrieveOrLaunch()
    {
        var jobService = new PreviewJobService { RequestPasswordResult = false };
        var queryManager = new PreviewQueryManager();
        var host = new RecordingPreviewHost();
        var service = CreateService(jobService, queryManager: queryManager, previewHost: host);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(jobService.RequestPasswordCallCount, Is.EqualTo(1));
        Assert.That(queryManager.GetFileNameCalls, Is.Empty);
        Assert.That(host.ShownFiles, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_RetrievesFileLaunchesPreviewAndCleansTemporaryFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "bsh-preview-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tempFile, "preview-content");

        var queryManager = new PreviewQueryManager
        {
            FileResult = (tempFile, true)
        };
        var host = new RecordingPreviewHost();
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(queryManager.GetFileNameCalls.Single(), Is.EqualTo((2, "report.txt", @"\source\docs\", "secret")));
        Assert.That(host.ShownFiles.Single(), Is.EqualTo(tempFile));
        Assert.That(File.Exists(tempFile), Is.False);
        Assert.That(presentation.MessageBoxes, Is.Empty);
    }

    [Test]
    public async Task PreviewFileAsync_WhenLaunchFails_SurfacesErrorThroughPresentationService()
    {
        var queryManager = new PreviewQueryManager
        {
            FileResult = (@"C:\backed-up\report.txt", false)
        };
        var host = new RecordingPreviewHost { ThrowOnShow = true };
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(presentation.MessageBoxes, Has.Count.EqualTo(1));
        // Without packaged PRI resources, CommunityToolkit.GetLocalized returns null in unit hosts.
        Assert.That(presentation.MessageBoxes[0].Title, Is.Null.Or.Empty.Or.EqualTo("Browser_FeatureNotAvailable_Title"));
        Assert.That(presentation.MessageBoxes[0].Content, Is.Null.Or.Empty.Or.EqualTo("Browser_FeatureNotAvailable_Text"));
    }

    [Test]
    public async Task PreviewFileAsync_WhenFileIsNotTemporary_LaunchesWithoutDeletingSource()
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), "bsh-preview-source-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(sourcePath, "source-content");

        try
        {
            var queryManager = new PreviewQueryManager
            {
                FileResult = (sourcePath, false)
            };
            var host = new RecordingPreviewHost();
            var service = CreateService(queryManager: queryManager, previewHost: host);

            await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

            Assert.That(host.ShownFiles.Single(), Is.EqualTo(sourcePath));
            Assert.That(File.Exists(sourcePath), Is.True);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    [Test]
    public async Task PreviewFileAsync_WhenRetrievedPathIsEmpty_ShowsErrorAndDoesNotLaunch()
    {
        var queryManager = new PreviewQueryManager
        {
            FileResult = (null, false)
        };
        var host = new RecordingPreviewHost();
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(host.ShownFiles, Is.Empty);
        Assert.That(presentation.MessageBoxes, Has.Count.EqualTo(1));
        Assert.That(presentation.MessageBoxes[0].Title, Is.Null.Or.Empty.Or.EqualTo("Browser_PreviewRetrieveFailed_Title"));
        Assert.That(presentation.MessageBoxes[0].Content, Is.Null.Or.Empty.Or.EqualTo("Browser_PreviewRetrieveFailed_Text"));
    }

    [Test]
    public async Task PreviewFileAsync_WhenLaunchFailsForTemporaryFile_StillCleansUp()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "bsh-preview-fail-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(tempFile, "preview-content");

        var queryManager = new PreviewQueryManager
        {
            FileResult = (tempFile, true)
        };
        var host = new RecordingPreviewHost { ThrowOnShow = true };
        var presentation = new PreviewPresentationService();
        var service = CreateService(queryManager: queryManager, previewHost: host, presentationService: presentation);

        await service.PreviewFileAsync("2", "report.txt", @"\source\docs\");

        Assert.That(presentation.MessageBoxes, Has.Count.EqualTo(1));
        Assert.That(File.Exists(tempFile), Is.False);
    }

    private static BrowserPreviewService CreateService(
        PreviewJobService? jobService = null,
        PreviewBackupService? backupService = null,
        PreviewQueryManager? queryManager = null,
        PreviewPresentationService? presentationService = null,
        RecordingPreviewHost? previewHost = null)
    {
        return new BrowserPreviewService(
            jobService ?? new PreviewJobService(),
            backupService ?? new PreviewBackupService(),
            queryManager ?? new PreviewQueryManager(),
            presentationService ?? new PreviewPresentationService(),
            previewHost ?? new RecordingPreviewHost());
    }

    private sealed class RecordingPreviewHost : ISmartPreviewHost
    {
        public List<string> ShownFiles { get; } = [];
        public bool ThrowOnShow { get; set; }

        public Task ShowAsync(string filePath)
        {
            if (ThrowOnShow)
            {
                throw new InvalidOperationException("SmartPreview failed to start.");
            }

            ShownFiles.Add(filePath);
            return Task.CompletedTask;
        }
    }

    private sealed class PreviewJobService : FakeJobService
    {
        public bool CheckMediaResult { get; set; } = true;
        public bool RequestPasswordResult { get; set; } = true;
        public List<ActionType> CheckMediaCalls { get; } = [];
        public int RequestPasswordCallCount { get; private set; }

        public override Task<bool> CheckMediaAsync(ActionType action, bool silent = false)
        {
            CheckMediaCalls.Add(action);
            return Task.FromResult(CheckMediaResult);
        }

        public override Task<bool> RequestPassword()
        {
            RequestPasswordCallCount++;
            return Task.FromResult(RequestPasswordResult);
        }
    }

    private sealed class PreviewBackupService : FakeBackupService
    {
        public override string GetPassword() => "secret";
        public override bool HasPassword() => true;
    }

    private sealed class PreviewQueryManager : FakeQueryManager
    {
        public (string Path, bool IsTemporary)? FileResult { get; set; } = (@"C:\backed-up\report.txt", false);
        public List<(int VersionId, string FileName, string FilePath, string Password)> GetFileNameCalls { get; } = [];

        public override Task<(string, bool)> GetFileNameFromDriveAsync(int versionId, string fileName, string filePath, string password)
        {
            GetFileNameCalls.Add((versionId, fileName, filePath, password));
            return Task.FromResult(FileResult ?? ((string)null, false));
        }

        public override Task<VersionDetails> GetLastBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public override Task<VersionDetails> GetLastFullBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public override Task<int> GetNumberOfVersionsAsync() => Task.FromResult(1);
        public override Task<VersionDetails> GetOldestBackupAsync() => Task.FromResult(new VersionDetails { Id = "1" });
        public override Task<VersionDetails> GetVersionByIdAsync(string id) => Task.FromResult(new VersionDetails { Id = id });
        public override List<VersionDetails> GetVersions(bool desc = true) => [new VersionDetails { Id = "1" }];
    }

    private sealed class PreviewPresentationService : FakePresentationService
    {
        public List<(string Title, string Content)> MessageBoxes { get; } = [];

        public override Task<ContentDialogResult> ShowMessageBoxAsync(string title, string content, IList<IUICommand>? commands, uint defaultCommandIndex = 0, uint cancelCommandIndex = 1)
        {
            MessageBoxes.Add((title, content));
            return Task.FromResult(ContentDialogResult.None);
        }
    }
}
