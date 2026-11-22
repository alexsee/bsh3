// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Services;
using BSH.MainApp.Contracts.Services;
using Serilog;

namespace BSH.MainApp.Services;

public class ScheduledBackupService : IScheduledBackupService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IJobService jobService;
    private readonly IQueryManager queryManager;
    private readonly IScheduleRepository scheduleRepository;
    private readonly ISchedulerAdapterFactory schedulerAdapterFactory;
    private readonly ScheduleSettingsService scheduleSettingsService;
    private readonly MediaArrivalBackupWatch mediaArrivalWatch;

    private ISchedulerAdapter? schedulerService;

    public ScheduledBackupService(
        IConfigurationManager configurationManager,
        IJobService jobService,
        IQueryManager queryManager,
        IScheduleRepository scheduleRepository,
        ISchedulerAdapterFactory schedulerAdapterFactory,
        ScheduleSettingsService scheduleSettingsService,
        IMediaWatcherFactory mediaWatcherFactory)
    {
        this.configurationManager = configurationManager;
        this.jobService = jobService;
        this.queryManager = queryManager;
        this.scheduleRepository = scheduleRepository;
        this.schedulerAdapterFactory = schedulerAdapterFactory;
        this.scheduleSettingsService = scheduleSettingsService;
        ArgumentNullException.ThrowIfNull(mediaWatcherFactory);
        mediaArrivalWatch = new MediaArrivalBackupWatch(configurationManager, mediaWatcherFactory);
    }

    public async Task StartAsync()
    {
        if (configurationManager.TaskType == TaskType.Auto)
        {
            StartFullAutomatedSystem();
        }
        else if (configurationManager.TaskType == TaskType.Schedule)
        {
            await StartScheduleSystem();
        }
    }

    public void Stop()
    {
        if (schedulerService != null)
        {
            schedulerService.Stop();
            schedulerService = null;
            Log.Information("Backup scheduler is stopped.");
        }

        mediaArrivalWatch.Stop();
    }

    public DateTime GetNextBackupDate()
    {
        if (schedulerService == null)
        {
            return DateTime.MaxValue;
        }

        return schedulerService.GetNextRun();
    }

    public async Task<bool> HasScheduleEntriesAsync()
    {
        return await scheduleRepository.HasScheduleEntriesAsync();
    }


    #region  Full Automated System 

    private void StartFullAutomatedSystem()
    {
        Log.Information("Service for \"Full automatic backup\" is started.");

        schedulerService = schedulerAdapterFactory.Create();
        schedulerService.Start();
        schedulerService.ScheduleAutoBackup(RunAutoBackup);

        WatchForDueBackup(RunAutoBackup);
    }

    private async Task RunAutoBackup()
    {
        Log.Information("Automatic backup is scheduled and will be performed now.");

        mediaArrivalWatch.Stop();

        // lower process priority
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        try
        {
            if (await jobService.CreateBackupAsync("Automatisches Backup", "", false))
            {
                await RemoveOldBackups();
            }
            else
            {
                await WatchIfMediaMissing(RunAutoBackup);
            }
        }
        finally
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
        }
    }

    private async Task RemoveOldBackups()
    {
        IReadOnlyList<VersionDetails> versionsToDelete;

        try
        {
            versionsToDelete = scheduleSettingsService.LoadPolicy()
                .GetAutomaticVersionsToDelete(queryManager.GetVersions(), DateTime.Now);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not determine backups for deletion");
            return;
        }

        foreach (var version in versionsToDelete)
        {
            await jobService.DeleteBackupAsync(version.Id, false);
        }
    }

    #endregion

    #region  Schedule System 

    private async Task StartScheduleSystem()
    {
        Log.Information("Service for \"Scheduled backups\" is started.");

        schedulerService = schedulerAdapterFactory.Create();
        schedulerService.Start();

        var schedules = await scheduleRepository.GetSchedulesAsync();
        var registration = new ScheduledBackupRegistration(configurationManager, queryManager);
        foreach (var schedule in schedules)
        {
            registration.Register(schedule, schedulerService, thScheduleSysRunBackup,
                () => WatchForDueBackup(thScheduleSysRunBackup), DateTime.Now);
        }
    }

    private async Task thScheduleSysRunBackup()
    {
        Log.Information("Scheduled backup is planned and will be performed now.");

        mediaArrivalWatch.Stop();

        // Priorität heruntersetzen
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        try
        {
            var schedulePolicy = scheduleSettingsService.LoadPolicy();
            var lastFullBackup = schedulePolicy.ScheduledFullBackupsEnabled
                ? await queryManager.GetLastFullBackupAsync()
                : null;
            var fullBackupOption = schedulePolicy.ShouldPerformFullBackup(lastFullBackup?.CreationDate, DateTime.Now);

            if (await jobService.CreateBackupAsync("Automatische Sicherung", "", false, fullBackupOption))
            {
                await RemoveOldBackupsScheduled();
            }
            else
            {
                await WatchIfMediaMissing(thScheduleSysRunBackup);
            }
        }
        finally
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
        }
    }

    private void WatchForDueBackup(Func<Task> runBackup)
    {
        mediaArrivalWatch.Start(runBackup, CheckBackupMediaAsync);
    }

    private Task WatchIfMediaMissing(Func<Task> runBackup)
    {
        return mediaArrivalWatch.StartIfMediaMissing(runBackup, CheckBackupMediaAsync);
    }

    private Task<bool> CheckBackupMediaAsync()
    {
        return jobService.CheckMediaAsync(ActionType.Backup, true);
    }

    private async Task RemoveOldBackupsScheduled()
    {
        // lower process priority
        var proc = Process.GetCurrentProcess();
        proc.PriorityClass = ProcessPriorityClass.BelowNormal;

        // obtain versions for deletion
        var listDelete = new List<VersionDetails>();

        try
        {
            listDelete.AddRange(scheduleSettingsService.LoadPolicy()
                .GetVersionsToDelete(queryManager.GetVersions(), DateTime.Now));
        }
        catch (Exception ex)
        {
            // although this is a major issue, we don't want the backup to fail
            Log.Error(ex, "Could not determine backups for deletion");
        }

        // delete versions
        foreach (var version in listDelete)
        {
            await jobService.DeleteBackupAsync(version.Id, false);
        }
    }

    #endregion
}
