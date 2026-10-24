// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Services;
using Microsoft.Win32;
using NUnit.Framework;

namespace BSH.Test;

public class RegistryStartupLaunchAdapterTests
{
    [Test]
    [Platform("Win")]
    public void EnablingStartupWritesTheAutostartArgument()
    {
        const string runKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using var originalKey = Registry.CurrentUser.OpenSubKey(runKeyPath);
        var originalValue = originalKey?.GetValue(RegistryStartupLaunchAdapter.ValueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
        var originalKind = originalValue is null ? RegistryValueKind.String : originalKey.GetValueKind(RegistryStartupLaunchAdapter.ValueName);
        var adapter = new RegistryStartupLaunchAdapter(() => @"C:\Preview\BSH.MainApp.exe");

        try
        {
            Assert.That(adapter.TrySetEnabled(true), Is.True);

            using var key = Registry.CurrentUser.OpenSubKey(runKeyPath);
            var value = key?.GetValue(RegistryStartupLaunchAdapter.ValueName) as string;
            Assert.That(value, Is.EqualTo("\"C:\\Preview\\BSH.MainApp.exe\" -autostart"));
        }
        finally
        {
            using var key = Registry.CurrentUser.OpenSubKey(runKeyPath, writable: true);
            if (originalValue is null)
            {
                key?.DeleteValue(RegistryStartupLaunchAdapter.ValueName, throwOnMissingValue: false);
            }
            else
            {
                key?.SetValue(RegistryStartupLaunchAdapter.ValueName, originalValue, originalKind);
            }
        }
    }
}
