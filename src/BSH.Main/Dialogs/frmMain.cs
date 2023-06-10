﻿// Copyright 2022 Alexander Seeliger
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

using Brightbits.BSH.Engine.Database;
using BSH.Main.Properties;
using Serilog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#if WIN_UWP
using Windows.ApplicationModel;
#endif

namespace Brightbits.BSH.Main;

public partial class frmMain
{
    private static readonly ILogger _logger = Log.ForContext<frmMain>();

    public frmMain()
    {
        InitializeComponent();
    }

    // Öffentliche Variablen
    public enum AvailableTabs
    {
        TabNothing = -1,
        TabConfiguration = 0,
        TabDoConfiguration = 1,
        TabOverview = 2
    }

    // Interne Variablen
    private IMainTabs _iMTCurrentTab;
    private AvailableTabs _aCurrentTab;

    public AvailableTabs CurrentTab
    {
        get
        {
            return _aCurrentTab;
        }

        set
        {
            try
            {
                if (_aCurrentTab == value)
                {
                    return;
                }

                _aCurrentTab = value;

                // Aktives Tab ausblenden
                if (_iMTCurrentTab is object)
                {
                    try
                    {
                        _iMTCurrentTab.CloseTab();
                        _iMTCurrentTab.UserControlInstance.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "An exception during the close of the tab occured.");
                    }
                }

                plMain.Controls.Clear();

                // Neues Tab laden
                switch (_aCurrentTab)
                {
                    case AvailableTabs.TabOverview:

                        _iMTCurrentTab = new ucOverview();
                        _iMTCurrentTab.UserControlInstance.Visible = false;
                        _iMTCurrentTab.Super = this;
                        _iMTCurrentTab.UserControlInstance.Dock = DockStyle.Fill;
                        plMain.Controls.Add((Control)_iMTCurrentTab);

                        // Home-Button anzeigen
                        picHome.Image = Resources.home_icon_48;
                        picHome.Tag = "home";
                        picHome.Cursor = Cursors.Default;
                        break;


                    case AvailableTabs.TabConfiguration:

                        _iMTCurrentTab = new ucConfig();
                        _iMTCurrentTab.UserControlInstance.Visible = false;
                        _iMTCurrentTab.Super = this;
                        _iMTCurrentTab.UserControlInstance.Dock = DockStyle.Fill;
                        plMain.Controls.Add((Control)_iMTCurrentTab);

                        // Zurück-Button anzeigen
                        picHome.Image = Resources.arrow_back_icon_48;
                        picHome.Tag = "config";
                        picHome.Cursor = Cursors.Hand;
                        break;


                    case AvailableTabs.TabDoConfiguration:

                        _iMTCurrentTab = new ucDoConfigure();
                        _iMTCurrentTab.UserControlInstance.Visible = false;
                        _iMTCurrentTab.Super = this;
                        _iMTCurrentTab.UserControlInstance.Dock = DockStyle.Fill;
                        plMain.Controls.Add((Control)_iMTCurrentTab);

                        // Home-Button anzeigen
                        picHome.Image = Resources.home_icon_48;
                        picHome.Tag = "home";
                        picHome.Cursor = Cursors.Default;
                        break;


                    case AvailableTabs.TabNothing:

                        return;
                }

                // Tab anzeigen
                _iMTCurrentTab.OpenTab();
                lblHeadTitle.Text = _iMTCurrentTab.Title;
            }
            catch (Exception ex)
            {
                // ignore error
                _logger.Error(ex, "An exception during the opening of the tab occured.");
            }
        }
    }

    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        Dispose();
    }

    private async void frmMain_Load(object sender, EventArgs e)
    {
        // Tabs laden und entsprechendes Tab öffnen
        switch (StatusController.Current.SystemStatus)
        {
            case SystemStatus.ACTIVATED:
                CurrentTab = AvailableTabs.TabOverview;
                break;

            case SystemStatus.DEACTIVATED:
                CurrentTab = AvailableTabs.TabOverview;
                break;

            case SystemStatus.PAUSED_DUE_TO_BATTERY:
                CurrentTab = AvailableTabs.TabOverview;
                break;

            case SystemStatus.NOT_CONFIGURED:
                CurrentTab = AvailableTabs.TabDoConfiguration;
                break;
        }

        Show();

        // Mit Windows starten
        try
        {
#if WIN_UWP
            AufAktualisierungenPrüfenToolStripMenuItem.Visible = false;
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Visible = false;

            var startupTask = await StartupTask.GetAsync("BackupServiceHomeStartupTask");

            if (startupTask.State == StartupTaskState.Disabled)
            {
                MitWindowsStartenToolStripMenuItem.Checked = false;
            }
            else if (startupTask.State == StartupTaskState.DisabledByUser)
            {
                MitWindowsStartenToolStripMenuItem.Checked = false;
                MitWindowsStartenToolStripMenuItem.Enabled = false;
            }
            else if (startupTask.State == StartupTaskState.Enabled)
            {
                MitWindowsStartenToolStripMenuItem.Checked = true;
            }
#else
            var keyValue = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue("BackupServiceHome3Run");
            if (keyValue is object && !string.IsNullOrEmpty(keyValue.ToString()))
            {
                MitWindowsStartenToolStripMenuItem.Checked = true;
            }

            // Betaversionen herunterladen?
            BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Checked = Settings.Default.DownloadBeta;
            AutomatischNachAktualisierungenSuchenToolStripMenuItem.Checked = Settings.Default.AutoSearchUpdates;
#endif
        }
        catch
        {
            // ignore error
        }
    }

    private void picHome_Click(object sender, EventArgs e)
    {
        if (picHome.Tag.ToString() == "config")
        {
            // Hauptseite anzeigen
            CurrentTab = AvailableTabs.TabOverview;
        }
    }

    private void picHelp_Click(object sender, EventArgs e)
    {
        BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Visible = (ModifierKeys & Keys.Control) == Keys.Control;
        cmsHelp.Show(picHelp, new Point((int)Math.Round(-cmsHelp.Width / 2d + picHelp.Width / 2d), picHelp.Height));
    }

    private async void MitWindowsStartenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
#if WIN_UWP
            var startupTask = await StartupTask.GetAsync("BackupServiceHomeStartupTask");

            if (startupTask.State == StartupTaskState.Disabled)
            {
                var newState = await startupTask.RequestEnableAsync();

                if (newState == StartupTaskState.Enabled)
                {
                    MitWindowsStartenToolStripMenuItem.Checked = true;
                }
            }
            else if (startupTask.State == StartupTaskState.Enabled)
            {
                MessageBox.Show(Resources.DLG_MAIN_MSG_UWP_AUTOSTART_TEXT, Resources.DLG_MAIN_MSG_UWP_AUTOSTART_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
#else
            if (MitWindowsStartenToolStripMenuItem.Checked)
            {
                // Ausschalten
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true).DeleteValue("BackupServiceHome3Run");
                MitWindowsStartenToolStripMenuItem.Checked = false;
            }
            else
            {
                // Aktivieren
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true).SetValue("BackupServiceHome3Run", "\"" + Application.ExecutablePath + "\" -delayedstart");
                MitWindowsStartenToolStripMenuItem.Checked = true;
            }
#endif
        }
        catch
        {
            MessageBox.Show(Resources.DLG_MAIN_MSG_ACCESS_DENIED_TEXT, Resources.DLG_MAIN_MSG_ACCESS_DENIED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }

    private void AufAktualisierungenPrüfenToolStripMenuItem_Click(object sender, EventArgs e)
    {
#if !WIN_UWP
        Program.mainUpdateController.releaseFilter.checkForBeta = Settings.Default.DownloadBeta;
        Program.mainUpdateController.updateInteractive(this);
#endif
    }

    private async void ZurücksetzenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(Resources.DLG_MAIN_MSG_RESET_TEXT, Resources.DLG_MAIN_MSG_RESET_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
            // Gesamte Konfiguration zurücksetzen
            CurrentTab = AvailableTabs.TabNothing;
            BackupLogic.StopSystem();
            DbClientFactory.ClosePool();

            System.IO.File.Delete(BackupLogic.DatabaseFile);

            await BackupLogic.StartupAsync();
            CurrentTab = AvailableTabs.TabDoConfiguration;
        }
    }

    private void ÜberBackupServiceHome3ToolStripMenuItem_Click(object sender, EventArgs e)
    {
        PresentationController.ShowAboutWindow(this);
    }

    private void BackupServiceHome3BeendenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        NotificationController.Current.Shutdown();
        BackupLogic.StopSystem();

        PresentationController.Current.CloseBackupBrowserWindow();

        try
        {
            Application.Exit();
        }
        catch
        {
            Environment.Exit(0);
        }
    }

    private void HilfeUndSupportToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            Process.Start("https://www.brightbits.de/?pk_campaign=software_link&pk_kwd=menu_help&pk_source=bsh-3");
        }
        catch
        {
            // ignore error
        }
    }

    private void BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.Default.DownloadBeta = BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Checked;
        Settings.Default.Save();
    }

    private void GespeichertesKennwortLöschenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Settings.Default.BackupPwd = "";
        Settings.Default.Save();
    }

    private void AutomatischNachAktualisierungenSuchenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        AutomatischNachAktualisierungenSuchenToolStripMenuItem.Checked = !AutomatischNachAktualisierungenSuchenToolStripMenuItem.Checked;
        Settings.Default.AutoSearchUpdates = AutomatischNachAktualisierungenSuchenToolStripMenuItem.Checked;
        Settings.Default.Save();
    }

    private void lblExtras_Click(object sender, EventArgs e)
    {
        BetaversionenÜberAktualiserungenHerunterladenToolStripMenuItem.Visible = (ModifierKeys & Keys.Control) == Keys.Control;
        cmsHelp.Show(picHelp, new Point((int)Math.Round(-cmsHelp.Width / 2d + picHelp.Width / 2d), picHelp.Height));
    }

    private void EreignisprotokollAnzeigenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            var d = DateTime.Now.ToString("yyyyMMdd");
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\Alexosoft\\Backup Service Home 3\\log" + d + ".txt");
        }
        catch
        {
            // ignore error
        }
    }
}