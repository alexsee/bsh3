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
using Brightbits.BSH.Engine.Jobs;
using BSH.Main.Properties;
using Serilog;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    /// <summary>
    /// The NotificationController is responsible for maintaining the global status of all backup
    /// jobs in the application. It maintains the taskbar tray icon and its status.
    /// </summary>
    public class NotificationController : IStatusReport
    {
        private static NotificationController _notificationService;

        private NotifyIcon systemTrayIcon;

        private ContextMenuStrip systemTrayIconContextMenu;

        /// <summary>
        /// Returns the current instance of the NotificationController for the application.
        /// </summary>
        public static NotificationController Current
        {
            get
            {
                if (_notificationService is null)
                {
                    _notificationService = new NotificationController();
                }

                return _notificationService;
            }
        }

        private NotificationController()
        {
            StatusController.Current.AddObserver(this);
        }

        /// <summary>
        /// Initializes the NotificationController and shows the taskbar tray icon with
        /// the current internal status of the application.
        /// </summary>
        public void InitializeSystemTrayIcon()
        {
            // create taskbar tray icon
            systemTrayIcon = new NotifyIcon
            {
                Text = Program.APP_TITLE,
                Icon = global::BSH.Main.Properties.Resources.AppIco,
                Visible = true
            };
            systemTrayIcon.DoubleClick += eventMainContextClick;

            // create context menu
            InitializeSystemTrayIconMenu();
            systemTrayIcon.ContextMenuStrip = systemTrayIconContextMenu;
        }

        /// <summary>
        /// Initializes the context menu of the taskbar tray icon with the current status
        /// of the application.
        /// </summary>
        private void InitializeSystemTrayIconMenu()
        {
            // create context menu strip
            systemTrayIconContextMenu = new ContextMenuStrip
            {
                RenderMode = ToolStripRenderMode.System
            };

            // "Konfigurieren"
            ToolStripItem newEntry;
            newEntry = systemTrayIconContextMenu.Items.Add(Resources.CONTEXT_MENU_STATUS_WINDOW);
            newEntry.Name = "mnuConfigure";
            newEntry.Font = new Font(newEntry.Font, FontStyle.Bold);
            newEntry.Click += eventMainContextConfiguration;

            // "Backupbrowser"
            newEntry = systemTrayIconContextMenu.Items.Add(Resources.CONTEXT_MENU_BACKUP_BROWSER_WINDOW);
            newEntry.Name = "mnuBrowser";
            newEntry.Click += eventMainContextBrowser;

            // Seperator
            systemTrayIconContextMenu.Items.Add("-");

            // "Manuelle Sicherung starten"
            newEntry = systemTrayIconContextMenu.Items.Add(Resources.CONTEXT_MENU_BACKUP_START);
            newEntry.Name = "mnuStartBackup";
            newEntry.Click += eventMainContextStartBackup;

            // "Manuelle Sicherung starten"
            newEntry = systemTrayIconContextMenu.Items.Add(Resources.CONTEXT_MENU_BACKUP_START_OPTIONS);
            newEntry.Name = "mnuStartBackupOptions";
            newEntry.Click += eventMainContextStartBackupOptions;

            // Seperator
            systemTrayIconContextMenu.Items.Add("-");

            // "Beenden"
            newEntry = systemTrayIconContextMenu.Items.Add(Resources.CONTEXT_MENU_CLOSE_APP);
            newEntry.Name = "mnuExit";
            newEntry.Click += eventMainContextExit;
        }

        /// <summary>
        /// Stops the NotificationController and hides the taskbar tray icon.
        /// </summary>
        public void Shutdown()
        {
            systemTrayIcon.Visible = false;
        }

        /// <summary>
        /// Shows a taskbar tray icon balloon with the given title, text, and icon.
        /// </summary>
        /// <param name="timeout">Specifies the timeout for the tray icon balloon in milliseconds.</param>
        /// <param name="title">Specifies the title of the balloon.</param>
        /// <param name="text">Specifies the text of the balloon.</param>
        /// <param name="icon">Specifies the icon of the balloon.</param>
        public void ShowIconBalloon(int timeout, string title, string text, ToolTipIcon icon)
        {
            systemTrayIcon.ShowBalloonTip(timeout, title, text, icon);
        }

        private void eventMainContextConfiguration(object sender, EventArgs e)
        {
            PresentationController.Current.ShowMainWindow();
        }

        private void eventMainContextBrowser(object sender, EventArgs e)
        {
            PresentationController.Current.ShowBackupBrowserWindow();
        }

        private async void eventMainContextStartBackup(object sender, EventArgs e)
        {
            await BackupLogic.BackupController.CreateBackupAsync(Resources.BACKUP_TITLE_MANUAL, "", true);
        }

        private async void eventMainContextStartBackupOptions(object sender, EventArgs e)
        {
            await PresentationController.ShowCreateBackupWindow();
        }

        private void eventMainContextClick(object sender, EventArgs e)
        {
            try
            {
                // is there an active job running or exception being thrown?
                if (StatusController.Current.SystemStatus == SystemStatus.ACTIVATED && StatusController.Current.JobState != JobState.RUNNING && StatusController.Current.JobState != JobState.ERROR)
                {
                    PresentationController.Current.ShowBackupBrowserWindow();
                }
                else
                {
                    PresentationController.Current.ShowMainWindow();
                }
            }
            catch
            {
                PresentationController.Current.ShowBackupBrowserWindow();
            }
        }

        private void eventMainContextExit(object sender, EventArgs e)
        {
            systemTrayIcon.Visible = false;
            BackupLogic.StopSystem();

            PresentationController.Current.CloseMainWindow();
            PresentationController.Current.CloseBackupBrowserWindow();

            Log.Information("Backup Service Home stopped");

            try
            {
                Application.Exit();
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        public void ReportAction(ActionType action, bool silent)
        {
        }

        public void ReportState(JobState jobState)
        {
            try
            {
                systemTrayIconContextMenu.Invoke(new Action(() => ReportState_Safe(jobState)));
            }
            catch (Exception)
            {
                // raise no error
            }
        }

        private void ReportState_Safe(JobState jobState)
        {
            // refresh status
            switch (jobState)
            {
                case JobState.RUNNING:
                    systemTrayIcon.Icon = global::BSH.Main.Properties.Resources.status_refresh;

                    SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], false);
                    SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], true);

                    systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = false;
                    systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = false;

                    break;

                case JobState.ERROR:
                    systemTrayIcon.Icon = global::BSH.Main.Properties.Resources.status_red;

                    SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], false);
                    SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], true);

                    systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = true;
                    systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = true;

                    break;

                case JobState.FINISHED:
                    ReportSystemStatus_Safe(StatusController.Current.SystemStatus);
                    break;
            }
        }

        private void SetDefault(ToolStripItem item, bool value)
        {
            if (value)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
            }
            else
            {
                item.Font = new Font(item.Font, FontStyle.Regular);
            }
        }

        public void ReportStatus(string title, string text)
        {
        }

        public void ReportProgress(int total, int current)
        {
        }

        public void ReportFileProgress(string file)
        {
        }

        public void ReportSystemStatus(SystemStatus systemStatus)
        {
            SynchronizationContext.Current.Send((cancellationToken) => ReportSystemStatus_Safe(systemStatus), systemStatus);
        }

        public void ReportSystemStatus_Safe(SystemStatus systemStatus)
        {
            // refresh status
            switch (systemStatus)
            {
                case SystemStatus.ACTIVATED:
                    // system status ok
                    if (StatusController.Current.SystemStatus == SystemStatus.PAUSED_DUE_TO_BATTERY)
                    {
                        // battery mode, so stop automatic tasks
                        systemTrayIcon.Icon = Resources.status_red;
                        SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], false);
                        SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], true);
                        systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = true;
                        systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = true;
                    }
                    else
                    {
                        systemTrayIcon.Icon = Resources.AppIco;
                        SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], false);
                        SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], true);
                        systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = true;
                        systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = true;
                    }

                    break;

                case SystemStatus.NOT_CONFIGURED:
                    // show tray icon baloon
                    ShowIconBalloon(5000, Resources.INFO_BSH_NOT_CONFIGURED_TITLE, Resources.INFO_BSH_NOT_CONFIGURED_TEXT, ToolTipIcon.Info);

                    systemTrayIcon.Icon = Resources.status_red;

                    SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], false);
                    SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], true);

                    systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = false;
                    systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = false;

                    break;

                case SystemStatus.DEACTIVATED:
                    // system deactivated
                    systemTrayIcon.Icon = Resources.status_red;

                    SetDefault(systemTrayIconContextMenu.Items["mnuBrowser"], false);
                    SetDefault(systemTrayIconContextMenu.Items["mnuConfigure"], true);

                    systemTrayIconContextMenu.Items["mnuStartBackup"].Enabled = true;
                    systemTrayIconContextMenu.Items["mnuStartBackupOptions"].Enabled = true;

                    break;
            }
        }
    }
}