// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Serilog;
using Serilog.Events;

namespace BSH.MainApp.Services;

/// <summary>
/// Configures the dated AppData event log that Show Event Logs opens, matching the WinForms file sink.
/// </summary>
public static class AppEventLog
{
    public static string GetDirectory(string databaseFile)
    {
        var directory = Path.GetDirectoryName(databaseFile);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("Database path has no directory.", nameof(databaseFile));
        }

        return directory;
    }

    public static string GetFilePath(string directory, DateTime date) =>
        Path.Combine(directory, $"log{date:yyyyMMdd}.txt");

    public static string GetCurrentFilePath(string databaseFile, DateTime date) =>
        GetFilePath(GetDirectory(databaseFile), date);

    public static void Initialize(string directory, string appTitle, string version)
    {
        Directory.CreateDirectory(directory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("FtpClient", LogEventLevel.Warning)
            .WriteTo.File(
                path: Path.Combine(directory, "log.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        Log.Information("{AppTitle} {Version} started.", appTitle, version);
    }
}
