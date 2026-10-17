// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Runtime;
using BSH.MainApp;
using BSH.Test.Helpers;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewAppStartupTests
{
    [TestCase("10")]
    [TestCase("not-a-version")]
    public async Task StartupRejectsAnUnsupportedSchemaBeforeChangingConfiguration(string schemaVersion)
    {
        var directory = Path.Combine(Path.GetTempPath(), "BSH.Test", "preview-startup-" + Guid.NewGuid().ToString("N"));
        var database = Path.Combine(directory, "backupservicehome.bshdb");
        try
        {
            await LegacyBackupDatabaseBuilder.CreateAsync(database, schemaVersion);
            var factory = new DbClientFactory();
            await factory.InitializeAsync(database);
            await factory.ExecuteNonQueryAsync("INSERT INTO configuration VALUES ('ftppass', 'legacy-plaintext');");
            DbClientFactory.ClosePool();
            var before = await File.ReadAllBytesAsync(database);
            var configuration = new ConfigurationManager(factory);
            var migration = new DbMigrationService(factory, configuration);
            var messages = new List<string>();

            var result = await PreviewAppStartup.OpenDatabaseAsync(
                database,
                factory,
                configuration,
                migration,
                (_, text) => messages.Add(text),
                (_, fallback) => fallback);

            Assert.That(result, Is.False);
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages[0], Does.Contain("newer"));
            DbClientFactory.ClosePool();
            Assert.That(await File.ReadAllBytesAsync(database), Is.EqualTo(before));
            Assert.That(Directory.GetFiles(directory, "*.bak"), Is.Empty);
        }
        finally
        {
            DbClientFactory.ClosePool();
            TempDirectory.DeleteBestEffort(directory);
        }
    }

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
        Assert.That(caption, Is.EqualTo("Backup Service Home 4"));
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

    [TestCase(null)]
    [TestCase("8")]
    [TestCase("9")]
    public async Task ACompatibleDatabaseOpens(string schemaVersion)
    {
        var directory = Path.Combine(Path.GetTempPath(), "BSH.Test", "preview-compatible-" + Guid.NewGuid().ToString("N"));
        var database = Path.Combine(directory, "backupservicehome.bshdb");
        try
        {
            if (schemaVersion != null)
            {
                await LegacyBackupDatabaseBuilder.CreateAsync(database, schemaVersion);
            }

            var factory = new DbClientFactory();
            var configuration = new ConfigurationManager(factory);
            var result = await PreviewAppStartup.OpenDatabaseAsync(
                database,
                factory,
                configuration,
                new DbMigrationService(factory, configuration),
                (_, _) => Assert.Fail("A compatible database does not show a dialog."),
                (_, fallback) => fallback);

            Assert.That(result, Is.True);
            Assert.That(configuration.DBVersion, Is.EqualTo("9"));
            Assert.That(Directory.GetFiles(directory, "*.bak"), Has.Length.EqualTo(schemaVersion == "8" ? 1 : 0));
        }
        finally
        {
            DbClientFactory.ClosePool();
            TempDirectory.DeleteBestEffort(directory);
        }
    }

    [Test]
    public async Task OtherDatabaseErrorsAreNotHidden()
    {
        var database = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(database, "not a database");
            var factory = new DbClientFactory();
            var configuration = new ConfigurationManager(factory);
            Assert.ThrowsAsync<SQLiteException>(async () => await PreviewAppStartup.OpenDatabaseAsync(
                database,
                factory,
                configuration,
                new DbMigrationService(factory, configuration),
                (_, _) => Assert.Fail("Storage errors must propagate."),
                (_, fallback) => fallback));
        }
        finally
        {
            DbClientFactory.ClosePool();
            File.Delete(database);
        }
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
        var value = PreviewAppStartup.LocalizedOrFallback("Preview_EngineBusy_Title", "Backup Service Home 4");
        Assert.That(value, Is.Not.Empty);
    }
}
