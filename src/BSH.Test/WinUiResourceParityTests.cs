// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Verifies WinUI de/en resource catalogs stay complete and in sync without loading WinRT ResourceLoader.
/// </summary>
public class WinUiResourceParityTests
{
    private static readonly string MainAppRoot = LocateMainAppRoot();

    private static readonly string EnglishResourcesPath =
        Path.Combine(MainAppRoot, "Strings", "en-us", "Resources.resw");

    private static readonly string GermanResourcesPath =
        Path.Combine(MainAppRoot, "Strings", "de-de", "Resources.resw");

    /// <summary>
    /// Keys that must exist for backup / restore / settings / browser spot-checks.
    /// </summary>
    private static readonly string[] RequiredKeys =
    [
        "AppDisplayName",
        "Nav_Overview",
        "Nav_BackupBrowser",
        "Settings_Title",
        "MainView_BtnCreateBackup_Title",
        "MainView_BackupMode_Automatic",
        "MainView_BackupMode_Manual",
        "MainView_BackupMode_Scheduled",
        "MainView_ScheduleWarning_NoEntries",
        "MainView_SystemStatus_Running",
        "MainView_SystemStatus_BatteryPaused",
        "MainView_SystemStatus_Deactivated",
        "MainView_SystemStatus_NotConfigured",
        "MainView_NonePlanned",
        "CreateBackup_Title_Manual",
        "MSG_TASK_RUNNING_TITLE",
        "MSG_TASK_RUNNING_TEXT",
        "MSG_BACKUP_DEVICE_NOT_READY_TITLE",
        "MSG_BACKUP_DEVICE_NOT_READY_TEXT",
        "MSG_PASSWORD_WRONG_TITLE",
        "MSG_PASSWORD_WRONG_TEXT",
        "MsgBox_Yes",
        "MsgBox_No",
        "MsgBox_Cancel",
        "MsgBox_OK",
        "MediaType_LocalDevice",
        "MediaType_FileTransferServer",
        "Browser_Title.Text",
        "Browser_DeleteBackup_Title",
        "Browser_DeleteBackup_Confirm",
        "Tray_ShowBackupBrowser",
        "Tray_StatusAndConfiguration",
        "Tray_StartManualBackup",
        "Tray_Exit",
        "INFO_BACKUP_SUCCESSFUL_TITLE",
        "INFO_BACKUP_SUCCESSFUL_TEXT",
        "INFO_BACKUP_UNSUCCESSFUL_TITLE",
        "INFO_BACKUP_UNSUCCESSFUL_TEXT",
        "Status_FilesProcessed",
        "Status_AfterCompletion",
        "Status_NoAction",
        "Status_Shutdown",
        "Status_Hibernate",
        "Password_Required_Title",
        "Overwrite_Title",
        "Overwrite_ReplaceHeader",
        "Overwrite_SkipHeader",
        "WaitForMedium_Title",
        "CreateBackup_Title",
        "Formatter_Never",
        "Formatter_Today",
        "Formatter_Yesterday",
    ];

    [Test]
    public void EnglishAndGermanResourceFilesExist()
    {
        Assert.That(File.Exists(EnglishResourcesPath), Is.True,
            $"Missing English resources at {EnglishResourcesPath}");
        Assert.That(File.Exists(GermanResourcesPath), Is.True,
            $"Missing German resources at {GermanResourcesPath}");
    }

    [Test]
    public void EnglishAndGermanResourceKeysAreInParity()
    {
        var englishKeys = LoadResourceKeys(EnglishResourcesPath);
        var germanKeys = LoadResourceKeys(GermanResourcesPath);

        var missingInGerman = englishKeys.Keys.Except(germanKeys.Keys).OrderBy(x => x).ToList();
        var missingInEnglish = germanKeys.Keys.Except(englishKeys.Keys).OrderBy(x => x).ToList();

        Assert.That(missingInGerman, Is.Empty,
            "Keys present in en-us but missing in de-de: " + string.Join(", ", missingInGerman));
        Assert.That(missingInEnglish, Is.Empty,
            "Keys present in de-de but missing in en-us: " + string.Join(", ", missingInEnglish));
    }

    [Test]
    public void ResourceValuesAreNonEmpty()
    {
        foreach (var path in new[] { EnglishResourcesPath, GermanResourcesPath })
        {
            var entries = LoadResourceKeys(path);
            var empty = entries.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
            Assert.That(empty, Is.Empty, $"Empty values in {path}: " + string.Join(", ", empty));
        }
    }

    [Test]
    public void RequiredKeysExistInBothLanguages()
    {
        var englishKeys = LoadResourceKeys(EnglishResourcesPath);
        var germanKeys = LoadResourceKeys(GermanResourcesPath);

        var missingEnglish = RequiredKeys.Where(k => !englishKeys.ContainsKey(k)).ToList();
        var missingGerman = RequiredKeys.Where(k => !germanKeys.ContainsKey(k)).ToList();

        Assert.That(missingEnglish, Is.Empty,
            "Required keys missing from en-us: " + string.Join(", ", missingEnglish));
        Assert.That(missingGerman, Is.Empty,
            "Required keys missing from de-de: " + string.Join(", ", missingGerman));
    }

    [Test]
    public void GetLocalizedKeysArePresentInBothResourceFiles()
    {
        var referencedKeys = DiscoverGetLocalizedKeys(MainAppRoot);
        Assert.That(referencedKeys, Is.Not.Empty, "Expected at least one GetLocalized usage in MainApp.");

        var englishKeys = LoadResourceKeys(EnglishResourcesPath);
        var germanKeys = LoadResourceKeys(GermanResourcesPath);

        var missingEnglish = referencedKeys.Where(k => !englishKeys.ContainsKey(k)).ToList();
        var missingGerman = referencedKeys.Where(k => !germanKeys.ContainsKey(k)).ToList();

        Assert.That(missingEnglish, Is.Empty,
            "GetLocalized keys missing from en-us: " + string.Join(", ", missingEnglish));
        Assert.That(missingGerman, Is.Empty,
            "GetLocalized keys missing from de-de: " + string.Join(", ", missingGerman));
    }

    [Test]
    public void ResourceStringMarkupKeysArePresentInBothResourceFiles()
    {
        var referencedKeys = DiscoverResourceStringKeys(MainAppRoot);
        if (referencedKeys.Count == 0)
        {
            Assert.Pass("No ResourceString markup references yet.");
            return;
        }

        var englishKeys = LoadResourceKeys(EnglishResourcesPath);
        var germanKeys = LoadResourceKeys(GermanResourcesPath);

        var missingEnglish = referencedKeys.Where(k => !englishKeys.ContainsKey(k)).ToList();
        var missingGerman = referencedKeys.Where(k => !germanKeys.ContainsKey(k)).ToList();

        Assert.That(missingEnglish, Is.Empty,
            "ResourceString keys missing from en-us: " + string.Join(", ", missingEnglish));
        Assert.That(missingGerman, Is.Empty,
            "ResourceString keys missing from de-de: " + string.Join(", ", missingGerman));
    }

    private static Dictionary<string, string> LoadResourceKeys(string path)
    {
        Assert.That(File.Exists(path), Is.True, $"Resource file not found: {path}");

        var document = XDocument.Load(path);
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var data in document.Root!.Elements("data"))
        {
            var name = data.Attribute("name")?.Value;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var value = data.Element("value")?.Value ?? string.Empty;
            entries[name] = value;
        }

        return entries;
    }

    private static List<string> DiscoverGetLocalizedKeys(string mainAppRoot)
    {
        var pattern = new Regex("\"([^\"]+)\"\\.GetLocalized\\(\\)", RegexOptions.Compiled);
        return DiscoverKeysFromSources(mainAppRoot, "*.cs", pattern);
    }

    private static List<string> DiscoverResourceStringKeys(string mainAppRoot)
    {
        // Matches: helpers:ResourceString Name=KeyName  or Name="KeyName"
        var pattern = new Regex(
            @"ResourceString\s+Name\s*=\s*(?:""([^""]+)""|([A-Za-z0-9_\.]+))",
            RegexOptions.Compiled);
        return DiscoverKeysFromSources(mainAppRoot, "*.xaml", pattern);
    }

    private static List<string> DiscoverKeysFromSources(string mainAppRoot, string searchPattern, Regex pattern)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in Directory.EnumerateFiles(mainAppRoot, searchPattern, SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (Match match in pattern.Matches(text))
            {
                var key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(key))
                {
                    keys.Add(key);
                }
            }
        }

        return keys.OrderBy(x => x).ToList();
    }

    private static string LocateMainAppRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "BSH.MainApp");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            var sibling = Path.Combine(dir.FullName, "..", "BSH.MainApp");
            var siblingFull = Path.GetFullPath(sibling);
            if (Directory.Exists(siblingFull))
            {
                return siblingFull;
            }

            dir = dir.Parent;
        }

        // Fallback for source-tree relative layout used by IDE runners.
        var fromCwd = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "BSH.MainApp"));
        if (Directory.Exists(fromCwd))
        {
            return fromCwd;
        }

        throw new DirectoryNotFoundException("Could not locate BSH.MainApp directory for resource parity tests.");
    }
}
