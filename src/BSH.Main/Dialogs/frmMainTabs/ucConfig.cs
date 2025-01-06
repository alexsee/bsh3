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

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Controls.UI;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

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

        StoreSettingsAsync().Wait();
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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public frmMain Super
    {
        set
        {
            SuperBase = value;
        }
    }

    #endregion

    private async Task StoreSettingsAsync()
    {
        // wait for any other tasks finished
        if (StatusController.Current.IsTaskRunning())
        {
            using var dlgShortStatus = new frmShortStatus();
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
            BackupLogic.ConfigurationManager.MediumType = MediaType.FileTransferServer;
        }
        else
        {
            // directory
            BackupLogic.ConfigurationManager.MediumType = MediaType.LocalDevice;

            // UNC authentication
            if (txtBackupPath.Text.StartsWith(@"\\"))
            {
                BackupLogic.ConfigurationManager.UNCUsername = txtUNCUsername.Text;
                BackupLogic.ConfigurationManager.UNCPassword = Crypto.EncryptString(txtUNCPassword.Text, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                BackupLogic.ConfigurationManager.MediaVolumeSerial = "";
            }
            else
            {
                BackupLogic.ConfigurationManager.UNCUsername = "";
                BackupLogic.ConfigurationManager.UNCPassword = "";

                // MedienID speichern
                BackupLogic.ConfigurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(txtBackupPath.Text[..1] + @":\");
                if (BackupLogic.ConfigurationManager.MediaVolumeSerial == null ||
                    BackupLogic.ConfigurationManager.MediaVolumeSerial == "0")
                {
                    BackupLogic.ConfigurationManager.MediaVolumeSerial = "";
                }
            }
        }

        var configurationManager = BackupLogic.ConfigurationManager;
        configurationManager.FtpHost = txtFTPServer.Text;
        configurationManager.FtpPort = txtFTPPort.Text;
        configurationManager.FtpUser = txtFTPUsername.Text;
        configurationManager.FtpPass = txtFTPPassword.Text;
        configurationManager.FtpFolder = FtpStorage.GetFtpPath(txtFTPPath.Text);
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
            var excludeCompression = string.Join("|", lstExcludeCompress.Items.Cast<object>().Select(x => lstExcludeCompress.GetItemText(x)));
            configurationManager.ExcludeCompression = excludeCompression;
        }

        // compression factor
        configurationManager.RemindAfterDays = chkRemind.Checked ? ((int)nudRemind.Value).ToString() : "";

        // stop system
        BackupLogic.StopSystem();
        await BackupLogic.StartSystemAsync();
    }

    private void LoadSettings()
    {
        lvSource.Items.Clear();

        // load sources
        if (!string.IsNullOrEmpty(BackupLogic.ConfigurationManager.SourceFolder))
        {
            var sources = BackupLogic.ConfigurationManager.SourceFolder.Split('|');
            foreach (var entry in sources)
            {
                lvSource.Items.Add(entry, 0);
            }
        }

        // block media selection
        cboMedia.Tag = "";

        // load backup storage
        if (BackupLogic.ConfigurationManager.MediumType == MediaType.FileTransferServer)
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

        var configurationManager = BackupLogic.ConfigurationManager;
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
        if (txtBackupPath.Text.StartsWith('\\'))
        {
            txtUNCUsername.Text = configurationManager.UNCUsername;
            txtUNCPassword.Text = Crypto.DecryptString(configurationManager.UNCPassword, System.Security.Cryptography.DataProtectionScope.LocalMachine);
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
            foreach (var entry in excludeCompression)
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
        using var dlgFolderBrowser = new FolderBrowserDialog();
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

    private void cmdAddSource_Click(object sender, EventArgs e)
    {
        using var dlgFolderBrowser = new FolderBrowserDialog();
        dlgFolderBrowser.SelectedPath = selectedFolderCache;

        if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
        {
            selectedFolderCache = dlgFolderBrowser.SelectedPath;
            AddSourceFolder(selectedFolderCache);
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
            using var dlgShortStatus = new frmShortStatus();
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

        // store source folder
        if (lvSource.Items.Count == 0)
        {
            BackupLogic.ConfigurationManager.SourceFolder = "";
        }
        else
        {
            BackupLogic.ConfigurationManager.SourceFolder = string.Join("|", lvSource.Items.Cast<ListViewItem>().Select(x => x.Text));
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
            BackupLogic.ConfigurationManager.SourceFolder = "";
        }
        else
        {
            BackupLogic.ConfigurationManager.SourceFolder = string.Join("|", lvSource.Items.Cast<ListViewItem>().Select(x => x.Text));
        }

        cmdDeleteSource_Click(sender, e);
    }

    private void cmdFTPCheck_Click(object sender, EventArgs e)
    {
        try
        {
            // check FTP
            var profile = FtpStorage.CheckConnection(txtFTPServer.Text, Convert.ToInt32(txtFTPPort.Text), txtFTPUsername.Text, txtFTPPassword.Text, txtFTPPath.Text, Convert.ToString(cboFtpEncoding.SelectedItem));

            if (profile)
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

        BackupLogic.ConfigurationManager.EncryptPassMD5 = Hash.GetMD5Hash(password);
        BackupLogic.ConfigurationManager.Encrypt = 1;
        cmdEncrypt.Enabled = false;

        OpenTab();
    }

    private void cmdFilter_Click(object sender, EventArgs e)
    {
        using var dlgFilter = new frmFilter();
        dlgFilter.ShowDialog(this);
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

            var task = BackupLogic.BackupService.StartEdit(ref statusWindowReport, new CancellationTokenSource().Token, true);
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
        using var dlgEditScheduler = new frmEditScheduler();
        dlgEditScheduler.ShowDialog(this);
    }

    private void chkRemindSpace_CheckedChanged(object sender, EventArgs e)
    {
        txtRemindSpace.Enabled = chkRemindSpace.Checked;
    }

    private void rdCompress_CheckedChanged(object sender, EventArgs e)
    {
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

                        var versions = BackupLogic.QueryManager.GetVersions().Select(x => x.Id).ToList();
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

                        var versions = BackupLogic.QueryManager.GetVersions().Select(x => x.Id).ToList();
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
        using var dlgStatus = new frmShortStatus();
        dlgStatus.lblStatus.Text = Resources.DLG_UC_CONFIG_STATUS_FINISHING_BACKUP_TEXT;
        dlgStatus.Show(SuperBase);

        if (!await BackupLogic.BackupController.CheckMediaAsync(ActionType.Modify, true))
        {
            dlgStatus.Close();
            return;
        }

        // update database file
        BackupLogic.BackupService.UpdateDatabaseFile(BackupLogic.DatabaseFile);

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
                BackupLogic.ConfigurationManager.MediumType = MediaType.LocalDevice;
                BackupLogic.ConfigurationManager.BackupFolder = Convert.ToString(dlgChangeMedia.lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + @"\" + Environment.UserName;
                BackupLogic.ConfigurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(BackupLogic.ConfigurationManager.BackupFolder[..1] + @":\");
                if (BackupLogic.ConfigurationManager.MediaVolumeSerial == "0")
                {
                    BackupLogic.ConfigurationManager.MediaVolumeSerial = "";
                }
            }
            else
            {
                // FTP server
                BackupLogic.ConfigurationManager.MediumType = MediaType.FileTransferServer;
                BackupLogic.ConfigurationManager.FtpHost = dlgChangeMedia.txtFTPServer.Text;
                BackupLogic.ConfigurationManager.FtpPort = dlgChangeMedia.txtFTPPort.Text;
                BackupLogic.ConfigurationManager.FtpUser = dlgChangeMedia.txtFTPUsername.Text;
                BackupLogic.ConfigurationManager.FtpPass = dlgChangeMedia.txtFTPPassword.Text;
                BackupLogic.ConfigurationManager.FtpFolder = dlgChangeMedia.txtFTPPath.Text;
                BackupLogic.ConfigurationManager.FtpCoding = Convert.ToString(dlgChangeMedia.cboFtpEncoding.SelectedItem);
            }
        }

        // delete all backups from database
        await BackupLogic.DbClientFactory.ExecuteNonQueryAsync("DELETE FROM versiontable");
        await BackupLogic.DbClientFactory.ExecuteNonQueryAsync("DELETE FROM filelink");
        await BackupLogic.DbClientFactory.ExecuteNonQueryAsync("DELETE FROM fileversiontable");

        // update database file
        BackupLogic.BackupService.UpdateDatabaseFile(BackupLogic.DatabaseFile);
        save = false;
        SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;

        dlgStatus.Close();
    }

    private void lstExcludeCompress_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmdDeleteExcludeCompress.Enabled = lstExcludeCompress.SelectedIndex != -1;
    }

    private void cmdAddExcludeCompress_Click(object sender, EventArgs e)
    {
        var extension = InputBox.ShowInputBox(this, Resources.DLG_UC_CONFIG_MSG_INPUT_FILE_EXTENSION_EXCLUDE_TEXT, Resources.DLG_UC_CONFIG_MSG_INPUT_FILE_EXTENSION_EXCLUDE_TITLE, "");
        if (string.IsNullOrEmpty(extension))
        {
            return;
        }

        if (extension.StartsWith('*'))
        {
            extension = extension[1..];
        }

        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        // check if entry is already present
        foreach (var entry2 in lstExcludeCompress.Items)
        {
            if ((lstExcludeCompress.GetItemText(entry2) ?? "") == extension)
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
            var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            AddSourceFolder(path);
        }
    }

    private void lvSource_DragEnter(object sender, DragEventArgs e)
    {
        var effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
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