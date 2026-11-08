// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using BSH.MainApp;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewStartupPresentationTests
{
    [Test]
    public void ForegroundRequestDuringStartupIsDeferredUntilInitializationFinishes()
    {
        var coordinator = new PreviewForegroundCoordinator();

        Assert.That(coordinator.TryPresentNow(), Is.False);
        Assert.That(coordinator.TryPresentNow(), Is.False);
        Assert.That(coordinator.CompleteStartup(), Is.True);
        Assert.That(coordinator.TryPresentNow(), Is.True);
        Assert.That(coordinator.CompleteStartup(), Is.False);
    }

    [Test]
    public void ForegroundRequestAfterStartupPresentsImmediately()
    {
        var coordinator = new PreviewForegroundCoordinator();

        Assert.That(coordinator.CompleteStartup(), Is.False);
        Assert.That(coordinator.TryPresentNow(), Is.True);
    }

    [Test]
    public void AMissingWindowIsNavigatedBeforeItIsShown()
    {
        var shell = true;
        var order = new List<string>();

        PreviewAppStartup.PresentPreviewWindow(
            () => false,
            () => order.Add("startup"),
            navigateToPage: false,
            () => shell,
            _ => order.Add("nav"),
            "page",
            () => order.Add("show"),
            () => order.Add("activate"),
            () => order.Add("top"));

        Assert.That(order, Is.EqualTo(new[] { "startup", "show", "activate", "top" }));
    }

    [Test]
    public void AnUnconfiguredWindowStaysOnSetupAndIsStillShown()
    {
        var shell = true;
        var order = new List<string>();

        PreviewAppStartup.PresentPreviewWindow(
            () => false,
            () =>
            {
                order.Add("startup");
                shell = false;
            },
            navigateToPage: true,
            () =>
            {
                order.Add("shell");
                return shell;
            },
            key => order.Add("nav:" + key),
            "settings",
            () => order.Add("show"),
            () => order.Add("activate"),
            () => order.Add("top"));

        Assert.That(order, Is.EqualTo(new[] { "startup", "shell", "show", "activate", "top" }));
    }

    [Test]
    public void AHiddenWindowIsShownBeforeActivationAndCanOpenTheRequestedPage()
    {
        var order = new List<string>();

        PreviewAppStartup.PresentPreviewWindow(
            () => true,
            () => order.Add("startup"),
            navigateToPage: true,
            () => true,
            key => order.Add("nav:" + key),
            "overview",
            () => order.Add("show"),
            () => order.Add("activate"),
            () => order.Add("top"));

        Assert.That(order, Is.EqualTo(new[] { "nav:overview", "show", "activate", "top" }));
        Assert.That(order.IndexOf("show"), Is.LessThan(order.IndexOf("activate")));
    }

    [Test]
    public void NavigationFailureDoesNotShowTheWindow()
    {
        var shown = false;

        Assert.Throws<InvalidOperationException>(() => PreviewAppStartup.PresentPreviewWindow(
            () => false,
            () => throw new InvalidOperationException("startup"),
            navigateToPage: false,
            () => true,
            _ => { },
            null,
            () => shown = true,
            () => { },
            () => { }));

        Assert.That(shown, Is.False);
    }
}
