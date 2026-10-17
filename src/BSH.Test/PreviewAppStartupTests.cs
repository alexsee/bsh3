// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewAppStartupTests
{
    [Test]
    public void SecondLaunchExitsWithoutADialog()
    {
        var messages = new List<string>();
        var action = PreviewAppStartup.Decide(
            PreviewStartupResult.ActivateExisting,
            autoStart: false,
            (caption, text) => messages.Add(caption + text),
            (key, fallback) => fallback);

        Assert.That(action, Is.EqualTo(PreviewLaunchAction.Exit));
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void AutoStartExitsQuietlyWhenTheStableAppHoldsTheDatabase()
    {
        var messages = new List<string>();
        var action = PreviewAppStartup.Decide(
            PreviewStartupResult.BlockedByOtherEngine,
            autoStart: true,
            (caption, text) => messages.Add(text),
            (key, fallback) => fallback);

        Assert.That(action, Is.EqualTo(PreviewLaunchAction.Exit));
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void AManualLaunchExplainsThatTheStableAppIsRunning()
    {
        string caption = null;
        string text = null;
        var action = PreviewAppStartup.Decide(
            PreviewStartupResult.BlockedByOtherEngine,
            autoStart: false,
            (shownCaption, shownText) =>
            {
                caption = shownCaption;
                text = shownText;
            },
            (key, fallback) => fallback);

        Assert.That(action, Is.EqualTo(PreviewLaunchAction.Exit));
        Assert.That(caption, Is.EqualTo("Backup Service Home 3 Preview"));
        Assert.That(text, Does.Contain("already running"));
    }

    [Test]
    public void AFreeMutexContinuesStartup()
    {
        var action = PreviewAppStartup.Decide(
            PreviewStartupResult.Started,
            autoStart: false,
            (_, _) => Assert.Fail("No dialog is shown when Preview can start."),
            (_, fallback) => fallback);

        Assert.That(action, Is.EqualTo(PreviewLaunchAction.Continue));
    }

    [Test]
    public async Task ANewerDatabaseIsRefusedAndLeftUnopened()
    {
        var opened = false;
        string text = null;
        var result = await PreviewAppStartup.OpenDatabaseAsync(
            () =>
            {
                opened = true;
                throw new DatabaseIncompatibleException();
            },
            (_, shownText) => text = shownText,
            (_, fallback) => fallback);

        Assert.That(result, Is.False);
        Assert.That(opened, Is.True);
        Assert.That(text, Does.Contain("newer"));
    }

    [Test]
    public async Task ACompatibleDatabaseOpens()
    {
        var result = await PreviewAppStartup.OpenDatabaseAsync(
            () => Task.CompletedTask,
            (_, _) => Assert.Fail("A compatible database does not show a dialog."),
            (_, fallback) => fallback);

        Assert.That(result, Is.True);
    }

    [Test]
    public void OtherDatabaseErrorsAreNotHidden()
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () => await PreviewAppStartup.OpenDatabaseAsync(
            () => throw new InvalidOperationException("disk"),
            (_, _) => { },
            (_, fallback) => fallback));
    }

    [Test]
    public void BringToFrontIgnoresAMissingWindowAndSwallowsActivationErrors()
    {
        Assert.DoesNotThrow(() => PreviewAppStartup.BringToFront(null));

        var activated = false;
        PreviewAppStartup.BringToFront(() => activated = true);
        Assert.That(activated, Is.True);

        Assert.DoesNotThrow(() => PreviewAppStartup.BringToFront(() => throw new InvalidOperationException("no window")));
    }

    [Test]
    public void MissingResourcesUseTheFallbackText()
    {
        var value = PreviewAppStartup.LocalizedOrFallback("Preview_EngineBusy_Title", "Backup Service Home 3 Preview");
        Assert.That(value, Is.Not.Empty);
    }
}
