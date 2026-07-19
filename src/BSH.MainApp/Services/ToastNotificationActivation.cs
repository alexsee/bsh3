// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Web;

namespace BSH.MainApp.Services;

/// <summary>
/// Maps toast activation arguments to a navigation page key.
/// Unknown or missing payloads resolve to overview.
/// </summary>
public static class ToastNotificationActivation
{
    public const string ActionOverview = "overview";
    public const string ActionSettings = "settings";
    public const string ActionBackupResult = "backupResult";

    public const string PageKeyOverview = "BSH.MainApp.ViewModels.MainViewModel";
    public const string PageKeySettings = "BSH.MainApp.ViewModels.SettingsViewModel";

    public static string ResolvePageKey(string? arguments)
    {
        try
        {
            var action = HttpUtility.ParseQueryString(arguments ?? string.Empty).Get("action");

            return action switch
            {
                ActionSettings => PageKeySettings,
                _ => PageKeyOverview,
            };
        }
        catch
        {
            return PageKeyOverview;
        }
    }
}
