// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Runtime;
using NUnit.Framework;

namespace BSH.Test;

public class PreviewLaunchAndSchemaTests
{
    [Test]
    public void AutoStartArgumentIsRecognized()
    {
        Assert.That(PreviewLaunch.IsAutoStart(new[] { "BSH.MainApp.exe", "-autostart" }), Is.True);
        Assert.That(PreviewLaunch.IsAutoStart(new[] { "BSH.MainApp.exe", "/autostart" }), Is.True);
        Assert.That(PreviewLaunch.IsAutoStart(new[] { "BSH.MainApp.exe" }), Is.False);
    }

    [Test]
    public void SchemaLevelMatchesTheShippedStableDatabase()
    {
        Assert.That(SchemaCompatibility.SupportedSchemaVersion, Is.EqualTo(9));
        Assert.That(SchemaCompatibility.WillChangeSchema("9"), Is.False);
        Assert.That(SchemaCompatibility.WillChangeSchema("8"), Is.True);
        Assert.That(SchemaCompatibility.WillChangeSchema(null), Is.True);
        Assert.That(SchemaCompatibility.IsNewerThanSupported("10"), Is.True);
        Assert.That(SchemaCompatibility.IsNewerThanSupported("9"), Is.False);
        Assert.That(SchemaCompatibility.WillChangeSchema("newer"), Is.False);
    }

    [Test]
    public void TimestampedBackupSitsNextToTheDatabase()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bsh-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = Path.Combine(directory, "backupservicehome.bshdb");
        File.WriteAllText(database, "database");
        File.WriteAllText(database + "-wal", "wal");

        try
        {
            var stamp = new DateTime(2026, 10, 17, 9, 35, 0);
            var first = DatabaseFileBackup.CreateTimestampedCopy(database, stamp);
            var second = DatabaseFileBackup.CreateTimestampedCopy(database, stamp);

            Assert.That(first, Is.EqualTo(Path.Combine(directory, "backupservicehome.bshdb.20261017-093500.bak")));
            Assert.That(File.ReadAllText(first), Is.EqualTo("database"));
            Assert.That(File.ReadAllText(first + ".wal"), Is.EqualTo("wal"));
            Assert.That(second, Does.EndWith("backupservicehome.bshdb.20261017-093500-1.bak"));
            Assert.That(File.Exists(database), Is.True);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
