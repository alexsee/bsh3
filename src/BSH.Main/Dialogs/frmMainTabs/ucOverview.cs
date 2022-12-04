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

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using BSH.Main.Properties;
using Humanizer;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucOverview : IMainTabs, IStatusReport
    {
        public ucOverview()
        {
            InitializeComponent();
        }

        #region  Implementation of IMainTabs 

        public void CloseTab()
        {
            StatusController.Current.RemoveObserver(this);
            Visible = false;
        }

        public void OpenTab()
        {
            StatusController.Current.AddObserver(this);
            RefreshInfo();

#if !WIN_UWP
            try
            {
                if (Program.mainUpdateController.currentUpdateResult.UpdatesAvailable)
                {
                    plUpdates.Visible = true;
                }
            }
            catch
            {
                // ignore error
            }
#endif

            Visible = true;
        }

        public UserControl UserControlInstance
        {
            get
            {
                return this;
            }
        }

        public string Title
        {
            get
            {
                return Resources.DLG_UC_OVERVIEW_TITLE;
            }
        }

        private frmMain SuperBase;

        public frmMain Super
        {
            set
            {
                SuperBase = value;
            }
        }

        #endregion

        private void RefreshInfo()
        {
            try
            {
                loadingCircle.InnerCircleRadius = 18;
                loadingCircle.OuterCircleRadius = 19;
                loadingCircle.RotationSpeed = 20;
                loadingCircle.NumberSpoke = 50;
                if (StatusController.Current.SystemStatus == SystemStatus.NOT_CONFIGURED)
                {
                    return;
                }

                // retrieve details from database
                long freeSpace = long.Parse(BackupLogic.GlobalBackup.ConfigurationManager.FreeSpace);
                lblBdSpaceAvailable.Text = (freeSpace == 0L) ? Resources.DLG_UC_OVERVIEW_LBL_FREE_SPACE_NOT_AVAILABLE_TEXT : freeSpace.Bytes().Humanize();

                // compression
                if (BackupLogic.GlobalBackup.ConfigurationManager.Compression == 1)
                {
                    var oldestBackup = BackupLogic.GlobalBackup.QueryManager.GetOldestBackup();

                    lblBdOldestBackup.Text = (oldestBackup != null) ? oldestBackup.CreationDate.Humanize(false) : Resources.DLG_UC_OVERVIEW_LBL_NOT_PERFORMED_TEXT;
                    lblOldBackup.Text = Resources.DLG_UC_OVERVIEW_LBL_OLD_BACKUP_TEXT;
                }
                else
                {
                    // compute device full date
                    lblOldBackup.Text = Resources.DLG_UC_OVERVIEW_LBL_BACKUP_MEDIUM_FULL_TEXT;
                    lblBdOldestBackup.Text = Resources.DLG_UC_OVERVIEW_LBL_BACKUP_FULL_NOT_DETERMINED_TEXT;

                    int countBackup = BackupLogic.GlobalBackup.QueryManager.GetNumberOfVersions();
                    if (countBackup >= 20 && !string.IsNullOrEmpty(BackupLogic.GlobalBackup.ConfigurationManager.BackupSize))
                    {
                        try
                        {
                            var oldestBackup = BackupLogic.GlobalBackup.QueryManager.GetOldestBackup().CreationDate;
                            double tmp;
                            tmp = Convert.ToDouble(BackupLogic.GlobalBackup.ConfigurationManager.FreeSpace) / (Convert.ToDouble(BackupLogic.GlobalBackup.ConfigurationManager.BackupSize) / countBackup);
                            double getDaysToOldestBackup;
                            getDaysToOldestBackup = (DateTime.Now.Subtract(oldestBackup).Days + 1) * tmp;
                            lblBdOldestBackup.Text = DateTime.Now.Add(new TimeSpan((int)Math.Round(getDaysToOldestBackup), 0, 0, 0)).ToLongDateString();
                        }
                        catch
                        {
                            // ignore error
                        }
                    }
                }

                // retrieve newest backup
                var lastBackup = BackupLogic.GlobalBackup.QueryManager.GetLastBackup();
                lblBdNewestBackup.Text = (lastBackup != null) ? lastBackup.CreationDate.Humanize(false) : Resources.DLG_UC_OVERVIEW_LBL_NO_BACKUP_TEXT;

                var nextDate = BackupLogic.GetNextBackupDate();
                lblNextBackup.Text = nextDate.Humanize();

                if (BackupLogic.GlobalBackup.ConfigurationManager.TaskType == TaskType.Auto)
                {
                    lblBackupMode.Text = Resources.DLG_UC_OVERVIEW_LBL_BACKUP_MODE_AUTO_TEXT;
                }
                else if (BackupLogic.GlobalBackup.ConfigurationManager.TaskType == TaskType.Schedule)
                {
                    lblBackupMode.Text = Resources.DLG_UC_OVERVIEW_LBL_BACKUP_MODE_SCHEDULED_TEXT;
                }
                else
                {
                    lblBackupMode.Text = Resources.DLG_UC_OVERVIEW_LBL_BACKUP_MODE_MANUAL_TEXT;
                    lblNextBackup.Text = Resources.DLG_UC_OVERVIEW_LBL_NEXT_BACKUP_NOT_PLANED_TEXT;
                }
            }
            catch
            {
                // ignore error
            }

            // refresh status
            llShowExceptionDialog.Visible = false;
            if (StatusController.Current.SystemStatus != SystemStatus.ACTIVATED)
            {
                if (StatusController.Current.SystemStatus == SystemStatus.PAUSED_DUE_TO_BATTERY)
                {
                    lblBdStatus.Text = Resources.BackupStatus_3;
                }
                else
                {
                    lblBdStatus.Text = Resources.BackupStatus_4;
                }

                picDataType.Image = Resources.status_error;
                loadingCircle.Visible = false;

                BtnChangeState("OFF");
            }
            else
            {
                if (StatusController.Current.JobState == JobState.ERROR)
                {
                    lblBdStatus.Text = Resources.BackupStatus_2;
                }
                else if (StatusController.Current.JobState == JobState.RUNNING)
                {
                    lblBdStatus.Text = Resources.BackupStatus_1;
                }
                else
                {
                    lblBdStatus.Text = Resources.BackupStatus_0;
                }

                if (StatusController.Current.JobState == JobState.ERROR)
                {
                    picDataType.Image = Resources.status_error;
                    loadingCircle.Visible = false;
                    plStatus.Visible = false;
                    btnSettings.Enabled = true;
                    cmdBackupNow.Enabled = true;
                    btnOnOff.Enabled = true;
                    if ((StatusController.Current.LastFilesException?.Count) > 0)
                    {
                        llShowExceptionDialog.Visible = true;
                    }
                }
                else if (StatusController.Current.JobState == JobState.RUNNING)
                {
                    // update status text
                    lblBdStatus.Text = string.IsNullOrEmpty(StatusController.Current.LastFileProgress) ? StatusController.Current.LastStatusText : (System.IO.Path.GetFileName(StatusController.Current.LastFileProgress) + " wird gesichert...");
                    pbStatus.Maximum = StatusController.Current.LastProgressTotal;
                    pbStatus.Value = StatusController.Current.LastProgressCurrent;

                    // show status state
                    picDataType.Image = Resources.status_running;
                    loadingCircle.Visible = true;
                    plStatus.Visible = true;
                    btnSettings.Enabled = false;
                    cmdBackupNow.Enabled = false;
                    btnOnOff.Enabled = false;
                }
                else
                {
                    picDataType.Image = Resources.status_ok;
                    loadingCircle.Visible = false;
                    plStatus.Visible = false;
                    btnSettings.Enabled = true;
                    cmdBackupNow.Enabled = true;
                    btnOnOff.Enabled = true;
                }

                BtnChangeState("ON");
            }

            RefreshInfoText();
        }

        private void BtnChangeState(string str)
        {
            if (str.Equals("OFF"))
            {
                btnOnOff.Tag = "OFF";
                btnOnOff.Image = Resources.toggle_off;
            }
            else
            {
                btnOnOff.Tag = "ON";
                btnOnOff.Image = Resources.toggle_on;
            }
        }

        private void RefreshInfoText()
        {
            try
            {
                var infoText = new StringBuilder();

                if (StatusController.Current.SystemStatus == SystemStatus.ACTIVATED)
                {
                    if (StatusController.Current.JobState == JobState.ERROR)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_STATUS_FAILED);
                        lblInfo.Text = infoText.ToString();
                        return;
                    }
                    else
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_STATUS_OK);
                    }

                    // source folder
                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_SOURCE_FOLDERS_TEXT.FormatWith(BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder.Split('|').Length));

                    // backup type
                    if (BackupLogic.GlobalBackup.ConfigurationManager.TaskType == TaskType.Auto)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_AUTO_BACKUP_TEXT);
                    }
                    else if (BackupLogic.GlobalBackup.ConfigurationManager.TaskType == TaskType.Schedule)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_SCHEDULED_BACKUP_TEXT);
                    }
                    else
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MANUAL_BACKUP_TEXT);
                    }

                    // backup device
                    if (BackupLogic.GlobalBackup.ConfigurationManager.MediumType != 3)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_ON_TEXT);
                        if (BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder.Substring(0, 1) == @"\")
                        {
                            infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MEDIA_NETWORK_BACKUP_TEXT.FormatWith(BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder));
                        }
                        else
                        {
                            var drive = new System.IO.DriveInfo(BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder.Substring(0, 1));
                            switch (drive.DriveType)
                            {
                                case System.IO.DriveType.Fixed:
                                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MEDIA_LOCAL_HDD_BACKUP_TEXT.FormatWith(drive.Name));
                                    break;

                                case System.IO.DriveType.Network:
                                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MEDIA_NETWORK_BACKUP_TEXT.FormatWith(drive.Name));
                                    break;

                                case System.IO.DriveType.Removable:
                                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MEDIA_EXTERNAL_HDD_BACKUP_TEXT.FormatWith(drive.Name));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_MEDIA_FTP_BACKUP_TEXT.FormatWith(BackupLogic.GlobalBackup.ConfigurationManager.FtpHost));
                    }

                    // compressed or encrypted
                    if (BackupLogic.GlobalBackup.ConfigurationManager.Compression == 1)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_COMPRESSED_TEXT);
                    }
                    else if (BackupLogic.GlobalBackup.ConfigurationManager.Encrypt == 1)
                    {
                        infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_ENCRYPTED_TEXT);
                    }
                }
                else if (StatusController.Current.SystemStatus == SystemStatus.PAUSED_DUE_TO_BATTERY)
                {
                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_STATUS_DEACTIVATED_BATTERY_TEXT);
                }
                else
                {
                    infoText.Append(Resources.DLG_UC_OVERVIEW_LBL_STATUS_DEACTIVATED_TEXT);
                }

                lblInfo.Text = infoText.ToString();
            }
            catch
            {
                lblInfo.Text = "";
            }
        }

        private async void cmdBackupNow_MouseClick(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                // show options
                using (var dlgCreateBackup = new frmCreateBackup())
                {
                    if (dlgCreateBackup.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // retrieve sources
                    string sources = string.Join("|", dlgCreateBackup.clstSources.CheckedItems.Cast<string>());
                    if (string.IsNullOrEmpty(sources))
                    {
                        return;
                    }

                    // start backup
                    await BackupLogic.BackupController.CreateBackupAsync(dlgCreateBackup.txtTitle.Text, dlgCreateBackup.txtDescription.Text, true, dlgCreateBackup.cbFullBackup.Checked, dlgCreateBackup.chkShutdownPC.Checked, sourceFolders: sources);
                }
            }
            else
            {
                // start backup
                await BackupLogic.BackupController.CreateBackupAsync(Resources.BACKUP_TITLE_MANUAL, "", true);
            }
        }

        private void cmdBackupCancel_Click(object sender, EventArgs e)
        {
            BackupLogic.BackupController.Cancel();
        }

#if !WIN_UWP
        private void btnUpdates_Click(object sender, EventArgs e)
        {
            Program.mainUpdateController.updateInteractive(ParentForm);
        }
#endif

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SuperBase.CurrentTab = frmMain.AvailableTabs.TabConfiguration;
        }

        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (btnOnOff.Tag.ToString().Equals("OFF"))
            {
                BackupLogic.StartSystem(true);
                btnOnOff.Tag = "ON";
            }
            else
            {
                BackupLogic.StopSystem(true);
                btnOnOff.Tag = "OFF";
            }
        }

        private void btnSettings_MouseLeave(object sender, EventArgs e)
        {
            btnSettings.Image = Resources.settings_3_line;
        }

        private void btnSettings_MouseEnter(object sender, EventArgs e)
        {
            btnSettings.Image = Resources.settings_3_fill;
        }

        private void cmdBackupNow_MouseEnter(object sender, EventArgs e)
        {
            cmdBackupNow.Image = Resources.file_copy_2_fill;
        }

        private void cmdBackupNow_MouseLeave(object sender, EventArgs e)
        {
            cmdBackupNow.Image = Resources.file_copy_2_line;
        }

        private void cmdBackupCancel_MouseEnter(object sender, EventArgs e)
        {
            cmdBackupCancel.Image = Resources.close_circle_fill;
        }

        private void cmdBackupCancel_MouseLeave(object sender, EventArgs e)
        {
            cmdBackupCancel.Image = Resources.close_circle_line;
        }

        private void llOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            btnSettings_Click(e, null);
        }

        private void llBackup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cmdBackupNow_MouseClick(e, null);
        }

        private void btnSettings_EnabledChanged(object sender, EventArgs e)
        {
            llOptions.Enabled = btnSettings.Enabled;
        }

        private void cmdBackupNow_EnabledChanged(object sender, EventArgs e)
        {
            llBackup.Enabled = cmdBackupNow.Enabled;
        }

        public void ReportAction(ActionType action, bool silent)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() => RefreshInfo()));
        }

        public void ReportState(JobState jobState)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() => RefreshInfo()));
        }

        public void ReportStatus(string title, string text)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() => lblBdStatus.Text = text));
        }

        private DateTime lastTimeProgressRefreshed = DateTime.Now;

        public void ReportProgress(int total, int current)
        {
            if (DateTime.Now - lastTimeProgressRefreshed < TimeSpan.FromMilliseconds(100d))
            {
                return;
            }

            if (!IsHandleCreated)
            {
                return;
            }

            lastTimeProgressRefreshed = DateTime.Now;
            Invoke(new Action(() =>
            {
                pbStatus.Maximum = total;
                pbStatus.Value = current;
            }));
        }

        private DateTime lastTimeRefreshed = DateTime.Now;

        public void ReportFileProgress(string file)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            if (DateTime.Now - lastTimeRefreshed < TimeSpan.FromMilliseconds(100d))
            {
                return;
            }

            lastTimeRefreshed = DateTime.Now;
            Invoke(new Action(() => { if (string.IsNullOrEmpty(file)) { lblBdStatus.Text = Resources.DLG_UC_OVERVIEW_STATUS_BACKUP_RUNNING_TEXT; } else { lblBdStatus.Text = Resources.DLG_UC_OVERVIEW_STATUS_BACKUP_RUNNING_FILE_TEXT.FormatWith(System.IO.Path.GetFileName(file)); } }));
        }

        public void ReportSystemStatus(SystemStatus systemStatus)
        {
            if (!IsHandleCreated)
            {
                return;
            }

            Invoke(new Action(() => RefreshInfo()));
        }

        private void llShowExceptionDialog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StatusController.Current.ShowExceptionDialog();
        }
    }
}