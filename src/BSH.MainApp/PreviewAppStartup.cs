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

    /// <summary>
    /// Shows the main window on the UI thread. A missing window is navigated first,
    /// so an unconfigured database opens setup. A hidden window is shown before it
    /// is activated. Page navigation runs only when the shell is enabled afterwards.
    /// </summary>
    public static void PresentPreviewWindow(
        Func<bool> windowExists,
        Action navigateForStartup,
        bool navigateToPage,
        Func<bool> shellNavigationEnabled,
        Action<string> navigateTo,
        string pageKey,
        Action show,
        Action activate,
        Action moveToTop)
    {
        ArgumentNullException.ThrowIfNull(windowExists);
        ArgumentNullException.ThrowIfNull(navigateForStartup);
        ArgumentNullException.ThrowIfNull(shellNavigationEnabled);
        ArgumentNullException.ThrowIfNull(show);
        ArgumentNullException.ThrowIfNull(activate);
        ArgumentNullException.ThrowIfNull(moveToTop);

        if (!windowExists())
        {
            navigateForStartup();
        }

        if (navigateToPage && shellNavigationEnabled())
        {
            ArgumentNullException.ThrowIfNull(navigateTo);
            navigateTo(pageKey);
        }

        show();
        activate();
        moveToTop();
    }

    /// <summary>
    /// A background prompt may show the main window only while its dialog is open.
    /// An explicit presentation during that time must keep the window visible.
    /// </summary>
    public static bool ShouldRestoreHiddenOwner(bool temporarilyShown, int epochAtStart, int epochNow)
    {
        return temporarilyShown && epochAtStart == epochNow;
    }

    public readonly struct DialogOwnerLease : IDisposable
    {
        private readonly bool temporarilyShown;
        private readonly int epochAtStart;
        private readonly Func<int> currentEpoch;
        private readonly Action hide;

        public DialogOwnerLease(bool temporarilyShown, int epochAtStart, Func<int> currentEpoch, Action hide)
        {
            this.temporarilyShown = temporarilyShown;
            this.epochAtStart = epochAtStart;
            this.currentEpoch = currentEpoch;
            this.hide = hide;
        }

        public void Dispose()
        {
            if (currentEpoch == null || hide == null)
            {
                return;
            }

            if (ShouldRestoreHiddenOwner(temporarilyShown, epochAtStart, currentEpoch()))
            {
                hide();
            }
        }
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
