// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Utils;
using BSH.Main.Properties;
using Humanizer;
using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    static class BackupLogic
    {
        public static EngineService GlobalBackup { get; set; }

        public static BackupController BackupController { get; set; }

        public static string DatabaseFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Alexosoft\Backup Service Home 3\backupservicehome.bshdb";

        private static UsbWatchService dCWatcher;

        private static SchedulerService schedulerService;

        private static Timer tmrUserReminder;

        private async static Task LoadDatabaseAsync()
        {
            try
            {
                // load database
                GlobalBackup = new EngineService(DatabaseFile);
                await GlobalBackup.InitAsync();

                BackupController = new BackupController(GlobalBackup.BackupService, GlobalBackup.ConfigurationManager);
            }
            catch (Exception ex)
            {
                // global error
                ExceptionController.HandleGlobalException(null, new System.Threading.ThreadExceptionEventArgs(ex));
            }
        }

        /// <summary>
        /// Loads the database and starts the backup engine if BSH is configured.
        /// </summary>
        public async static Task StartupAsync()
        {
            // first time start?
            if (!System.IO.File.Exists(DatabaseFile))
            {
                // Status: unconfigured
                StatusController.Current.SetSystemStatus(SystemStatus.NOT_CONFIGURED);

                // create database
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DatabaseFile));
                await LoadDatabaseAsync();
            }
            else
            {
                // load existing database
                await LoadDatabaseAsync();
                if (GlobalBackup.ConfigurationManager.IsConfigured == "0")
                {
                    // Status: unconfigured
                    StatusController.Current.SetSystemStatus(SystemStatus.NOT_CONFIGURED);
                }
                else
                {
                    // Status: OK
                    await StartSystemAsync();
                }
            }
        }

        public async static Task StartSystemAsync(bool doOn = false)
        {
            // check system status
            if (doOn || GlobalBackup.ConfigurationManager.DbStatus == "0")
            {
                // check battery status
                if (GlobalBackup.ConfigurationManager.DeativateAutoBackupsWhenAkku == "1")
                {
                    StartBatteryCheck();
                }

                // is system activated?
                if (doOn)
                {
                    GlobalBackup.ConfigurationManager.DbStatus = "0";
                }

                StatusController.Current.SetSystemStatus(SystemStatus.ACTIVATED);

                // are we running on battery mode?
                if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery || SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online || GlobalBackup.ConfigurationManager.DeativateAutoBackupsWhenAkku == "0")
                {
                    // automatic or scheduled backups
                    if (GlobalBackup.ConfigurationManager.TaskType == TaskType.Auto)
                    {
                        StartFullAutomatedSystem();
                    }
                    else if (GlobalBackup.ConfigurationManager.TaskType == TaskType.Schedule)
                    {
                        await StartScheduleSystem();
                    }
                }
                else
                {
                    StatusController.Current.SetSystemStatus(SystemStatus.PAUSED_DUE_TO_BATTERY);
                }

                // check free space
                if (GlobalBackup.ConfigurationManager.RemindSpace != "-1" && GlobalBackup.ConfigurationManager.FreeSpace != "0" && Convert.ToDouble(GlobalBackup.ConfigurationManager.FreeSpace) < Convert.ToDouble(GlobalBackup.ConfigurationManager.RemindSpace) * 1024L * 1024L)
                {
                    NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_NO_DISKSPACE_LEFT_TITLE, Resources.INFO_NO_DISKSPACE_LEFT_TEXT, ToolTipIcon.Warning);
                }

                // check if backup is running
                if (StatusController.Current.IsTaskRunning())
                {
                    return;
                }

                // check unexpected end of last backup
                if (!string.IsNullOrEmpty(GlobalBackup.ConfigurationManager.LastVersionDate))
                {
                    GlobalBackup.ConfigurationManager.LastVersionDate = "";
                }

                // remind user about last backup
                if (!string.IsNullOrEmpty(GlobalBackup.ConfigurationManager.RemindAfterDays))
                {
                    var lastBackup = await GlobalBackup.QueryManager.GetLastBackupAsync();
                    if (lastBackup != null)
                    {
                        try
                        {
                            // check if backup is older than x-days
                            if (DateTime.Now.Subtract(lastBackup.CreationDate).Days > Convert.ToInt32(GlobalBackup.ConfigurationManager.RemindAfterDays) &&
                                GlobalBackup.QueryManager.GetVersions().Count > 0 &&
                                tmrUserReminder == null)
                            {
                                tmrUserReminder = new Timer
                                {
                                    Interval = 60000
                                };
                                tmrUserReminder.Tick += RemindUserOldBackup;
                                tmrUserReminder.Start();
                            }
                        }
                        catch
                        {
                            GlobalBackup.ConfigurationManager.LastBackupDone = "";
                        }
                    }
                }
            }
            else
            {
                StatusController.Current.SetSystemStatus(SystemStatus.DEACTIVATED);
            }
        }

        private async static void RemindUserOldBackup(object sender, EventArgs e)
        {
            Timer tmr = (Timer)sender;
            tmr.Stop();
            tmr.Dispose();

            var lastBackup = await GlobalBackup.QueryManager.GetLastBackupAsync();
            if (lastBackup == null)
            {
                return;
            }

            if (DateTime.Now.Subtract(lastBackup.CreationDate).Days > 0)
            {
                NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_OLD_TITLE, Resources.INFO_BACKUP_OLD_TEXT.FormatWith(DateTime.Now.Subtract(lastBackup.CreationDate).Days), ToolTipIcon.Info);
            }
        }

        public static void StopSystem(bool doOff = false)
        {
            if (doOff)
            {
                GlobalBackup.ConfigurationManager.DbStatus = "1";
            }

            // stop all systems
            StopFullAutomatedSystem();
            StopScheduleSystem();
            StopDoBackupWhenDriveIsAvailable();

            // set status to deactivated
            StatusController.Current.SetSystemStatus(SystemStatus.DEACTIVATED);
        }

        private static PowerLineStatus LastBatteryStatus;
        private static bool BatteryStatusUnderCheck = false;

        public static void StartBatteryCheck()
        {
            if (BatteryStatusUnderCheck)
            {
                return;
            }

            // observe battery status
            if (SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery)
            {
                LastBatteryStatus = SystemInformation.PowerStatus.PowerLineStatus;
                SystemEvents.PowerModeChanged += PowerChanged;
                BatteryStatusUnderCheck = true;
            }
        }

        public static void StopBatteryCheck()
        {
            if (!BatteryStatusUnderCheck)
            {
                return;
            }

            // stop battery status observation
            SystemEvents.PowerModeChanged -= PowerChanged;
            BatteryStatusUnderCheck = false;
        }

        private async static void PowerChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (GlobalBackup.ConfigurationManager.DeativateAutoBackupsWhenAkku == "0")
            {
                StopBatteryCheck();
                return;
            }

            if (LastBatteryStatus != SystemInformation.PowerStatus.PowerLineStatus)
            {
                LastBatteryStatus = SystemInformation.PowerStatus.PowerLineStatus;

                // check status
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                {
                    await StartSystemAsync();
                }
                else
                {
                    StopSystem();
                }
            }
        }

        #region  Full Automated System 

        public static void StartFullAutomatedSystem()
        {
            Log.Information("Service for \"Full automatic backup\" is started.");

            // start scheduler
            schedulerService = new SchedulerService();
            schedulerService.Start();
            schedulerService.ScheduleAutoBackup(() => RunAutoBackup());

            // start backup when device is connected
            DoBackupWhenDriveIsAvailable(RunBackupMethod.Auto);
        }

        public static void StopFullAutomatedSystem()
        {
            // stop scheduler
            if (schedulerService == null)
            {
                return;
            }

            schedulerService.Stop();
            Log.Information("Service for \"Full automatic backup\" is stopped.");
        }

        private static void RunAutoBackup()
        {
            Log.Information("Automatic backup is scheduled and will be performed now.");

            // check if backup is in progress
            if (StatusController.Current.IsTaskRunning())
            {
                Log.Warning("Automatic backup cancelled due to other task in progress.");
                return;
            }

            // check if device is ready
            if (!GlobalBackup.BackupService.CheckMedia())
            {
                Log.Warning("Automatic backup cancelled due to not reachable storage device.");
                return;
            }

            // request password
            if (!BackupController.RequestPassword())
            {
                Log.Warning("Automatic backup cancelled due to wrong password.");
                return;
            }

            // stop automatic backup when device ready
            StopDoBackupWhenDriveIsAvailable();

            // lower process priority
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            // run backup
            var cancellationToken = BackupController.GetNewCancellationToken();
            Engine.Jobs.IJobReport argjobReport = StatusController.Current;

            var task = GlobalBackup.BackupService.StartBackup("Automatisches Backup", "", ref argjobReport, cancellationToken, false, "", true);
            if (task == null)
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                return;
            }

            task.ContinueWith((x) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    RemoveOldBackups();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith((x) => Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal,
                TaskContinuationOptions.None);
        }

        private static List<VersionDetails> GetAutomaticVersionsToDelete()
        {
            var listDelete = new List<VersionDetails>();
            var listVersions = GlobalBackup.QueryManager.GetVersions();

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

        private static void RemoveOldBackups()
        {
            // get versions to clean
            var listDelete = GetAutomaticVersionsToDelete();

            // delete old versions
            foreach (var version in listDelete)
            {
                var cancellationToken = BackupController.GetNewCancellationToken();
                Engine.Jobs.IJobReport argjobReport = StatusController.Current;

                var task = GlobalBackup.BackupService.StartDelete(version.Id, ref argjobReport, cancellationToken, true);
                task.Wait();
            }
        }

        #endregion

        #region  Schedule System 

        private async static void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // restart system after standby resume
                StopScheduleSystem();
                await StartScheduleSystem();
            }
        }

        public async static Task StartScheduleSystem()
        {
            Log.Information("Service for \"Scheduled backups\" is started.");

            // start scheduler
            schedulerService = new SchedulerService();
            schedulerService.Start();

            // observe power mode
            SystemEvents.PowerModeChanged += PowerModeChanged;

            // read scheduler entries in database
            using var dbClient = GlobalBackup.DbClientFactory.CreateDbClient();
            using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM schedule", null);
            
            while (reader.Read())
            {
                var scheduleDate = reader.GetDateTimeParsed("timDate");
                int scheduleType = reader.GetInt32("timType");

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
                        var versions = GlobalBackup.QueryManager.GetVersions(true);
                        var checkDate = DateUtils.GetDateToWeekDay(scheduleDate.DayOfWeek, DateTime.Now);

                        foreach (var version in versions)
                        {
                            if (version.CreationDate.Subtract(checkDate).Days < 0)
                            {
                                // backup was performed already earlier
                                if (DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                                {
                                    schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                                }

                                break;
                            }

                            if (version.CreationDate.DayOfWeek == scheduleDate.DayOfWeek)
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
                        var versions = GlobalBackup.QueryManager.GetVersions(true);
                        var checkDate = DateUtils.GetDateToMonth(scheduleDate.Day, DateTime.Now);

                        foreach (var version in versions)
                        {
                            // backup was performed already earlier
                            if (version.CreationDate.Subtract(checkDate).Days < 0)
                            {
                                if (DoPastBackup(DateTime.Now.Date.Add(scheduleDate.TimeOfDay), true))
                                {
                                    schedulerService.ScheduleOnce(async () => await thScheduleSysRunBackup(), DateTime.Now.AddMinutes(1));
                                }

                                break;
                            }

                            if (version.CreationDate.Day == scheduleDate.Day)
                            {
                                break;
                            }
                        }
                    }

                    schedulerService.ScheduleMonthly(async () => await thScheduleSysRunBackup(), scheduleDate);
                }
            }

            reader.Close();
        }

        public static bool DoPastBackup(DateTime date, bool orOlder = false)
        {
            if (GlobalBackup.ConfigurationManager.DoPastBackups != "1")
            {
                return false;
            }

            // backup is after the last backup
            if (!string.IsNullOrEmpty(GlobalBackup.ConfigurationManager.LastBackupDone) && DateUtils.ReformatVersionDate(GlobalBackup.ConfigurationManager.LastBackupDone) >= date)
            {
                return false;
            }

            // check if this backup was performed
            foreach (var version in GlobalBackup.QueryManager.GetVersions())
            {
                if (orOlder)
                {
                    if (version.CreationDate >= date)
                    {
                        return false;
                    }
                }
                else if (version.CreationDate > date.AddMinutes(-5) && version.CreationDate < date.AddMinutes(5))
                {
                    return false;
                }
            }

            // backup must be performed
            return true;
        }

        public static void StopScheduleSystem()
        {
            if (schedulerService != null)
            {
                Log.Information("Service for \"Scheduled backups\" is stopped.");
                schedulerService.Stop();
                schedulerService = null;
            }

            StopDoBackupWhenDriveIsAvailable();
            SystemEvents.PowerModeChanged -= PowerModeChanged;
        }

        public async static Task thScheduleSysRunBackup()
        {
            Log.Information("Scheduled backup is planned and will be performed now.");

            // Prüfen, ob was in Arbeit ist
            if (StatusController.Current.IsTaskRunning())
            {
                Log.Warning("Scheduled backup cancelled due to other task in progress.");
                return;
            }

            if (!GlobalBackup.BackupService.CheckMedia())
            {
                Log.Warning("Scheduled backup cancelled due to not reachable storage device.");
                return;
            }

            if (!BackupController.RequestPassword())
            {
                Log.Warning("Scheduled backup cancelled due to wrong password.");
                return;
            }

            // Automatisches nachholen abbrechen
            StopDoBackupWhenDriveIsAvailable();

            // Priorität heruntersetzen
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            // Vollsicherung durchführen?
            bool FullBackup = false;
            if (!string.IsNullOrEmpty(GlobalBackup.ConfigurationManager.ScheduleFullBackup))
            {
                // Letzte Vollsicherung ermitteln
                var Item = GlobalBackup.ConfigurationManager.ScheduleFullBackup.Split('|');
                if (Item[0] == "day")
                {
                    var lastFullBackup = await GlobalBackup.QueryManager.GetLastFullBackupAsync();
                    var Diff = DateTime.Now.Subtract(lastFullBackup.CreationDate);
                    FullBackup = Diff.Days >= Convert.ToInt32(Item[1]);
                }
            }

            // Backup durchführen
            var cancellationToken = BackupController.GetNewCancellationToken();
            Engine.Jobs.IJobReport argjobReport = StatusController.Current;

            var task = GlobalBackup.BackupService.StartBackup(Resources.BACKUP_TITLE_AUTOMATIC, "", ref argjobReport, cancellationToken, FullBackup, "", true);
            if (task == null)
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                return;
            }

            await task.ContinueWith((x) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    RemoveOldBackupsScheduled();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith((x) => Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal, TaskContinuationOptions.None);
        }

        private static void RemoveOldBackupsScheduled()
        {
            // lower process priority
            var proc = Process.GetCurrentProcess();
            proc.PriorityClass = ProcessPriorityClass.BelowNormal;

            // obtain versions for deletion
            var listDelete = new List<VersionDetails>();

            try
            {
                if (GlobalBackup.ConfigurationManager.IntervallDelete == "auto")
                {
                    // automatic deletion
                    listDelete.AddRange(GetAutomaticVersionsToDelete());
                }
                else if (string.IsNullOrEmpty(GlobalBackup.ConfigurationManager.IntervallDelete))
                {
                    // don't delete anything
                }
                else
                {
                    // custom intervals
                    var intervals = GlobalBackup.ConfigurationManager.IntervallDelete.Split('|');
                    var listVersions = GlobalBackup.QueryManager.GetVersions();

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
                var cancellationToken = BackupController.GetNewCancellationToken();
                Engine.Jobs.IJobReport argjobReport = StatusController.Current;

                var task = GlobalBackup.BackupService.StartDelete(version.Id, ref argjobReport, cancellationToken, true);
                task.Wait();
            }
        }

        #endregion

        private static RunBackupMethod _RunBackupDelegate;

        public enum RunBackupMethod
        {
            Schedule,
            Auto,
            Manuell
        }

        public static void DoBackupWhenDriveIsAvailable(RunBackupMethod RunBackupDelegate)
        {
            if (dCWatcher != null)
            {
                return;
            }

            // only start, if local device
            if (GlobalBackup.ConfigurationManager.MediumType != 3)
            {
                dCWatcher = new UsbWatchService();
                dCWatcher.StartWatching();

                // observe devices
                dCWatcher.DeviceAdded += DriveArrived;
                _RunBackupDelegate = RunBackupDelegate;
            }
        }

        public static void StopDoBackupWhenDriveIsAvailable()
        {
            try
            {
                if (dCWatcher != null)
                {
                    dCWatcher.DeviceAdded -= DriveArrived;
                    dCWatcher.StopWatching();
                    dCWatcher = null;
                }
            }
            catch
            {
                // ignore error
            }
        }

        private async static void DriveArrived(object sender, string driveLetter)
        {
            if (!await BackupController.CheckMediaAsync(ActionType.Backup, true))
            {
                return;
            }

            // check is backup device is connected
            if (GlobalBackup.ConfigurationManager.BackupFolder.ToLower().StartsWith(driveLetter.ToLower()))
            {
                try
                {
                    dCWatcher.DeviceAdded -= DriveArrived;
                    dCWatcher.StopWatching();
                    dCWatcher = null;
                }
                catch
                {
                    // ignore error
                }

                // start backup
                if (_RunBackupDelegate == RunBackupMethod.Auto)
                {
                    RunAutoBackup();
                }
                else if (_RunBackupDelegate == RunBackupMethod.Schedule)
                {
                    await thScheduleSysRunBackup();
                }
            }
        }

        public static void CommandAutoDelete()
        {
            // check device
            if (!GlobalBackup.BackupService.CheckMedia())
            {
                return;
            }

            // delete old versions
            RemoveOldBackups();
        }

        public static DateTime GetNextBackupDate()
        {
            if (schedulerService == null)
            {
                return DateTime.MaxValue;
            }

            return schedulerService.GetNextRun();
        }
    }
}