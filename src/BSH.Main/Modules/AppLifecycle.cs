// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;
using Serilog;

namespace Brightbits.BSH.Main;

/// <summary>
/// Shared application shutdown helpers for the WinForms host.
/// </summary>
internal static class AppLifecycle
{
    public static void Exit(bool closeWindows = true)
    {
        NotificationController.Current.Shutdown();
        BackupLogic.StopSystem();

        if (closeWindows)
        {
            PresentationController.Current.CloseMainWindow();
            PresentationController.Current.CloseBackupBrowserWindow();
        }

        Log.Information("Backup Service Home stopped");

        try
        {
            Application.Exit();
        }
        catch
        {
            Environment.Exit(0);
        }
    }
}
