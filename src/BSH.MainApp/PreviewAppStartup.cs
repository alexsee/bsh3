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
    /// Tracks whether the user currently wants the main window to stay visible,
    /// and how many dialogs are using it as an owner. Visibility is recorded only
    /// after a successful explicit show, and the window is hidden only when the
    /// last dialog releases it.
    /// </summary>
    public sealed class DialogOwnerGate
    {
        private int openDialogs;
        private bool userVisible;

        public int OpenDialogs => openDialogs;

        public bool UserVisible => userVisible;

        public void MarkExplicitlyShown()
        {
            userVisible = true;
        }

        public void MarkClosedToTray()
        {
            userVisible = false;
        }

        public Lease Begin(bool windowIsVisible, Action showTemporary, Action hide)
        {
            ArgumentNullException.ThrowIfNull(showTemporary);
            ArgumentNullException.ThrowIfNull(hide);
            openDialogs++;
            try
            {
                if (!windowIsVisible)
                {
                    showTemporary();
                }
            }
            catch
            {
                openDialogs--;
                throw;
            }

            return new Lease(this, hide);
        }

        private void Release(Action hide)
        {
            openDialogs--;
            if (openDialogs == 0 && !userVisible)
            {
                hide();
            }
        }

        public sealed class Lease : IDisposable
        {
            private DialogOwnerGate gate;
            private readonly Action hide;

            internal Lease(DialogOwnerGate gate, Action hide)
            {
                this.gate = gate;
                this.hide = hide;
            }

            public void Dispose()
            {
                var current = Interlocked.Exchange(ref gate, null);
                current?.Release(hide);
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
