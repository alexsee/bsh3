// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Contract for release installers: the original WinForms setup stays as its
/// own artifact, the WinUI beta setup launches BSH.MainApp, and both shells
/// remain publishable with the VSS helper.
/// </summary>
public class InstallerEntryPointTests
{
    private static readonly string RepoRoot = LocateRepoRoot();
    private static readonly string WinFormsSetupIssPath = Path.Combine(RepoRoot, "tools", "setup", "Setup.iss");
    private static readonly string WinUiSetupIssPath = Path.Combine(RepoRoot, "tools", "setup", "Setup-WinUI.iss");
    private static readonly string MainAppCsprojPath = Path.Combine(RepoRoot, "src", "BSH.MainApp", "BSH.MainApp.csproj");
    private static readonly string MainCsprojPath = Path.Combine(RepoRoot, "src", "BSH.Main", "BSH.Main.csproj");

    [Test]
    public void WinFormsInstallerStartMenuLaunchesWinFormsShell()
    {
        var icons = GetIssSection(WinFormsSetupIssPath, "[Icons]");

        Assert.That(icons, Does.Contain(@"Filename: ""{app}\BSH.Main.exe"""));
        Assert.That(icons, Does.Not.Contain(@"BSH.MainApp.exe"));
    }

    [Test]
    public void WinFormsInstallerPostInstallLaunchesWinFormsShell()
    {
        var run = GetIssSection(WinFormsSetupIssPath, "[Run]");

        Assert.That(run, Does.Contain(@"Filename: ""{app}\BSH.Main.exe""; Flags: nowait postinstall skipifsilent"));
        Assert.That(run, Does.Not.Contain(@"Filename: ""{app}\BSH.MainApp.exe"""));
    }

    [Test]
    public void WinFormsInstallerStartWithWindowsTargetsWinFormsShell()
    {
        var registry = GetIssSection(WinFormsSetupIssPath, "[Registry]");

        Assert.That(registry, Does.Contain(@"ValueData: ""{app}\BSH.Main.exe"""));
        Assert.That(registry, Does.Not.Contain(@"BSH.MainApp.exe"));
    }

    [Test]
    public void WinUiInstallerStartMenuLaunchesWinUiShell()
    {
        var icons = GetIssSection(WinUiSetupIssPath, "[Icons]");

        Assert.That(icons, Does.Match(
            @"Name:\s*""\{group\}\\Backup Service Home"";\s*Filename:\s*""\{app\}\\BSH\.MainApp\.exe"""));
        Assert.That(icons, Does.Not.Contain(@"BSH.Main.exe"));
    }

    [Test]
    public void WinUiInstallerPostInstallLaunchesWinUiShell()
    {
        var run = GetIssSection(WinUiSetupIssPath, "[Run]");

        Assert.That(run, Does.Contain(@"Filename: ""{app}\BSH.MainApp.exe""; Flags: nowait postinstall skipifsilent"));
        Assert.That(run, Does.Not.Contain(@"Filename: ""{app}\BSH.Main.exe"""));
    }

    [Test]
    public void WinUiInstallerStartWithWindowsTargetsWinUiShell()
    {
        var registry = GetIssSection(WinUiSetupIssPath, "[Registry]");

        Assert.That(registry, Does.Contain(@"ValueName: ""BackupServiceHome3Run""; ValueData: ""{app}\BSH.MainApp.exe"""));
        Assert.That(registry, Does.Not.Contain(@"BSH.Main.exe"));
    }

    [Test]
    public void BothInstallersRegisterAndStartVssService()
    {
        foreach (var setupIssPath in new[] { WinFormsSetupIssPath, WinUiSetupIssPath })
        {
            var run = GetIssSection(setupIssPath, "[Run]");

            Assert.That(run, Does.Contain("binpath=\"\"{app}\\BSH.Service.exe\"\" start=auto"));
            Assert.That(run, Does.Contain("Parameters: \"start \"\"Backup Service Home-Dienst\"\""));
        }
    }

    [Test]
    public void WinUiInstallerIsASeparateArtifact()
    {
        var setup = GetIssSection(WinUiSetupIssPath, "[Setup]");
        var winFormsSetup = GetIssSection(WinFormsSetupIssPath, "[Setup]");

        Assert.That(setup, Does.Contain("OutputBaseFilename=backupservicehome-{#ApplicationVersion}-winui-win64"));
        Assert.That(winFormsSetup, Does.Contain("OutputBaseFilename=backupservicehome-{#ApplicationVersion}-win64"));
        Assert.That(winFormsSetup, Does.Not.Contain("-winui-win64"));
    }

    [Test]
    public void WinUiShellIsIncludedInReleasePublish()
    {
        Assert.That(IsPublishable(MainAppCsprojPath), Is.True);
    }

    [Test]
    public void WinUiPublishIncludesWindowsAppRuntime()
    {
        var document = XDocument.Load(MainAppCsprojPath);
        var element = document.Descendants("WindowsAppSDKSelfContained").FirstOrDefault();

        Assert.That(element, Is.Not.Null);
        Assert.That(element.Value.Trim(), Is.EqualTo("true").IgnoreCase);
        Assert.That(
            element.Attribute("Condition")?.Value,
            Is.EqualTo("'$(SelfContained)' == 'true'"));
    }

    [Test]
    public void WinUiInstallerPackagesNestedPublishOutputForWindowsAppRuntime()
    {
        var files = GetIssSection(WinUiSetupIssPath, "[Files]");

        Assert.That(
            files,
            Does.Contain(@"Source: ""..\..\output\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs"));
    }

    [Test]
    public void WinFormsShellRemainsPublishable()
    {
        Assert.That(IsPublishable(MainCsprojPath), Is.True);
    }

    private static string GetIssSection(string setupIssPath, string sectionHeader)
    {
        Assert.That(File.Exists(setupIssPath), Is.True, $"Installer script was not found at {setupIssPath}.");

        var text = File.ReadAllText(setupIssPath);
        var match = Regex.Match(
            text,
            Regex.Escape(sectionHeader) + @"\r?\n(?<body>[\s\S]*?)(?=\r?\n\[|\z)");

        Assert.That(match.Success, Is.True, $"Section {sectionHeader} was not found in {Path.GetFileName(setupIssPath)}.");
        return match.Groups["body"].Value;
    }

    private static bool IsPublishable(string csprojPath)
    {
        var document = XDocument.Load(csprojPath);
        var value = document
            .Descendants("IsPublishable")
            .Select(element => element.Value.Trim())
            .FirstOrDefault();

        return value is null || !string.Equals(value, "False", StringComparison.OrdinalIgnoreCase);
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
