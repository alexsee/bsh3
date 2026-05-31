// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using Serilog;

namespace BSH.MainApp.Services;

public sealed class CompletionActionService : ICompletionActionService
{
    private readonly ILogger logger = Log.ForContext<CompletionActionService>();

    public Task ExecuteAsync(TaskCompleteAction action)
    {
        switch (action)
        {
            case TaskCompleteAction.ShutdownPC:
                logger.Debug("Computer will be shutdown after task has finished.");
                Process.Start("shutdown.exe", "-s -t 60 -c \"Backup Service Home 3 task finished.\"");
                break;
            case TaskCompleteAction.HibernatePC:
                logger.Debug("Computer will be hibernated after task has finished.");
                Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState");
                break;
            case TaskCompleteAction.NoAction:
                logger.Debug("No completion action selected after task has finished.");
                break;
        }

        return Task.CompletedTask;
    }
}
