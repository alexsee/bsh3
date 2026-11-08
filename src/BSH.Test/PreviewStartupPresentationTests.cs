// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewStartupPresentationTests
{
    private static readonly string RepoRoot = LocateRepoRoot();

    [TestCaseSource(nameof(LaunchArguments))]
    public void FirstLaunchPresentsTheMainWindowOnlyForAManualStart(string[] args, bool present)
    {
        var autoStart = PreviewLaunch.IsAutoStart(args);
        Assert.That(autoStart, Is.Not.EqualTo(present));
        Assert.That(PreviewAppStartup.ShouldPresentMainWindow(autoStart), Is.EqualTo(present));
        Assert.That(
            PreviewAppStartup.ShouldExit(
                PreviewStartupResult.Started,
                autoStart,
                (_, _) => Assert.Fail("A start that owns the database does not show a dialog."),
                (_, fallback) => fallback),
            Is.False);
    }

    [Test]
    public void MissingArgumentsAreAManualLaunch()
    {
        Assert.That(PreviewLaunch.IsAutoStart(null), Is.False);
        Assert.That(PreviewAppStartup.ShouldPresentMainWindow(false), Is.True);
    }

    [Test]
    public void ForegroundRequestDuringStartupIsDeferredUntilInitializationFinishes()
    {
        var coordinator = new PreviewForegroundCoordinator();

        Assert.That(coordinator.TryPresentNow(), Is.False);
        Assert.That(coordinator.TryPresentNow(), Is.False);
        Assert.That(coordinator.CompleteStartup(), Is.True);
        Assert.That(coordinator.TryPresentNow(), Is.True);
        Assert.That(coordinator.CompleteStartup(), Is.False);
    }

    [Test]
    public void ForegroundRequestAfterStartupPresentsImmediately()
    {
        var coordinator = new PreviewForegroundCoordinator();

        Assert.That(coordinator.CompleteStartup(), Is.False);
        Assert.That(coordinator.TryPresentNow(), Is.True);
    }

    [Test]
    public void AutostartPolicyIsOnlyConsultedWhileDecidingTheInitialLaunch()
    {
        var mainApp = Path.Combine(RepoRoot, "src", "BSH.MainApp");
        var readers = Directory.GetFiles(mainApp, "*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("ShouldPresentMainWindow("))
            .Select(Path.GetFileName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.That(readers, Is.EqualTo(new[] { "App.xaml.cs", "PreviewAppStartup.cs" }));

        var app = File.ReadAllText(Path.Combine(mainApp, "App.xaml.cs"));
        var launched = ExtractMethod(app, "void OnLaunched(");
        Assert.That(launched, Does.Contain("PreviewLaunch.IsAutoStart(Environment.GetCommandLineArgs())"));
        Assert.That(launched, Does.Contain("activateExisting: presentMainWindow"));
        Assert.That(CountOf(app, "IsAutoStart("), Is.EqualTo(1));

        foreach (var handler in new[]
        {
            ExtractMethod(app, "void BringPreviewToFront("),
            ExtractMethod(app, "void PresentMainWindow("),
            ExtractMethod(app, "void ShowMainWindowCommand_ExecuteRequested("),
            ExtractMethod(app, "void ShowBrowserWindowCommand_ExecuteRequested("),
            ExtractMethod(
                File.ReadAllText(Path.Combine(mainApp, "Services", "AppNotificationService.cs")),
                "void Activate("),
            ExtractMethod(
                File.ReadAllText(Path.Combine(mainApp, "Services", "PresentationService.cs")),
                "Task ShowMainWindowAsync(")
        })
        {
            Assert.That(handler, Does.Not.Contain("IsAutoStart"));
            Assert.That(handler, Does.Not.Contain("ShouldPresentMainWindow"));
        }
    }

    [Test]
    public void BackgroundStartupDoesNotCreateTheMainWindow()
    {
        var mainApp = Path.Combine(RepoRoot, "src", "BSH.MainApp");
        var app = File.ReadAllText(Path.Combine(mainApp, "App.xaml.cs"));
        var launched = ExtractMethod(app, "void OnLaunched(");

        Assert.That(launched, Does.Not.Contain("App.MainWindow"));
        Assert.That(launched, Does.Not.Contain("new MainWindow"));
        Assert.That(launched, Does.Not.Contain("NavigateForStartup"));

        var presentBranch = launched.IndexOf("if (presentMainWindow)", StringComparison.Ordinal);
        Assert.That(presentBranch, Is.GreaterThan(0));
        Assert.That(launched.IndexOf("ActivateAsync(", StringComparison.Ordinal), Is.GreaterThan(presentBranch));
        Assert.That(CountOf(launched, "ActivateAsync("), Is.EqualTo(1));

        foreach (var step in new[]
        {
            "OpenDatabaseAsync(",
            "IStatusService>().Initialize()",
            "IOrchestrationService>().InitializeAsync()",
            "InitializeTrayIcon()",
            "MaybeCheckOnStartupAsync()"
        })
        {
            var index = launched.IndexOf(step, StringComparison.Ordinal);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), step);
            Assert.That(index, Is.LessThan(presentBranch), step);
        }

        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "StatusService.cs"),
            "void Initialize(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "OrchestrationService.cs"),
            "Task InitializeAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "OrchestrationService.cs"),
            "Task StartAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "OrchestrationService.cs"),
            "Task ApplyAutomationPolicyAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "ScheduledBackupService.cs"),
            "Task StartAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "ApplicationUpdateService.cs"),
            "Task InitializeAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "ApplicationUpdateService.cs"),
            "Task MaybeCheckOnStartupAsync(");
        AssertDoesNotTouchMainWindow(
            Path.Combine(mainApp, "Services", "ApplicationUpdateService.cs"),
            "Task CheckAsync(");
        AssertDoesNotTouchMainWindow(Path.Combine(mainApp, "App.xaml.cs"), "void InitializeTrayIcon(");

        var present = ExtractMethod(app, "void PresentMainWindow(");
        Assert.That(present, Does.Contain("NavigateForStartup()"));
        Assert.That(present, Does.Contain("window = MainWindow"));
        Assert.That(present, Does.Contain("AppWindow.Show()"));
        Assert.That(present, Does.Contain("window.Activate()"));
    }

    private static void AssertDoesNotTouchMainWindow(string path, string signature)
    {
        var body = ExtractMethod(File.ReadAllText(path), signature);
        Assert.That(body, Does.Not.Contain("App.MainWindow"), signature);
        Assert.That(body, Does.Not.Contain("new MainWindow"), signature);
        Assert.That(body, Does.Not.Contain(".Activate("), signature);
    }

    private static IEnumerable<TestCaseData> LaunchArguments()
    {
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "-autostart" }, false);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "/autostart" }, false);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "-AUTOSTART" }, false);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "/AutoStart" }, false);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "-autostart", "/config" }, false);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe" }, true);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "-delayedstart" }, true);
        yield return new TestCaseData(new[] { "BSH.MainApp.exe", "/config" }, true);
    }

    private static int CountOf(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string ExtractMethod(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThanOrEqualTo(0), signature);
        var open = source.IndexOf('{', start);
        Assert.That(open, Is.GreaterThan(start), signature);

        var depth = 0;
        for (var i = open; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(open, i - open + 1);
                }
            }
        }

        Assert.Fail($"Method {signature} was not closed.");
        return string.Empty;
    }

    private static string LocateRepoRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null)
        {
            var setupIss = Path.Combine(dir.FullName, "tools", "setup", "Setup.iss");
            if (File.Exists(setupIss))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing tools/setup/Setup.iss.");
    }
}
