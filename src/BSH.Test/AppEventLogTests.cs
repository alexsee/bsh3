// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Threading.Tasks;
using BSH.MainApp.Services;
using NUnit.Framework;
using Serilog;

namespace BSH.Test;

public class AppEventLogTests
{
    [Test]
    public void InitializeWritesDatedLogLineToDisk()
    {
        var directory = CreateTempDirectory();
        var previous = Log.Logger;

        try
        {
            AppEventLog.Initialize(directory, "Backup Service Home", "3.9.0.3");
            Log.CloseAndFlush();

            var logFile = AppEventLog.GetFilePath(directory, DateTime.Now);
            Assert.That(File.Exists(logFile), Is.True, $"Expected event log at {logFile}");

            var contents = File.ReadAllText(logFile);
            Assert.That(contents, Does.Contain("Backup Service Home"));
            Assert.That(contents, Does.Contain("3.9.0.3"));
            Assert.That(contents, Does.Contain("started"));
        }
        finally
        {
            Log.CloseAndFlush();
            Log.Logger = previous;
            DeleteDirectory(directory);
        }
    }

    [Test]
    public async Task UnhandledExceptionIsWrittenToTheSameEventLogFile()
    {
        var directory = CreateTempDirectory();
        var previous = Log.Logger;

        try
        {
            AppEventLog.Initialize(directory, "Backup Service Home", "3.9.0.3");

            var handler = new UnhandledExceptionHandler(
                askContinueAsync: (_, _) => Task.FromResult(true),
                exitApplication: () => { });

            await handler.HandleAsync(new InvalidOperationException("collectable crash for support"));
            await Log.CloseAndFlushAsync();

            var contents = await File.ReadAllTextAsync(AppEventLog.GetFilePath(directory, DateTime.Now));
            Assert.That(contents, Does.Contain("collectable crash for support"));
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            Log.Logger = previous;
            DeleteDirectory(directory);
        }
    }

    [Test]
    public void GetFilePathUsesLegacyDatedEventLogName()
    {
        var path = AppEventLog.GetFilePath(@"C:\Alexosoft\Backup Service Home 3", new DateTime(2026, 8, 16));
        Assert.That(path, Is.EqualTo(@"C:\Alexosoft\Backup Service Home 3\log20260816.txt"));
    }

    [Test]
    public void GetCurrentFilePathUsesTheDatabaseFolder()
    {
        var path = AppEventLog.GetCurrentFilePath(
            @"C:\Alexosoft\Backup Service Home 3\backupservicehome.bshdb",
            new DateTime(2026, 8, 16));

        Assert.That(path, Is.EqualTo(@"C:\Alexosoft\Backup Service Home 3\log20260816.txt"));
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BSH.Test", $"eventlog-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Temp cleanup is best-effort; locked log files should not fail the test.
        }
    }
}
