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

/// <summary>
/// Startup decisions for the WinUI Preview. Kept separate from <see cref="App"/>
/// so tests can run them without a window or a second process.
/// </summary>
public static class PreviewAppStartup
{
    public static bool ShouldExit(
        PreviewStartupResult startup,
        bool autoStart,
        Action<string, string> showMessage,
        Func<string, string, string> localize)
    {
        if (startup != PreviewStartupResult.BlockedByOtherEngine)
        {
            return startup == PreviewStartupResult.ActivateExisting;
        }

        if (autoStart)
        {
            Log.Information("Preview autostart exited because the stable app already holds the shared database.");
            return true;
        }

        showMessage(
            localize("Preview_EngineBusy_Title", "Backup Service Home 4"),
            localize(
                "Preview_EngineBusy_Text",
                "Backup Service Home is already running. Preview uses the same database and will not start while that app is running."));
        return true;
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

    public static string LocalizedOrFallback(string key, string fallback)
    {
        var value = key.GetLocalized();
        return string.IsNullOrEmpty(value) || value == key ? fallback : value;
    }
}
