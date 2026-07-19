// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Services;
using NUnit.Framework;

namespace BSH.Test;

public class ToastNotificationActivationTests
{
    [TestCase(null, ToastNotificationActivation.PageKeyOverview)]
    [TestCase("", ToastNotificationActivation.PageKeyOverview)]
    [TestCase("action=overview", ToastNotificationActivation.PageKeyOverview)]
    [TestCase("action=settings", ToastNotificationActivation.PageKeySettings)]
    [TestCase("action=backupResult", ToastNotificationActivation.PageKeyOverview)]
    [TestCase("action=unknown", ToastNotificationActivation.PageKeyOverview)]
    [TestCase("not-a-query", ToastNotificationActivation.PageKeyOverview)]
    public void ResolvePageKeyMapsArgumentsToSafeNavigationTargets(string? arguments, string expectedPageKey)
    {
        Assert.That(ToastNotificationActivation.ResolvePageKey(arguments), Is.EqualTo(expectedPageKey));
    }

    [Test]
    public void ResolvePageKeyDoesNotThrowOnMalformedArguments()
    {
        Assert.DoesNotThrow(() => ToastNotificationActivation.ResolvePageKey("action=%zz&%%%"));
    }
}
