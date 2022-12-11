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
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Controls.UI;
using BSH.Main.Properties;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucConfig : IMainTabs
    {
        private bool save = true;

        private string selectedFolderCache = "";

        public ucConfig()
        {
            InitializeComponent();
        }

        #region  Implementation of IMainTabs 

        public void CloseTab()
        {
            // hide tabs
            Visible = false;
            if (!save)
            {
                return;
            }

            StoreSettings();
        }

        public void OpenTab()
        {
            // Tab anzeigen
            Visible = true;

            LoadSettings();
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
                return Resources.DLG_UC_CONFIG_TITLE;
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

        private void StoreSettings()
        {
            // wait for any other tasks finished
            if (StatusController.Current.IsTaskRunning())
            {
                using (var dlgShortStatus = new frmShortStatus())
                {
                    Enabled = false;
                    dlgShortStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_WAIT_FOR_FINISH_TEXT;
                    dlgShortStatus.Show(SuperBase);

                    while (StatusController.Current.IsTaskRunning())
                    {
                        Application.DoEvents();
                    }

                    dlgShortStatus.Hide();
                    Enabled = true;
                }
            }

            // reset backup path
            if (cmdChange.Tag?.ToString() == "save")
            {
                // user does not store the settings
                txtBackupPath.Text = txtBackupPath.Tag.ToString();
            }

            // update backup storage
            if (cboMedia.SelectedIndex == 1)
            {
                // FTP
                BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 3;
            }
            else
            {
                // directory
                BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 1;

                // UNC authentication
                if (txtBackupPath.Text.StartsWith(@"\\"))
                {
                    BackupLogic.GlobalBackup.ConfigurationManager.UNCUsername = txtUNCUsername.Text;
                    BackupLogic.GlobalBackup.ConfigurationManager.UNCPassword = Crypto.EncryptString(Crypto.ToSecureString(txtUNCPassword.Text), System.Security.Cryptography.DataProtectionScope.LocalMachine);
                    BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial = "";
                }
                else
                {
                    BackupLogic.GlobalBackup.ConfigurationManager.UNCUsername = "";
                    BackupLogic.GlobalBackup.ConfigurationManager.UNCPassword = "";

                    // MedienID speichern
                    BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(txtBackupPath.Text.Substring(0, 1) + @":\");
                    if (BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial == null ||
                        BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial == "0")
                    {
                        BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial = "";
                    }
                }
            }

            var configurationManager = BackupLogic.GlobalBackup.ConfigurationManager;
            configurationManager.FtpHost = txtFTPServer.Text;
            configurationManager.FtpPort = txtFTPPort.Text;
            configurationManager.FtpUser = txtFTPUsername.Text;
            configurationManager.FtpPass = txtFTPPassword.Text;
            configurationManager.FtpFolder = FTPStorage.GetFtpPath(txtFTPPath.Text);
            configurationManager.FtpCoding = cboFtpEncoding.SelectedItem?.ToString();

            if (chkFtpEncryption.Checked)
            {
                configurationManager.FtpEncryptionMode = "0";
                configurationManager.FtpSslProtocols = "0";
            }
            else
            {
                configurationManager.FtpEncryptionMode = "3";
                configurationManager.FtpSslProtocols = "0";
            }

            configurationManager.BackupFolder = txtBackupPath.Text;

            // backup mode
            if (rbFAB.Checked)
            {
                configurationManager.TaskType = TaskType.Auto;
            }
            else if (rbTSB.Checked)
            {
                configurationManager.TaskType = TaskType.Schedule;
            }
            else
            {
                configurationManager.TaskType = TaskType.Manual;
            }

            // compression / encryption
            configurationManager.Compression = rdCompress.Checked ? 1 : 0;

            // free space
            configurationManager.RemindSpace = chkRemindSpace.Checked ? ((int)txtRemindSpace.Value).ToString() : "-1";

            // shutdown, if battery powered
            configurationManager.DeativateAutoBackupsWhenAkku = chkDeactivateAutoBackupsWhenAkku.Checked ? "1" : "0";

            // cancel, if media is available
            configurationManager.Medium = chkAbortWhenNotAvailable.Checked ? "1" : "0";

            configurationManager.ShowLocalizedPath = chkShowLocalized.Checked ? "1" : "0";

            configurationManager.DoPastBackups = chkDoPastBackups.Checked ? "1" : "0";

            configurationManager.InfoBackupDone = chkInfoBackupDone.Checked ? "1" : "0";

            configurationManager.ShowWaitOnMediaAutoBackups = chkWaitOnMediaInteractive.Checked ? "1" : "0";

            // compression exclusion
            if (lstExcludeCompress.Items.Count == 0)
            {
                configurationManager.ExcludeCompression = "";
            }
            else
            {
                string excludeCompression = string.Join("|", lstExcludeCompress.Items.Cast<object>().Select(x => lstExcludeCompress.GetItemText(x)));
                configurationManager.ExcludeCompression = excludeCompression;
            }

            // compression factor
            configurationManager.CompressionLevel = tbCompressionLevel.Value.ToString();

            configurationManager.RemindAfterDays = chkRemind.Checked ? ((int)nudRemind.Value).ToString() : "";

            // stop system
            BackupLogic.StopSystem();
            BackupLogic.StartSystem();
        }

        private void LoadSettings()
        {
            lvSource.Items.Clear();

            // load sources
            if (!string.IsNullOrEmpty(BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder))
            {
                var sources = BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder.Split('|');
                foreach (string entry in sources)
                {
                    lvSource.Items.Add(entry, 0);
                }
            }

            // block media selection
            cboMedia.Tag = "";

            // load backup storage
            if (BackupLogic.GlobalBackup.ConfigurationManager.MediumType == 3)
            {
                // FTP
                cboMedia.SelectedIndex = 1;
                plDevice.Visible = false;
                plFTP.Visible = true;
            }
            else
            {
                // directory
                cboMedia.SelectedIndex = 0;
                plDevice.Visible = true;
                plFTP.Visible = false;
            }

            // release media selection
            cboMedia.Tag = "1";

            var configurationManager = BackupLogic.GlobalBackup.ConfigurationManager;
            txtFTPServer.Text = configurationManager.FtpHost;
            txtFTPPort.Text = string.IsNullOrEmpty(configurationManager.FtpPort) ? "21" : configurationManager.FtpPort;
            txtFTPUsername.Text = configurationManager.FtpUser;
            txtFTPPassword.Text = configurationManager.FtpPass;
            txtFTPPath.Text = configurationManager.FtpFolder;
            cboFtpEncoding.SelectedItem = configurationManager.FtpCoding;
            txtBackupPath.Text = configurationManager.BackupFolder;

            if (configurationManager.FtpEncryptionMode == "3")
            {
                chkFtpEncryption.Checked = false;
            }
            else
            {
                chkFtpEncryption.Checked = true;
            }

            // UNC authentication
            if (txtBackupPath.Text.StartsWith(@"\\"))
            {
                txtUNCUsername.Text = configurationManager.UNCUsername;
                txtUNCPassword.Text = Crypto.ToInsecureString(Crypto.DecryptString(configurationManager.UNCPassword, System.Security.Cryptography.DataProtectionScope.LocalMachine));
            }

            // backup mode
            if (configurationManager.TaskType == TaskType.Auto)
            {
                rbFAB.Checked = true;
            }
            else if (configurationManager.TaskType == TaskType.Schedule)
            {
                rbTSB.Checked = true;
            }
            else
            {
                rbMB.Checked = true;
            }

            // encryption
            if (configurationManager.Encrypt == 1)
            {
                plCompressEncrypt.Enabled = false;
                rdEncrypt.Checked = true;
                cmdDeactivateEncrypt.Enabled = true;
                cmdEncrypt.Enabled = false;
            }
            else
            {
                cmdDeactivateEncrypt.Enabled = false;
                plCompressEncrypt.Enabled = true;
                if (configurationManager.Compression == 1)
                {
                    rdCompress.Checked = true;
                }
                else
                {
                    rdNoCompress.Checked = true;
                }
            }

            if (configurationManager.RemindSpace != "-1")
            {
                chkRemindSpace.Checked = true;
                txtRemindSpace.Value = decimal.Parse(configurationManager.RemindSpace);
            }

            chkDeactivateAutoBackupsWhenAkku.Checked = configurationManager.DeativateAutoBackupsWhenAkku == "1";

            tbCompressionLevel.Value = int.Parse(configurationManager.CompressionLevel);
            chkAbortWhenNotAvailable.Checked = configurationManager.Medium == "1";

            chkShowLocalized.Checked = configurationManager.ShowLocalizedPath == "1";

            chkDoPastBackups.Checked = configurationManager.DoPastBackups == "1";

            chkInfoBackupDone.Checked = configurationManager.InfoBackupDone == "1";

            chkWaitOnMediaInteractive.Checked = configurationManager.ShowWaitOnMediaAutoBackups == "1";

            // compression exclusion
            if (!string.IsNullOrEmpty(configurationManager.ExcludeCompression))
            {
                var excludeCompression = configurationManager.ExcludeCompression.Split('|');
                lstExcludeCompress.Items.Clear();
                foreach (string entry in excludeCompression)
                {
                    lstExcludeCompress.Items.Add(entry);
                }
            }

            if (!string.IsNullOrEmpty(configurationManager.RemindAfterDays))
            {
                chkRemind.Checked = true;
                nudRemind.Value = decimal.Parse(configurationManager.RemindAfterDays);
            }
            else
            {
                chkRemind.Checked = false;
            }
        }

        private void cmdChange_Click(object sender, EventArgs e)
        {
            // select folder from computer
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                dlgFolderBrowser.ShowNewFolderButton = true;

                if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // show change path window
                var formChangePath = new frmChangePath();
                var result = formChangePath.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // move
                    try
                    {
                        // we need to copy or move the file, this is why the "old" VB.NET method is used
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(txtBackupPath.Text, dlgFolderBrowser.SelectedPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_ERROR_MOVE_TEXT + ex.Message, Resources.DLG_UC_CONFIG_MSG_ERROR_MOVE_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    txtBackupPath.Text = dlgFolderBrowser.SelectedPath;
                }
                else if (result == DialogResult.Ignore)
                {
                    // use
                    txtBackupPath.Text = dlgFolderBrowser.SelectedPath;
                }
            }
        }

        private void cmdAddSource_Click(object sender, EventArgs e)
        {
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                dlgFolderBrowser.SelectedPath = selectedFolderCache;

                if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
                {
                    selectedFolderCache = dlgFolderBrowser.SelectedPath;
                    AddSourceFolder(selectedFolderCache);
                }
            }
        }

        private void AddSourceFolder(string folderPath)
        {
            // check same folder name
            foreach (ListViewItem entry in lvSource.Items)
            {
                if (string.Compare(new DirectoryInfo(entry.Text).Name, new DirectoryInfo(folderPath).Name, true) == 0)
                {
                    MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_ERROR_SAME_DIRECTORY_NAME_TEXT, Resources.DLG_UC_CONFIG_MSG_ERROR_SAME_DIRECTORY_NAME_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            if (folderPath.Length <= 3)
            {
                MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_INFO_DRIVE_SELECTED_TEXT, Resources.DLG_UC_CONFIG_MSG_INFO_DRIVE_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            lvSource.Items.Add(folderPath, 0);

            // wait for task if running
            if (StatusController.Current.IsTaskRunning())
            {
                using (var dlgShortStatus = new frmShortStatus())
                {
                    Enabled = false;
                    dlgShortStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_WAIT_FOR_FINISH_TEXT;
                    dlgShortStatus.Show();

                    while (StatusController.Current.IsTaskRunning())
                    {
                        Application.DoEvents();
                    }

                    dlgShortStatus.Hide();
                    Enabled = true;
                }
            }

            // store source folder
            if (lvSource.Items.Count == 0)
            {
                BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder = "";
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder = string.Join("|", lvSource.Items.Cast<ListViewItem>().Select(x => x.Text));
            }
        }

        private void cmdDeleteSource_Click(object sender, EventArgs e)
        {
            // delete entry
            if (lvSource.SelectedItems.Count <= 0)
            {
                return;
            }

            lvSource.SelectedItems[0].Remove();

            // store source folders
            if (lvSource.Items.Count == 0)
            {
                BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder = "";
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder = string.Join("|", lvSource.Items.Cast<ListViewItem>().Select(x => x.Text));
            }

            cmdDeleteSource_Click(sender, e);
        }

        private void cmdFTPCheck_Click(object sender, EventArgs e)
        {
            try
            {
                // check FTP
                var profile = FTPStorage.CheckConnection(txtFTPServer.Text, Convert.ToInt32(txtFTPPort.Text), txtFTPUsername.Text, txtFTPPassword.Text, txtFTPPath.Text, Convert.ToString(cboFtpEncoding.SelectedItem));

                if (profile != null)
                {
                    MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_INFO_FTP_SUCCESSFUL_TEXT, Resources.DLG_UC_CONFIG_MSG_INFO_FTP_SUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT, Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + "\r\n\r\n" + ex.Message.ToString(), Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmdEncrypt_Click(object sender, EventArgs e)
        {
            // setup encryption
            var (password, _) = PresentationController.Current.RequestPassword();
            if (password == null)
            {
                return;
            }

            BackupLogic.GlobalBackup.ConfigurationManager.EncryptPassMD5 = Hash.GetMD5Hash(password);
            BackupLogic.GlobalBackup.ConfigurationManager.Encrypt = 1;
            cmdEncrypt.Enabled = false;

            OpenTab();
        }

        private void cmdFilter_Click(object sender, EventArgs e)
        {
            using (var dlgFilter = new frmFilter())
            {
                dlgFilter.ShowDialog(this);
            }
        }

        private void rdEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            cmdEncrypt.Enabled = rdEncrypt.Checked;
        }

        private async void cmdDeactivateEncrypt_Click(object sender, EventArgs e)
        {
            if (!await BackupLogic.BackupController.CheckMediaAsync(ActionType.Restore))
            {
                return;
            }

            if (!BackupLogic.BackupController.RequestPassword())
            {
                return;
            }

            // disable encryption (need to decrypt everything)
            using (var statusWindow = new frmShortStatus())
            {
                IJobReport statusWindowReport = statusWindow;
                statusWindow.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_DECRYPT_BACKUP_TEXT;
                statusWindow.Show(this);

                Enabled = false;

                var task = BackupLogic.GlobalBackup.BackupService.StartEdit(ref statusWindowReport, new CancellationTokenSource().Token, true);
                await task.ConfigureAwait(true);
                statusWindow.Close();

                Enabled = true;
            }

            OpenTab();
        }

        private void rbTSB_CheckedChanged(object sender, EventArgs e)
        {
            cmdEditSchedule.Enabled = rbTSB.Checked;
            chkDoPastBackups.Enabled = rbTSB.Checked;
        }

        private void cmdEditSchedule_Click(object sender, EventArgs e)
        {
            using (var dlgEditScheduler = new frmEditScheduler())
            {
                dlgEditScheduler.ShowDialog(this);
            }
        }

        private void chkRemindSpace_CheckedChanged(object sender, EventArgs e)
        {
            txtRemindSpace.Enabled = chkRemindSpace.Checked;
        }

        private void tbCompressionLevel_ValueChanged(object sender, EventArgs e)
        {
            if (tbCompressionLevel.Value == 0)
            {
                lblCompressionLevel.Text = Resources.DLG_UC_CONFIG_LBL_NO_COMPRESSION;
            }
            else if (tbCompressionLevel.Value < 5)
            {
                lblCompressionLevel.Text = Resources.DLG_UC_CONFIG_LBL_LOW_COMPRESSION;
            }
            else if (tbCompressionLevel.Value < 9)
            {
                lblCompressionLevel.Text = Resources.DLG_UC_CONFIG_LBL_HIGH_COMPRESSION;
            }
            else
            {
                lblCompressionLevel.Text = Resources.DLG_UC_CONFIG_LBL_HIGHEST_COMPRESSION;
            }

            lblCompressionLevel.Text += Resources.DLG_UC_CONFIG_LBL_COMPRESSION_STAGE + tbCompressionLevel.Value.ToString();
        }

        private void rdCompress_CheckedChanged(object sender, EventArgs e)
        {
            tbCompressionLevel.Enabled = rdCompress.Checked;
            cmdExcludeCompress.Enabled = rdCompress.Checked;
        }

        private async void cboMedia_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboMedia.Tag.ToString()))
            {
                return;
            }

            switch (cboMedia.SelectedIndex)
            {
                case 0:

                    if (MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_WARN_EXISTING_BACKUP_DELETION_TEXT, Resources.DLG_UC_CONFIG_MSG_WARN_EXISTING_BACKUP_DELETION_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        // delete backups
                        using (var dlgStatus = new frmShortStatus())
                        {
                            dlgStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_DELETE_BACKUP_TEXT;
                            dlgStatus.Show(SuperBase);

                            var versions = BackupLogic.GlobalBackup.QueryManager.GetVersions().Select(x => x.Id).ToList();
                            await BackupLogic.BackupController.DeleteBackupsAsync(versions, false);

                            dlgStatus.Close();
                        }

                        plDevice.Visible = true;
                        plFTP.Visible = false;
                    }
                    else
                    {
                        // reset
                        cboMedia.Tag = "";
                        cboMedia.SelectedIndex = 1;
                        cboMedia.Tag = "1";
                    }

                    break;

                case 1:

                    if (MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_WARN_EXISTING_BACKUP_DELETION_FTP_TEXT, Resources.DLG_UC_CONFIG_MSG_WARN_EXISTING_BACKUP_DELETION_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        // delete backups
                        using (var formShortStatus = new frmShortStatus())
                        {
                            formShortStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_DELETE_BACKUP_TEXT;
                            formShortStatus.Show(SuperBase);

                            var versions = BackupLogic.GlobalBackup.QueryManager.GetVersions().Select(x => x.Id).ToList();
                            await BackupLogic.BackupController.DeleteBackupsAsync(versions, false);

                            formShortStatus.Close();
                        }

                        plDevice.Visible = false;
                        plFTP.Visible = true;
                    }
                    else
                    {
                        // reset
                        cboMedia.Tag = "";
                        cboMedia.SelectedIndex = 0;
                        cboMedia.Tag = "1";
                    }

                    break;
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            // show status
            using (var dlgStatus = new frmShortStatus())
            {
                dlgStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_FINISHING_BACKUP_TEXT;
                dlgStatus.Show(SuperBase);

                if (!await BackupLogic.BackupController.CheckMediaAsync(ActionType.Modify, true))
                {
                    dlgStatus.Close();
                    return;
                }

                // update database file
                BackupLogic.GlobalBackup.BackupService.UpdateDatabaseFile(BackupLogic.GlobalBackup.DatabaseFile);

                // update status
                dlgStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_INIT_BACKUP_TEXT;

                using (var dlgChangeMedia = new frmChangeMedia())
                {
                    if (dlgChangeMedia.ShowDialog() != DialogResult.OK)
                    {
                        dlgStatus.Close();
                        return;
                    }

                    if (dlgChangeMedia.cboMedia.SelectedIndex == 0)
                    {
                        // directory
                        BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 1;
                        BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder = Convert.ToString(dlgChangeMedia.lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + @"\" + Environment.UserName;
                        BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder.Substring(0, 1) + @":\");
                        if (BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial == "0")
                        {
                            BackupLogic.GlobalBackup.ConfigurationManager.MediaVolumeSerial = "";
                        }
                    }
                    else
                    {
                        // FTP server
                        BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 3;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpHost = dlgChangeMedia.txtFTPServer.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpPort = dlgChangeMedia.txtFTPPort.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpUser = dlgChangeMedia.txtFTPUsername.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpPass = dlgChangeMedia.txtFTPPassword.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpFolder = dlgChangeMedia.txtFTPPath.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpCoding = Convert.ToString(dlgChangeMedia.cboFtpEncoding.SelectedItem);
                    }
                }

                // delete all backups from database
                BackupLogic.GlobalBackup.ExecuteNonQuery("DELETE FROM versiontable");
                BackupLogic.GlobalBackup.ExecuteNonQuery("DELETE FROM filelink");
                BackupLogic.GlobalBackup.ExecuteNonQuery("DELETE FROM fileversiontable");

                // update database file
                BackupLogic.GlobalBackup.BackupService.UpdateDatabaseFile(BackupLogic.GlobalBackup.DatabaseFile);
                save = false;
                SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;

                dlgStatus.Close();
            }
        }

        private void lstExcludeCompress_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDeleteExcludeCompress.Enabled = lstExcludeCompress.SelectedIndex != -1;
        }

        private void cmdAddExcludeCompress_Click(object sender, EventArgs e)
        {
            string extension = InputBox.ShowInputBox(this, Resources.DLG_UC_CONFIG_MSG_INPUT_FILE_EXTENSION_EXCLUDE_TEXT, Resources.DLG_UC_CONFIG_MSG_INPUT_FILE_EXTENSION_EXCLUDE_TITLE, "");
            if (string.IsNullOrEmpty(extension))
            {
                return;
            }

            if (extension.StartsWith("*"))
            {
                extension = extension.Substring(1, extension.Length - 1);
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            // check if entry is already present
            foreach (object entry2 in lstExcludeCompress.Items)
            {
                if ((lstExcludeCompress.GetItemText(entry2) ?? "") == (extension ?? ""))
                {
                    return;
                }
            }

            lstExcludeCompress.Items.Add(extension.ToLower());
        }

        private void cmdExcludeCompress_Click(object sender, EventArgs e)
        {
            plExcludeCompress.Visible = true;
            plExcludeCompress.BringToFront();
            tcOptions.Enabled = false;
            cmdOK.Enabled = false;
        }

        private void cmdCloseExcludeCompress_Click(object sender, EventArgs e)
        {
            plExcludeCompress.Visible = false;
            plExcludeCompress.SendToBack();
            tcOptions.Enabled = true;
            cmdOK.Enabled = true;
        }

        private void cmdDeleteExcludeCompress_Click(object sender, EventArgs e)
        {
            if (lstExcludeCompress.SelectedItems.Count <= 0)
            {
                return;
            }

            lstExcludeCompress.Items.Remove(lstExcludeCompress.SelectedItem);
        }

        private void chkRemind_CheckedChanged(object sender, EventArgs e)
        {
            nudRemind.Enabled = chkRemind.Checked;
        }

        private void txtBackupPath_TextChanged(object sender, EventArgs e)
        {
            plUNCAuthentication.Visible = txtBackupPath.Text.StartsWith(@"\\");
        }

        private void lvSource_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                AddSourceFolder(path);
            }
        }

        private void lvSource_DragEnter(object sender, DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                if (Directory.Exists(path))
                {
                    effects = DragDropEffects.Copy;
                }
            }

            e.Effect = effects;
        }

        private void lvSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDeleteSource.Enabled = lvSource.SelectedItems.Count > 0;
        }
    }
}