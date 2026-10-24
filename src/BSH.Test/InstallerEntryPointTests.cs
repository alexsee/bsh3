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
    private const string QuotedServiceBinPath = "binpath= \"\"\\\"\"{app}\\BSH.Service.exe\\\"\"\"\" start=auto";

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
    public void WinFormsInstallerPackagesItsCompleteIsolatedPayload()
    {
        var files = GetIssSection(WinFormsSetupIssPath, "[Files]");
        Assert.That(files, Does.Contain(
            @"Source: ""..\..\output\stable\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs"));
        Assert.That(files, Does.Not.Contain(@"..\..\output\preview\"));
        Assert.That(files, Does.Not.Contain(@"Source: ""..\..\output\*"""));
    }

    [Test]
    public void WinUiInstallerStartMenuLaunchesWinUiShell()
    {
        var icons = GetIssSection(WinUiSetupIssPath, "[Icons]");

        Assert.That(icons, Does.Match(
            @"Name:\s*""\{group\}\\Backup Service Home 4"";\s*Filename:\s*""\{app\}\\BSH\.MainApp\.exe"""));
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

        Assert.That(registry, Does.Contain(@"ValueName: ""BackupServiceHome3PreviewRun"""));
        // Inno doubled quotes: the script text """{app}\BSH.MainApp.exe"" -autostart"
        // is the registry value "{app}\BSH.MainApp.exe" -autostart, expanded to
        // "C:\...\BSH.MainApp.exe" -autostart.
        Assert.That(registry, Does.Contain(@"ValueData: """"""{app}\BSH.MainApp.exe"""" -autostart"""));
        Assert.That(registry, Does.Contain(@"ValueName: ""BackupServiceHome3Run""; Tasks: disableStableAutostart; Flags: deletevalue"));
        Assert.That(registry, Does.Not.Contain(@"BSH.Main.exe"));
    }

    [Test]
    public void BothInstallersRegisterAndStartVssService()
    {
        var winFormsRun = GetIssSection(WinFormsSetupIssPath, "[Run]");
        Assert.That(winFormsRun, Does.Contain(QuotedServiceBinPath));
        Assert.That(winFormsRun, Does.Contain("Parameters: \"start \"\"Backup Service Home-Dienst\"\""));

        var winUiRun = GetIssSection(WinUiSetupIssPath, "[Run]");
        Assert.That(winUiRun, Does.Contain(QuotedServiceBinPath));
        Assert.That(winUiRun, Does.Contain("Parameters: \"start \"\"Backup Service Home Preview-Dienst\"\""));
        Assert.That(winUiRun, Does.Not.Contain("Backup Service Home-Dienst"));
    }

    [Test]
    public void BothInstallersConfigureExistingVssServiceBinPath()
    {
        // sc create fails silently when the service already exists (runhidden).
        // sc config still applies the current binpath and start mode.
        AssertVssServiceBinPathIsCreatedThenConfigured(
            GetIssSection(WinFormsSetupIssPath, "[Run]"),
            "Backup Service Home-Dienst");
        AssertVssServiceBinPathIsCreatedThenConfigured(
            GetIssSection(WinUiSetupIssPath, "[Run]"),
            "Backup Service Home Preview-Dienst");
    }

    [Test]
    public void WinUiInstallerDoesNotRemoveTheStableVssService()
    {
        var text = File.ReadAllText(WinUiSetupIssPath);

        Assert.That(text, Does.Not.Contain("Backup Service Home-Dienst"));
        Assert.That(text, Does.Contain("Backup Service Home Preview-Dienst"));
        Assert.That(GetIssSection(WinUiSetupIssPath, "[UninstallRun]"), Does.Contain("delete \"\"Backup Service Home Preview-Dienst\"\""));
    }

    [Test]
    public void WinUiInstallerIsSideBySideWithTheStableProduct()
    {
        var preview = GetIssSection(WinUiSetupIssPath, "[Setup]");
        var stable = GetIssSection(WinFormsSetupIssPath, "[Setup]");

        Assert.That(preview, Does.Contain("ArchitecturesAllowed=x64compatible"));
        Assert.That(preview, Does.Contain("ArchitecturesInstallIn64BitMode=x64compatible"));
        Assert.That(preview, Does.Contain(@"DefaultDirName={commonpf64}\brightbits\Backup Service Home 4"));
        Assert.That(preview, Does.Contain("DefaultGroupName=Backup Service Home 4"));
        Assert.That(preview, Does.Contain("AppMutex=BackupServiceHome3Preview"));
        Assert.That(preview, Does.Contain("AppID={{8F4C2B1A-7E63-4D9A-B5C0-6A1E9D3F47B2}}"));
        Assert.That(stable, Does.Contain(@"DefaultDirName={pf}\Alexosoft\Backup Service Home 3"));
        Assert.That(stable, Does.Not.Contain("Backup Service Home 4"));
        Assert.That(stable, Does.Contain("AppMutex=BackupServiceHome3"));
        Assert.That(stable, Does.Contain("AppID={{5979B77A-9AE6-4E75-AED8-283C5E16C02D}}"));
    }

    [Test]
    public void WinUiInstallerOmitsTheWinFormsShell()
    {
        var files = GetIssSection(WinUiSetupIssPath, "[Files]");

        Assert.That(files, Does.Contain(@"Source: ""..\..\output\preview\*"""));
        Assert.That(files, Does.Not.Contain(@"..\..\output\stable\"));
        Assert.That(files, Does.Not.Contain(@"Source: ""..\..\output\*"""));
        Assert.That(files, Does.Contain(@"Source: ""preview-vss.pipe""; DestDir: ""{app}""; DestName: ""vss.pipe"""));
        Assert.That(File.ReadAllText(Path.Combine(RepoRoot, "tools", "setup", "preview-vss.pipe")).Trim(), Is.EqualTo("backupservicehome-preview"));
    }

    [Test]
    public void BothInstallersUseVersionedWindowsFilename()
    {
        var setup = GetIssSection(WinUiSetupIssPath, "[Setup]");
        var winFormsSetup = GetIssSection(WinFormsSetupIssPath, "[Setup]");

        Assert.That(setup, Does.Contain("OutputBaseFilename=backupservicehome-{#ApplicationVersion}-win64"));
        Assert.That(winFormsSetup, Does.Contain("OutputBaseFilename=backupservicehome-{#ApplicationVersion}-win64"));
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
            Does.Contain(@"Source: ""..\..\output\preview\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs"));
    }

    [Test]
    public void WinFormsShellRemainsPublishable()
    {
        Assert.That(IsPublishable(MainCsprojPath), Is.True);
    }

    private static void AssertVssServiceBinPathIsCreatedThenConfigured(string runSection, string serviceName)
    {
        var create = "Parameters: \"create \"\"" + serviceName + "\"\" " + QuotedServiceBinPath + "\"";
        var config =
            "Filename: \"sc\"; Parameters: \"config \"\"" + serviceName + "\"\" " + QuotedServiceBinPath +
            "\"; WorkingDir: \"{app}\"; Flags: runhidden runascurrentuser";

        var createIndex = runSection.IndexOf(create, StringComparison.Ordinal);
        var configIndex = runSection.IndexOf(config, StringComparison.Ordinal);

        Assert.That(createIndex, Is.GreaterThanOrEqualTo(0), $"sc create for {serviceName} was not found.");
        Assert.That(
            configIndex,
            Is.GreaterThan(createIndex),
            $"sc config for {serviceName} must follow sc create and set binpath and start=auto.");
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
