// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Contracts.Services;

namespace BSH.MainApp.Services;
public class OrchestrationService : IOrchestrationService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IStatusService statusService;
    private readonly IScheduledBackupService scheduledBackupService;

    public OrchestrationService(
        IConfigurationManager configurationManager,
        IStatusService statusService,
        IScheduledBackupService scheduledBackupService)
    {
        this.configurationManager = configurationManager;
        this.statusService = statusService;
        this.scheduledBackupService = scheduledBackupService;
    }

    public async Task InitializeAsync()
    {
        // check if database is configured
        if (configurationManager.IsConfigured == "0")
        {
            return;
        }

        await StartAsync();
    }

    public async Task StartAsync(bool turnOn = false)
    {
        if (turnOn || configurationManager.DbStatus == "0")
        {
            // start system

            // is system turned on?
            if (turnOn)
            {
                configurationManager.DbStatus = "0";
            }

            // TODO: implement battery check

            // report system start
            statusService.SetSystemStatus(Models.SystemStatus.ACTIVATED);

            // start scheduled backups
            await scheduledBackupService.StartAsync();

            // check free space
            // TODO: implement free disk space notification

            // check if backup is running
            if (statusService.IsTaskRunning())
            {
                return;
            }

            // check unexpected end of last backup
            CheckForLastException();

            // TODO: implement remind last backup notification
        }
        else
        {
            statusService.SetSystemStatus(Models.SystemStatus.DEACTIVATED);
        }
    }

    public async Task StopAsync(bool turnOff = false)
    {
        if (turnOff)
        {
            configurationManager.DbStatus = "1";
        }

        // stop all services
        scheduledBackupService.Stop();

        // set status to deactivated
        statusService.SetSystemStatus(Models.SystemStatus.DEACTIVATED);
    }

    private void CheckForLastException()
    {
        if (!string.IsNullOrEmpty(configurationManager.LastVersionDate))
        {
            configurationManager.LastVersionDate = "";
        }
    }
}
