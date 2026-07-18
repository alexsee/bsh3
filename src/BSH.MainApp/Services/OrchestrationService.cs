// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Contracts;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;

namespace BSH.MainApp.Services;

public class OrchestrationService : IOrchestrationService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IStatusService statusService;
    private readonly IScheduledBackupService scheduledBackupService;
    private readonly IQueryManager queryManager;
    private readonly IAppNotificationService appNotificationService;
    private readonly IPowerStatusService powerStatusService;

    public OrchestrationService(
        IConfigurationManager configurationManager,
        IStatusService statusService,
        IScheduledBackupService scheduledBackupService,
        IQueryManager queryManager,
        IAppNotificationService appNotificationService,
        IPowerStatusService powerStatusService)
    {
        this.configurationManager = configurationManager;
        this.statusService = statusService;
        this.scheduledBackupService = scheduledBackupService;
        this.queryManager = queryManager;
        this.appNotificationService = appNotificationService;
        this.powerStatusService = powerStatusService;
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

            await ApplyAutomationPolicyAsync(stopFirst: false);

            // check free space
            CheckFreeDiskSpaceNotification();

            // check if backup is running
            if (statusService.IsTaskRunning())
            {
                return;
            }

            // check unexpected end of last backup
            CheckForLastException();

            await CheckOutdatedBackupNotificationAsync();
        }
        else
        {
            statusService.SetSystemStatus(SystemStatus.DEACTIVATED);
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
        if (turnOff)
        {
            StopPowerStatusMonitoring();
        }

        // set status to deactivated
        statusService.SetSystemStatus(SystemStatus.DEACTIVATED);
    }

    public async Task RefreshAutomationAsync()
    {
        if (configurationManager.DbStatus != "0")
        {
            return;
        }

        await ApplyAutomationPolicyAsync(stopFirst: true);
    }

    private async Task ApplyAutomationPolicyAsync(bool stopFirst)
    {
        if (stopFirst)
        {
            scheduledBackupService.Stop();
        }

        UpdatePowerStatusMonitoring();

        if (ShouldPauseForBattery())
        {
            statusService.SetSystemStatus(SystemStatus.PAUSED_DUE_TO_BATTERY);
            return;
        }

        statusService.SetSystemStatus(SystemStatus.ACTIVATED);
        await scheduledBackupService.StartAsync();
    }

    private void CheckForLastException()
    {
        if (!string.IsNullOrEmpty(configurationManager.LastVersionDate))
        {
            configurationManager.LastVersionDate = "";
        }
    }

    private bool ShouldPauseForBattery()
    {
        return configurationManager.DeativateAutoBackupsWhenAkku == "1" &&
            powerStatusService.IsRunningOnBattery;
    }

    private void UpdatePowerStatusMonitoring()
    {
        if (configurationManager.DeativateAutoBackupsWhenAkku == "1")
        {
            StartPowerStatusMonitoring();
        }
        else
        {
            StopPowerStatusMonitoring();
        }
    }

    private void StartPowerStatusMonitoring()
    {
        powerStatusService.StartPowerStatusMonitoring(OnPowerStatusChanged);
    }

    private void StopPowerStatusMonitoring()
    {
        powerStatusService.StopPowerStatusMonitoring(OnPowerStatusChanged);
    }

    private async void OnPowerStatusChanged(object? sender, EventArgs e)
    {
        if (configurationManager.DeativateAutoBackupsWhenAkku != "1")
        {
            StopPowerStatusMonitoring();
            return;
        }

        await RefreshAutomationAsync();
    }

    private void CheckFreeDiskSpaceNotification()
    {
        if (!double.TryParse(configurationManager.RemindSpace, out var reminderMegabytes) ||
            reminderMegabytes < 0 ||
            !double.TryParse(configurationManager.FreeSpace, out var freeBytes) ||
            freeBytes == 0)
        {
            return;
        }

        if (freeBytes < reminderMegabytes * 1024L * 1024L)
        {
            appNotificationService.Show(ToastNotificationPayload.Create(
                "Not enough disk space.",
                "There is not enough storage space left on the backup medium. Delete backups or switch to a new medium."));
        }
    }

    private async Task CheckOutdatedBackupNotificationAsync()
    {
        if (!int.TryParse(configurationManager.RemindAfterDays, out var remindAfterDays))
        {
            return;
        }

        var lastBackup = await queryManager.GetLastBackupAsync();
        if (lastBackup == null ||
            DateTime.Now.Subtract(lastBackup.CreationDate).Days <= remindAfterDays ||
            queryManager.GetVersions().Count == 0)
        {
            return;
        }

        var days = DateTime.Now.Subtract(lastBackup.CreationDate).Days;
        appNotificationService.Show(ToastNotificationPayload.Create(
            "Backup outdated",
            $"Your last data backup is already {days} days old. Perform a data backup to ensure your current files are backed up."));
    }
}
