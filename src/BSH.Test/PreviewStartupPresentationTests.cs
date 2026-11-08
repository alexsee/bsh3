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
            (Show: () => order.Add("show"), Activate: () => order.Add("activate"), MoveToTop: () => order.Add("top")));

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
            (Show: () => order.Add("show"), Activate: () => order.Add("activate"), MoveToTop: () => order.Add("top")));

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
            (Show: () => order.Add("show"), Activate: () => order.Add("activate"), MoveToTop: () => order.Add("top")));

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
            (Show: () => shown = true, Activate: () => { }, MoveToTop: () => { })));

        Assert.That(shown, Is.False);
    }

    [Test]
    public void APromptOpenedBeforeTheExplicitShowDoesNotHideAfterwards()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();
        var hidden = 0;
        var shown = 0;
        var dialog = gate.Begin(windowIsVisible: false, () => shown++, () => hidden++);

        Assert.That(shown, Is.EqualTo(1));
        Assert.That(gate.UserVisible, Is.False);

        gate.MarkExplicitlyShown();
        dialog.Dispose();

        Assert.That(hidden, Is.EqualTo(0));
        Assert.That(gate.UserVisible, Is.True);
        Assert.That(gate.OpenDialogs, Is.EqualTo(0));
    }

    [Test]
    public void AnEarlierDialogDoesNotHideWhileAnotherDialogIsOpen()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();
        var hidden = 0;
        var first = gate.Begin(windowIsVisible: false, () => { }, () => hidden++);
        var second = gate.Begin(windowIsVisible: true, () => { }, () => hidden++);

        first.Dispose();
        Assert.That(hidden, Is.EqualTo(0));
        Assert.That(gate.OpenDialogs, Is.EqualTo(1));

        second.Dispose();
        Assert.That(hidden, Is.EqualTo(1));
        Assert.That(gate.OpenDialogs, Is.EqualTo(0));
    }

    [Test]
    public void ClosingToTrayLetsTheNextPromptHideTheWindowAgain()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();
        var hidden = 0;
        gate.MarkExplicitlyShown();
        gate.MarkClosedToTray();

        using (gate.Begin(windowIsVisible: false, () => { }, () => hidden++))
        {
        }

        Assert.That(gate.UserVisible, Is.False);
        Assert.That(hidden, Is.EqualTo(1));
    }

    [Test]
    public void ReleasingADialogTwiceHidesOnlyOnce()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();
        var hidden = 0;
        var dialog = gate.Begin(windowIsVisible: false, () => { }, () => hidden++);

        dialog.Dispose();
        dialog.Dispose();

        Assert.That(hidden, Is.EqualTo(1));
        Assert.That(gate.OpenDialogs, Is.EqualTo(0));

        using (gate.Begin(windowIsVisible: false, () => { }, () => hidden++))
        {
        }

        Assert.That(hidden, Is.EqualTo(2));
        Assert.That(gate.OpenDialogs, Is.EqualTo(0));
    }

    [Test]
    public void AStartupShowThatHasSucceededStaysUpAfterAPrompt()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();
        var hidden = 0;
        gate.MarkExplicitlyShown();

        using (gate.Begin(windowIsVisible: true, () => { }, () => hidden++))
        {
        }

        Assert.That(hidden, Is.EqualTo(0));
        Assert.That(gate.UserVisible, Is.True);
    }

    [Test]
    public void AFailedTemporaryShowReleasesTheDialogCount()
    {
        var gate = new PreviewAppStartup.DialogOwnerGate();

        Assert.Throws<InvalidOperationException>(() => gate.Begin(
            windowIsVisible: false,
            () => throw new InvalidOperationException("show"),
            () => { }));

        Assert.That(gate.OpenDialogs, Is.EqualTo(0));
    }
}
