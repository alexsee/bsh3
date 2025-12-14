// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using System.Diagnostics;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
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
    private readonly IDbClientFactory dbClientFactory;
    private readonly IStatusService statusService;

    private SchedulerService schedulerService;

    public ScheduledBackupService(IConfigurationManager configurationManager, IJobService jobService, IQueryManager queryManager, IDbClientFactory dbClientFactory, IStatusService statusService)
    {
        this.configurationManager = configurationManager;
        this.jobService = jobService;
        this.queryManager = queryManager;
        this.dbClientFactory = dbClientFactory;
        this.statusService = statusService;
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


    #region  Full Automated System 

    private void StartFullAutomatedSystem()
    {
        Log.Information("Service for \"Full automatic backup\" is started.");

        // start scheduler
        schedulerService = new SchedulerService();
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

        // check if backup is in progress
        if (statusService.IsTaskRunning())
        {
            Log.Warning("Automatic backup cancelled due to other task in progress.");
            return;
        }

        // check if device is ready
        if (!await jobService.CheckMediaAsync(ActionType.Backup, true))
        {
            Log.Warning("Automatic backup cancelled due to not reachable storage device.");
            return;
        }

        // request password
        if (!await jobService.RequestPassword())
        {
            Log.Warning("Automatic backup cancelled due to wrong password.");
            return;
        }

        // lower process priority
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        // run backup
        var task = jobService.CreateBackupAsync("Automatisches Backup", "", false);
        if (task == null)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            return;
        }

        await task.ContinueWith(async (x) =>
        {
            if (!jobService.IsCancellationRequested)
            {
                await RemoveOldBackups();
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith((x) => Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal,
            TaskContinuationOptions.None);
    }

    private List<VersionDetails> GetAutomaticVersionsToDelete()
    {
        var listDelete = new List<VersionDetails>();
        var listVersions = queryManager.GetVersions();

        foreach (var version in listVersions)
        {
            // ignore fixed versions
            if (version.Stable)
            {
                continue;
            }

            // version not older than 24h
            if (DateTime.Now.Subtract(version.CreationDate) <= new TimeSpan(24, 0, 0))
            {
                continue;
            }

            // version older than 1 month
            if (DateTime.Now.Subtract(version.CreationDate) > new TimeSpan(DateTime.DaysInMonth(version.CreationDate.Year, version.CreationDate.Month), 0, 0, 0))
            {
                // keep if last backup of the week
                if (listVersions.Any(x =>
                    version.CreationDate != x.CreationDate &&
                    version.CreationDate.Year == x.CreationDate.Year &&
                    version.CreationDate.Month == x.CreationDate.Month &&
                    version.CreationDate.Day - (int)version.CreationDate.DayOfWeek == x.CreationDate.Day - (int)x.CreationDate.DayOfWeek &&
                    version.CreationDate.DayOfWeek < x.CreationDate.DayOfWeek)
                    )
                {
                    listDelete.Add(version);
                }
            }
            else
            {
                // keep if last backup on that day
                if (listVersions.Any(x =>
                    version.CreationDate != x.CreationDate &&
                    version.CreationDate.Year == x.CreationDate.Year &&
                    version.CreationDate.Month == x.CreationDate.Month &&
                    version.CreationDate.Day == x.CreationDate.Day &&
                    version.CreationDate.TimeOfDay < x.CreationDate.TimeOfDay)
                    )
                {
                    listDelete.Add(version);
                }
            }
        }

        return listDelete;
    }

    private async Task RemoveOldBackups()
    {
        // get versions to clean
        var listDelete = GetAutomaticVersionsToDelete();

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
        schedulerService = new SchedulerService();
        schedulerService.Start();

        // read scheduler entries in database
        using var dbClient = dbClientFactory.CreateDbClient();
        using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM schedule", null);

        while (await reader.ReadAsync())
        {
            var scheduleDate = reader.GetDateTimeParsed("timDate");
            var scheduleType = reader.GetInt32("timType");

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

        await reader.CloseAsync();
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

        // Prüfen, ob was in Arbeit ist
        if (statusService.IsTaskRunning())
        {
            Log.Warning("Scheduled backup cancelled due to other task in progress.");
            return;
        }

        if (!await jobService.CheckMediaAsync(ActionType.Backup, true))
        {
            Log.Warning("Scheduled backup cancelled due to not reachable storage device.");
            return;
        }

        if (!await jobService.RequestPassword())
        {
            Log.Warning("Scheduled backup cancelled due to wrong password.");
            return;
        }

        // Priorität heruntersetzen
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

        // Vollsicherung durchführen?
        var fullBackupOption = false;
        if (!string.IsNullOrEmpty(configurationManager.ScheduleFullBackup))
        {
            // Letzte Vollsicherung ermitteln
            var Item = configurationManager.ScheduleFullBackup.Split('|');
            if (Item[0] == "day")
            {
                var lastFullBackup = await queryManager.GetLastFullBackupAsync();
                var Diff = DateTime.Now.Subtract(lastFullBackup.CreationDate);
                fullBackupOption = Diff.Days >= Convert.ToInt32(Item[1]);
            }
        }

        // Backup durchführen
        var task = jobService.CreateBackupAsync("Automatische Sicherung", "", false, fullBackupOption);
        if (task == null)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
            return;
        }

        await task.ContinueWith(async (x) =>
        {
            if (!jobService.IsCancellationRequested)
            {
                await RemoveOldBackupsScheduled();
            }
        }, TaskContinuationOptions.OnlyOnRanToCompletion)
            .ContinueWith((x) => Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal, TaskContinuationOptions.None);
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
            if (configurationManager.IntervallDelete == "auto")
            {
                // automatic deletion
                listDelete.AddRange(GetAutomaticVersionsToDelete());
            }
            else if (string.IsNullOrEmpty(configurationManager.IntervallDelete))
            {
                // don't delete anything
            }
            else
            {
                // custom intervals
                var intervals = configurationManager.IntervallDelete.Split('|');
                var listVersions = queryManager.GetVersions();

                foreach (var version in listVersions)
                {
                    // ignore fixed versions
                    if (version.Stable)
                    {
                        continue;
                    }

                    // Löschung
                    switch (intervals[0] ?? "")
                    {
                        case "hour":
                            if (DateTime.Now.Subtract(version.CreationDate) > new TimeSpan(Convert.ToInt32(intervals[1]), 0, 0) && !listDelete.Contains(version))
                            {
                                listDelete.Add(version);
                            }

                            break;

                        case "day":
                            if (DateTime.Now.Subtract(version.CreationDate) > new TimeSpan(Convert.ToInt32(intervals[1]), 0, 0, 0) && !listDelete.Contains(version))
                            {
                                listDelete.Add(version);
                            }

                            break;

                        case "week":

                            if (DateTime.Now.Subtract(version.CreationDate) > new TimeSpan((int)Math.Round(Convert.ToDouble(intervals[1]) * 7d), 0, 0, 0) && !listDelete.Contains(version))
                            {
                                listDelete.Add(version);
                            }

                            break;
                    }
                }
            }
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
