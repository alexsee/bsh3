// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;

public class SmartPreviewHost : ISmartPreviewHost
{
    public async Task ShowAsync(string filePath, bool isTemporary)
    {
        var executablePath = Path.Combine(AppContext.BaseDirectory, "SmartPreview.exe");
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException("SmartPreview.exe was not found.", executablePath);
        }

        var arguments = " -file:\"" + filePath + "\"" + (isTemporary ? " -c" : "");
        var process = Process.Start(new ProcessStartInfo(executablePath, arguments)
        {
            WindowStyle = ProcessWindowStyle.Normal
        });

        if (process == null)
        {
            throw new InvalidOperationException("Failed to start SmartPreview.exe.");
        }

        await process.WaitForExitAsync();
    }
}
