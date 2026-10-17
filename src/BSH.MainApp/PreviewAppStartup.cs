// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Runtime;
using CommunityToolkit.WinUI;
using Serilog;

namespace BSH.MainApp;

public enum PreviewLaunchAction
{
    Continue,
    Exit
}

/// <summary>
/// Startup decisions for the WinUI Preview. Kept separate from <see cref="App"/>
/// so tests can run them without a window or a second process.
/// </summary>
public static class PreviewAppStartup
{
    public static PreviewLaunchAction Decide(
        PreviewStartupResult startup,
        bool autoStart,
        Action<string, string> showMessage,
        Func<string, string, string> localize)
    {
        if (startup == PreviewStartupResult.ActivateExisting)
        {
            return PreviewLaunchAction.Exit;
        }

        if (startup == PreviewStartupResult.BlockedByOtherEngine)
        {
            if (autoStart)
            {
                Log.Information("Preview autostart exited because the stable app already holds the shared database.");
            }
            else
            {
                showMessage(
                    localize("Preview_EngineBusy_Title", "Backup Service Home 4"),
                    localize(
                        "Preview_EngineBusy_Text",
                        "Backup Service Home is already running. Preview uses the same database and will not start while that app is running."));
            }

            return PreviewLaunchAction.Exit;
        }

        return PreviewLaunchAction.Continue;
    }

    public static async Task<bool> OpenDatabaseAsync(
        string databaseFile,
        IDbClientFactory dbClientFactory,
        IConfigurationManager configurationManager,
        IDbMigrationService migrationService,
        Action<string, string> showMessage,
        Func<string, string, string> localize)
    {
        try
        {
            // Loading configuration can persist values, including legacy passwords.
            await SchemaCompatibility.CheckExistingDatabaseAsync(databaseFile);
            await dbClientFactory.InitializeAsync(databaseFile);
            await configurationManager.InitializeAsync();
            await migrationService.InitializeAsync();
            return true;
        }
        catch (DatabaseIncompatibleException)
        {
            showMessage(
                localize("Preview_DatabaseTooNew_Title", "Backup Service Home 4"),
                localize(
                    "Preview_DatabaseTooNew_Text",
                    "This database was written by a newer Backup Service Home and was not opened. Install a newer version."));
            return false;
        }
    }

    public static void BringToFront(Action? activate)
    {
        try
        {
            if (activate == null)
            {
                return;
            }

            activate();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not bring the Preview window to the front.");
        }
    }

    public static string LocalizedOrFallback(string key, string fallback)
    {
        var value = key.GetLocalized();
        return string.IsNullOrEmpty(value) || value == key ? fallback : value;
    }
}
