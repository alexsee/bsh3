// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Main;
using NUnit.Framework;

namespace BSH.Test;

[Apartment(ApartmentState.STA)]
public class WinFormsPreviewServiceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task EntryPointsPassTheirSelectedVersionFileAndFolder(bool properties)
    {
        (int, string, string) retrieved = default;
        var service = new WinFormsPreviewService(() => Task.FromResult(true), () => true,
            (version, file, folder) =>
            {
                retrieved = (version, file, folder);
                return Task.FromResult((@"C:\source\report one.txt", false));
            }, _ => Task.CompletedTask, () => Assert.Fail("Unexpected preview error."));

        if (properties)
        {
            using var form = new frmFileProperties { PreviewService = service, CurrentFileFolder = @"\properties folder\" };
            form.lblFileName.Text = "report one.txt";
            Select(form.lvVersions, new ListViewItem { Tag = new FileTableRow { FilePackage = "42" } });
            await form.PreviewSelectedFileAsync();
            Assert.That(retrieved, Is.EqualTo((42, "report one.txt", @"\properties folder\")));
        }
        else
        {
            using var form = new frmBrowser { PreviewService = service };
            typeof(frmBrowser).GetField("selectedVersion", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(form, new VersionDetails { Id = "17" });
            Select(form.lvFiles, new ListViewItem("report one.txt") { Tag = @"\browser folder\" });
            await form.PreviewSelectedFileAsync();
            Assert.That(retrieved, Is.EqualTo((17, "report one.txt", @"\browser folder\")));
        }
    }

    [Test]
    public async Task EmptySelectionsAndBrowserFoldersDoNotRunPreflight()
    {
        var service = new WinFormsPreviewService(() => throw new AssertionException("Unexpected preflight."),
            () => true, (_, _, _) => throw new NotSupportedException(),
            _ => Task.CompletedTask, () => Assert.Fail("Unexpected error."));
        using var browser = new frmBrowser { PreviewService = service };
        using var properties = new frmFileProperties { PreviewService = service };

        await browser.PreviewSelectedFileAsync();
        await properties.PreviewSelectedFileAsync();
        Select(browser.lvFiles, new ListViewItem("folder") { ImageKey = "folder" });
        await browser.PreviewSelectedFileAsync();
    }

    [Test]
    public async Task MissingRetrievedPathReportsErrorWithoutLaunching()
    {
        var errors = 0;
        var launches = 0;
        var service = new WinFormsPreviewService(() => Task.FromResult(true), () => true,
            (_, _, _) => Task.FromResult(((string)null, false)), _ =>
            {
                launches++;
                return Task.CompletedTask;
            }, () => errors++);

        await service.PreviewAsync(() => (1, "file", "folder"));

        Assert.That(errors, Is.EqualTo(1));
        Assert.That(launches, Is.Zero);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task RejectedPreflightDoesNotRetrieveOrLaunch(bool media, bool password)
    {
        var passwordCalls = 0;
        var service = new WinFormsPreviewService(() => Task.FromResult(media), () =>
        {
            passwordCalls++;
            return password;
        }, (_, _, _) => throw new AssertionException("Unexpected extraction."),
            _ => throw new AssertionException("Unexpected launch."), () => Assert.Fail("Unexpected error."));

        await service.PreviewAsync(() => throw new AssertionException("Unexpected selection."));

        Assert.That(passwordCalls, Is.EqualTo(media ? 1 : 0));
    }

    [TestCase(true, "success")]
    [TestCase(true, "start failure")]
    [TestCase(true, "wait failure")]
    [TestCase(false, "success")]
    [TestCase(false, "start failure")]
    [TestCase(false, "wait failure")]
    public async Task LifetimeCleansOnlyOwnedFilesAndDisposesProcess(bool owned, string outcome)
    {
        var path = Path.Combine(Path.GetTempPath(), "bsh preview " + Guid.NewGuid() + ".txt");
        await File.WriteAllTextAsync(path, "source content");
        var errors = 0;
        TrackingProcess process = null;
        ProcessStartInfo launchInfo = null;
        var service = new WinFormsPreviewService(() => Task.FromResult(true), () => true,
            (_, _, _) => Task.FromResult((path, owned)),
            file => WinFormsPreviewService.LaunchAsync(file, info =>
            {
                launchInfo = info;
                Assert.That(File.Exists(path), Is.True, "File must exist during launch.");
                if (outcome == "start failure")
                {
                    throw new InvalidOperationException("Start failed.");
                }

                process = new TrackingProcess();
                if (outcome == "success")
                {
                    process.StartInfo = new ProcessStartInfo("cmd.exe", "/c exit 0") { UseShellExecute = false, CreateNoWindow = true };
                    process.Start();
                }
                // An unstarted process makes WaitForExitAsync fail deterministically.
                return process;
            }), () => errors++);

        try
        {
            await service.PreviewAsync(() => (1, "report.txt", @"\folder\"));

            Assert.That(errors, Is.EqualTo(outcome == "success" ? 0 : 1));
            Assert.That(File.Exists(path), Is.EqualTo(!owned));
            Assert.That(launchInfo.ArgumentList, Is.EqualTo(new[] { "-file:" + path }));
            Assert.That(launchInfo.FileName, Is.EqualTo(Path.Combine(Application.StartupPath, "SmartPreview.exe")));
            if (process != null)
            {
                Assert.That(process.IsDisposed, Is.True);
            }
            if (!owned)
            {
                Assert.That(await File.ReadAllTextAsync(path), Is.EqualTo("source content"));
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public async Task ExtractedFileRemainsUntilPreviewCompletes()
    {
        var path = Path.GetTempFileName();
        var finished = new TaskCompletionSource();
        var service = new WinFormsPreviewService(() => Task.FromResult(true), () => true,
            (_, _, _) => Task.FromResult((path, true)), _ => finished.Task, () => Assert.Fail("Unexpected error."));
        var preview = service.PreviewAsync(() => (1, "report.txt", "folder"));
        try
        {
            Assert.That(preview.IsCompleted, Is.False);
            Assert.That(File.Exists(path), Is.True);
        }
        finally
        {
            finished.SetResult();
            await preview;
            try
            {
                Assert.That(File.Exists(path), Is.False);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }

    private static void Select(ListView list, ListViewItem item)
    {
        // Selection is only maintained by ListView once its native handle exists.
        _ = list.Handle;
        list.Items.Add(item);
        item.Selected = true;
    }

    private sealed class TrackingProcess : Process
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
