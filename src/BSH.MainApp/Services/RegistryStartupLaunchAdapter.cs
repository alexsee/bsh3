// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Security;
using BSH.MainApp.Contracts.Services;
using Microsoft.Win32;

namespace BSH.MainApp.Services;

public sealed class RegistryStartupLaunchAdapter : IStartupLaunchAdapter
{
    public const string ValueName = "BackupServiceHome3Run";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private readonly Func<string> getExecutablePath;

    public RegistryStartupLaunchAdapter(Func<string>? getExecutablePath = null)
    {
        this.getExecutablePath = getExecutablePath ?? (() => Environment.ProcessPath ?? AppContext.BaseDirectory);
    }

    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            var value = key?.GetValue(ValueName);
            return value is string text && !string.IsNullOrEmpty(text);
        }
        catch (SecurityException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool TrySetEnabled(bool enabled)
    {
        try
        {
            if (enabled)
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                    ?? throw new InvalidOperationException("Unable to open startup registry key.");
                var executablePath = getExecutablePath();
                key.SetValue(ValueName, $"\"{executablePath}\" -delayedstart");
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
                key?.DeleteValue(ValueName, throwOnMissingValue: false);
            }

            return true;
        }
        catch (SecurityException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
