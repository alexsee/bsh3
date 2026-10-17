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
        var adapter = new RegistryStartupLaunchAdapter(() => @"C:\Preview\BSH.MainApp.exe");

        try
        {
            Assert.That(adapter.TrySetEnabled(true), Is.True);

            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            var value = key?.GetValue(RegistryStartupLaunchAdapter.ValueName) as string;
            Assert.That(value, Is.EqualTo("\"C:\\Preview\\BSH.MainApp.exe\" -autostart"));
        }
        finally
        {
            adapter.TrySetEnabled(false);
        }
    }
}
