// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Globalization;
using ServiceWire;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BSH.Service;

internal sealed class ServiceWireLogger(ILogger<WindowsBackgroundService> logger) : ILog
{
    // ServiceWire debug output contains serialized RPC payloads (including file
    // paths). Keep it out of service logs, even when debug logging is enabled.
    public void Debug(string formattedMessage, params object[] args) { }

    public void Info(string formattedMessage, params object[] args) => Write(LogLevel.Information, formattedMessage, args);

    public void Warn(string formattedMessage, params object[] args) => Write(LogLevel.Warning, formattedMessage, args);

    public void Error(string formattedMessage, params object[] args) => Write(LogLevel.Error, formattedMessage, args);

    public void Fatal(string formattedMessage, params object[] args) => Write(LogLevel.Critical, formattedMessage, args);

    private void Write(LogLevel level, string format, object[] args)
    {
        if (logger.IsEnabled(level))
        {
            var message = args.Length == 0 ? format : string.Format(CultureInfo.InvariantCulture, format, args);
            logger.Log(level, "ServiceWire: {Message}", message);
        }
    }
}
