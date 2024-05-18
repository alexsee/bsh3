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

using Brightbits.BSH.Engine.Storage;
using BSH.Main.Properties;
using System;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmChangeMedia
{
    public frmChangeMedia()
    {
        InitializeComponent();
    }

    private void cboMedia_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (cboMedia.SelectedIndex)
        {
            case 0:
                PopulateDrives();

                plDevice.Visible = true;
                plFTP.Visible = false;

                break;

            case 1:
                plDevice.Visible = false;
                plFTP.Visible = true;

                break;
        }
    }

    private void PopulateDrives()
    {
        // Laufwerke hinzufügen
        var drives = System.IO.DriveInfo.GetDrives();
        lvBackupDrive.Items.Clear();

        foreach (var entry in drives)
        {
            // Bereit?
            if (!entry.IsReady)
            {
                continue;
            }

            // Bild
            var iImageKey = 2;
            var gGroup = lvBackupDrive.Groups[0];
            if (entry.DriveType == System.IO.DriveType.Fixed)
            {
                iImageKey = 2;
                gGroup = lvBackupDrive.Groups[0];
            }

            if (entry.DriveType == System.IO.DriveType.Removable)
            {
                iImageKey = 3;
                gGroup = lvBackupDrive.Groups[1];
            }

            if (entry.DriveType == System.IO.DriveType.Network)
            {
                iImageKey = 1;
                gGroup = lvBackupDrive.Groups[2];
            }

            var newEntry = lvBackupDrive.Items.Add(entry.Name + " (" + entry.VolumeLabel + ")", iImageKey);
            newEntry.Group = gGroup;
            newEntry.Tag = entry.RootDirectory.FullName;
        }
    }

    private void cmdRefresh_Click(object sender, EventArgs e)
    {
        PopulateDrives();
    }

    private void Button1_Click(object sender, EventArgs e)
    {
        // Wenigstens eine Option ist korrekt gewählt
        if (cboMedia.SelectedIndex == 0)
        {
            if (lvBackupDrive.SelectedItems.Count <= 0)
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TEXT, Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (System.IO.File.Exists(lvBackupDrive.SelectedItems[0].Tag.ToString() + @"Backups\" + Environment.MachineName + '\\' + Environment.UserName + @"\backup.bshdb"))
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        else
        {
            // FTP testen
            try
            {
                txtFTPPath.Text = FtpStorage.GetFtpPath(txtFTPPath.Text);

                using (var storage = new FtpStorage(
                    txtFTPServer.Text,
                    int.Parse(txtFTPPort.Text),
                    txtFTPUsername.Text,
                    txtFTPPassword.Text,
                    txtFTPPath.Text,
                    cboFtpEncoding.SelectedItem.ToString(),
                    !chkFtpEncryption.Checked,
                    0))
                {
                    storage.Open();

                    if (storage.FileExists("backup.bshdb"))
                    {
                        MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Verbindungdaten falsch
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void frmChangeMedia_Load(object sender, EventArgs e)
    {
        cboMedia.SelectedIndex = 0;
    }

    private void cmdFTPCheck_Click(object sender, EventArgs e)
    {
        // FTP testen
        try
        {
            txtFTPPath.Text = FtpStorage.GetFtpPath(txtFTPPath.Text);

            var profile = FtpStorage.CheckConnection(txtFTPServer.Text, int.Parse(txtFTPPort.Text), txtFTPUsername.Text, txtFTPPassword.Text, txtFTPPath.Text, cboFtpEncoding.SelectedItem.ToString());

            if (!profile)
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_UNSPECIFIC_TEXT, Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_UNSPECIFIC_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_INFO_FTP_SUCCESSFUL_TEXT, Resources.DLG_CHANGE_MEDIA_MSG_INFO_FTP_SUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // Verbindungdaten falsch
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}