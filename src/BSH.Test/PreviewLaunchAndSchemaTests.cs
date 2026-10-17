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
        Assert.That(PreviewLaunch.IsAutoStart(null), Is.False);
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

    [Test]
    public void TimestampedBackupRejectsAMissingDatabase()
    {
        Assert.Throws<ArgumentException>(() => DatabaseFileBackup.CreateTimestampedCopy(null, DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() => DatabaseFileBackup.CreateTimestampedCopy("   ", DateTime.UtcNow));
        Assert.Throws<FileNotFoundException>(() => DatabaseFileBackup.CreateTimestampedCopy(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bshdb"),
            DateTime.UtcNow));
    }

    [Test]
    public void TimestampedBackupUsesTheWorkingDirectoryWhenThePathHasNone()
    {
        var directory = Path.Combine(Path.GetTempPath(), "bsh-rel-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var previous = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(directory);

        try
        {
            File.WriteAllText("backup.bshdb", "database");
            var backup = DatabaseFileBackup.CreateTimestampedCopy("backup.bshdb", new DateTime(2026, 10, 17, 9, 35, 0));

            Assert.That(File.ReadAllText(backup), Is.EqualTo("database"));
            Assert.That(Path.GetFileName(backup), Is.EqualTo("backup.bshdb.20261017-093500.bak"));
        }
        finally
        {
            Directory.SetCurrentDirectory(previous);
            Directory.Delete(directory, recursive: true);
        }
    }
}
