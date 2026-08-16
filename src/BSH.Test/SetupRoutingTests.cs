// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Services;
using BSH.MainApp.ViewModels;
using NUnit.Framework;

namespace BSH.Test;

public class SetupRoutingTests
{
    [Test]
    public void CanNavigateToAllowsOnlySetupWhileUnconfigured()
    {
        var setupPage = typeof(SetupViewModel).FullName!;

        Assert.That(SetupRouting.CanNavigateTo(setupPage, "0"), Is.True);
        Assert.That(SetupRouting.CanNavigateTo(typeof(MainViewModel).FullName!, "0"), Is.False);
        Assert.That(SetupRouting.CanNavigateTo(typeof(BrowserViewModel).FullName!, "0"), Is.False);
        Assert.That(SetupRouting.CanNavigateTo(typeof(SettingsViewModel).FullName!, "0"), Is.False);
    }

    [Test]
    public void CanNavigateToAllowsAppPagesOnceConfigured()
    {
        Assert.That(SetupRouting.CanNavigateTo(typeof(MainViewModel).FullName!, "1"), Is.True);
        Assert.That(SetupRouting.CanNavigateTo(typeof(BrowserViewModel).FullName!, "1"), Is.True);
        Assert.That(SetupRouting.CanNavigateTo(typeof(SettingsViewModel).FullName!, "1"), Is.True);
        Assert.That(SetupRouting.CanNavigateTo(typeof(SetupViewModel).FullName!, "1"), Is.True);
    }
}
