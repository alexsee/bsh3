// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Diagnostics;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Utils;
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

    private ISchedulerAdapter? schedulerService;

    public ScheduledBackupService(
        IConfigurationManager configurationManager,
        IJobService jobService,
        IQueryManager queryManager,
        IScheduleRepository scheduleRepository,
        ISchedulerAdapterFactory schedulerAdapterFactory,
        ScheduleSettingsService scheduleSettingsService)
    {
        this.configurationManager = configurationManager;
        this.jobService = jobService;
        this.queryManager = queryManager;
        this.scheduleRepository = scheduleRepository;
        this.schedulerAdapterFactory = schedulerAdapterFactory;
        this.scheduleSettingsService = scheduleSettingsService;
    }

    public async Task InitializeAsync()
    {

    }

    public async Task StartAsync()
    {
        // automatic or scheduled backups
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
        StopFullAutomatedSystem();
        StopScheduleSystem();
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

        // start scheduler
        schedulerService = schedulerAdapterFactory.Create();
        schedulerService.Start();
        schedulerService.ScheduleAutoBackup(async () => await RunAutoBackup());
    }

    private void StopFullAutomatedSystem()
    {
        // stop scheduler
        if (schedulerService == null)
        {
            return;
        }

        schedulerService.Stop();
        Log.Information("Service for \"Full automatic backup\" is stopped.");
    }

    private async Task RunAutoBackup()
    {
        Log.Information("Automatic backup is scheduled and will be performed now.");

        // lower process priority
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        try
        {
            if (await jobService.CreateBackupAsync("Automatisches Backup", "", false))
            {
                await RemoveOldBackups();
            }
        }
        finally
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
        }
    }

    private async Task RemoveOldBackups()
    {
        var listDelete = scheduleSettingsService.LoadPolicy()
            .GetAutomaticVersionsToDelete(queryManager.GetVersions(), DateTime.Now);

        // delete old versions
        foreach (var version in listDelete)
        {
            await jobService.DeleteBackupAsync(version.Id, false);
        }
    }

    #endregion

    #region  Schedule System 

    private async Task StartScheduleSystem()
    {
        Log.Information("Service for \"Scheduled backups\" is started.");

        // start scheduler
        schedulerService = schedulerAdapterFactory.Create();
        schedulerService.Start();

        // read scheduler entries in database
        var schedules = await scheduleRepository.GetSchedulesAsync();
        foreach (var schedule in schedules)
        {
            var scheduleDate = schedule.Date;
            var scheduleType = schedule.Type;

            if (scheduleType == 1)
            {
                // once
                if (DateTime.Compare(DateTime.Now, scheduleDate) < 0)
                {
                    schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), scheduleDate);
                }
            }
            else if (scheduleType == 2)
            {
                // hourly
                schedulerService.ScheduleHourly(async () => await thScheduleSysRunBackup(), scheduleDate);
            }
            else if (scheduleType == 3)
            {
                // daily

                // catch up old backup?
                if (DateTime.Now.TimeOfDay > scheduleDate.TimeOfDay && DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                {
                    schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                }

                schedulerService.ScheduleDaily(async () => await thScheduleSysRunBackup(), scheduleDate);
            }
            else if (scheduleType == 4)
            {
                // weekly

                // catch up old backup?
                if (DateTime.Now.DayOfWeek == scheduleDate.DayOfWeek)
                {
                    // backup was performed already earlier
                    if (DateTime.Now.TimeOfDay > scheduleDate.TimeOfDay && DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                    {
                        schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                    }
                }
                else
                {
                    // check of backup was done the last 7 days
                    var versions = queryManager.GetVersions(true);
                    var checkDate = DateUtils.GetDateToWeekDay(scheduleDate.DayOfWeek, DateTime.Now);

                    foreach (var versionDate in versions.Select(x => x.CreationDate))
                    {
                        if (versionDate.Subtract(checkDate).Days < 0)
                        {
                            // backup was performed already earlier
                            if (DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                            {
                                schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                            }

                            break;
                        }

                        if (versionDate.DayOfWeek == scheduleDate.DayOfWeek)
                        {
                            break;
                        }
                    }
                }

                schedulerService.ScheduleWeekly(async () => await thScheduleSysRunBackup(), scheduleDate);
            }
            else if (scheduleType == 5)
            {
                // monthly

                // catch up old backup?
                if (DateTime.Now.Day == scheduleDate.Day)
                {
                    // backup was performed already earlier
                    if (DateTime.Now.TimeOfDay > scheduleDate.TimeOfDay && DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                    {
                        schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                    }
                }
                else
                {
                    // check of backup was done the last 7 days
                    var versions = queryManager.GetVersions(true);
                    var checkDate = DateUtils.GetDateToMonth(scheduleDate.Day, DateTime.Now);

                    foreach (var versionDate in versions.Select(x => x.CreationDate))
                    {
                        // backup was performed already earlier
                        if (versionDate.Subtract(checkDate).Days < 0)
                        {
                            if (DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                            {
                                schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                            }

                            break;
                        }

                        if (versionDate.Day == scheduleDate.Day)
                        {
                            break;
                        }
                    }
                }

                schedulerService.ScheduleMonthly(async () => await thScheduleSysRunBackup(), scheduleDate);
            }
        }

    }

    private bool DoPastBackup(DateTime date, bool orOlder = false)
    {
        if (configurationManager.DoPastBackups != "1")
        {
            return false;
        }

        // backup is after the last backup
        if (!string.IsNullOrEmpty(configurationManager.LastBackupDone) && DateUtils.ReformatVersionDate(configurationManager.LastBackupDone) >= date)
        {
            return false;
        }

        // check if this backup was performed
        foreach (var versionDate in queryManager.GetVersions().Select(x => x.CreationDate))
        {
            if (orOlder)
            {
                if (versionDate >= date)
                {
                    return false;
                }
            }
            else if (versionDate > date.AddMinutes(-5) && versionDate < date.AddMinutes(5))
            {
                return false;
            }
        }

        // backup must be performed
        return true;
    }

    private void StopScheduleSystem()
    {
        if (schedulerService != null)
        {
            Log.Information("Service for \"Scheduled backups\" is stopped.");
            schedulerService.Stop();
            schedulerService = null;
        }
    }

    private async Task thScheduleSysRunBackup()
    {
        Log.Information("Scheduled backup is planned and will be performed now.");

        // Priorität heruntersetzen
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        var schedulePolicy = scheduleSettingsService.LoadPolicy();
        var lastFullBackup = schedulePolicy.ScheduledFullBackupsEnabled
            ? await queryManager.GetLastFullBackupAsync()
            : null;
        var fullBackupOption = schedulePolicy.ShouldPerformFullBackup(lastFullBackup?.CreationDate, DateTime.Now);

        // Backup durchführen
        try
        {
            if (await jobService.CreateBackupAsync("Automatische Sicherung", "", false, fullBackupOption))
            {
                await RemoveOldBackupsScheduled();
            }
        }
        finally
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
        }
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
