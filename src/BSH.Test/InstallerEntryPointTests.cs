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
/// Contract for the tagged beta installer: customers launch the WinUI shell,
/// the VSS helper still installs, and both shells remain publishable.
/// </summary>
public class InstallerEntryPointTests
{
    private static readonly string RepoRoot = LocateRepoRoot();
    private static readonly string SetupIssPath = Path.Combine(RepoRoot, "tools", "setup", "Setup.iss");
    private static readonly string MainAppCsprojPath = Path.Combine(RepoRoot, "src", "BSH.MainApp", "BSH.MainApp.csproj");
    private static readonly string MainCsprojPath = Path.Combine(RepoRoot, "src", "BSH.Main", "BSH.Main.csproj");

    [Test]
    public void StartMenuShortcutLaunchesWinUiShell()
    {
        var icons = GetIssSection("[Icons]");

        Assert.That(icons, Does.Match(
            @"Name:\s*""\{group\}\\Backup Service Home"";\s*Filename:\s*""\{app\}\\BSH\.MainApp\.exe"""));
        Assert.That(icons, Does.Not.Contain(@"BSH.Main.exe"));
    }

    [Test]
    public void PostInstallLaunchStartsWinUiShell()
    {
        var run = GetIssSection("[Run]");

        Assert.That(run, Does.Contain(@"Filename: ""{app}\BSH.MainApp.exe""; Flags: nowait postinstall skipifsilent"));
        Assert.That(run, Does.Not.Contain(@"Filename: ""{app}\BSH.Main.exe"""));
    }

    [Test]
    public void StartWithWindowsRunKeyTargetsWinUiShell()
    {
        var registry = GetIssSection("[Registry]");

        Assert.That(registry, Does.Contain(@"ValueName: ""BackupServiceHome3Run""; ValueData: ""{app}\BSH.MainApp.exe"""));
        Assert.That(registry, Does.Not.Contain(@"BSH.Main.exe"));
    }

    [Test]
    public void InstallerRegistersAndStartsVssService()
    {
        var run = GetIssSection("[Run]");

        Assert.That(run, Does.Contain("binpath=\"\"{app}\\BSH.Service.exe\"\" start=auto"));
        Assert.That(run, Does.Contain("Parameters: \"start \"\"Backup Service Home-Dienst\"\""));
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
    public void InstallerPackagesNestedPublishOutputForWindowsAppRuntime()
    {
        var files = GetIssSection("[Files]");

        Assert.That(
            files,
            Does.Contain(@"Source: ""..\..\output\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs"));
    }

    [Test]
    public void WinFormsShellRemainsPublishable()
    {
        Assert.That(IsPublishable(MainCsprojPath), Is.True);
    }

    private static string GetIssSection(string sectionHeader)
    {
        var text = File.ReadAllText(SetupIssPath);
        var match = Regex.Match(
            text,
            Regex.Escape(sectionHeader) + @"\r?\n(?<body>[\s\S]*?)(?=\r?\n\[|\z)");

        Assert.That(match.Success, Is.True, $"Section {sectionHeader} was not found in Setup.iss.");
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
