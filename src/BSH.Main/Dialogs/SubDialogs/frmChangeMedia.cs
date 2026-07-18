// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Storage;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

public partial class frmChangeMedia
{
    public frmChangeMedia()
    {
        InitializeComponent();
    }

    private void cboMedia_SelectedIndexChanged(object sender, EventArgs e)
    {
        var mediaType = MediaTypeExtensions.FromMediaComboIndex(cboMedia.SelectedIndex);

        if (mediaType.IsLocal())
        {
            PopulateDrives();

            plDevice.Visible = true;
            plFTP.Visible = false;
        }
        else if (mediaType.IsRemote())
        {
            plDevice.Visible = false;
            plFTP.Visible = true;
            ApplyRemoteProtocolUi(mediaType);
        }
    }

    private void ApplyRemoteProtocolUi(MediaType mediaType)
    {
        var useWebDav = mediaType.IsWebDav();
        cboFtpEncoding.Visible = !useWebDav;
        Label10.Visible = !useWebDav;
        chkFtpEncryption.Visible = !useWebDav;

        txtFTPPort.Text = mediaType.AdjustPortForProtocol(txtFTPPort.Text);
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
        var mediaType = MediaTypeExtensions.FromMediaComboIndex(cboMedia.SelectedIndex);

        if (mediaType.IsLocal())
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
            // Remote storage testen
            try
            {
                var credentials = CreateRemoteCredentialsFromUi(mediaType);
                txtFTPPath.Text = credentials.Folder;

                using (IStorage storage = StorageFactory.CreateRemote(mediaType, credentials))
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
        // Remote storage testen
        try
        {
            var mediaType = MediaTypeExtensions.FromMediaComboIndex(cboMedia.SelectedIndex);
            var credentials = CreateRemoteCredentialsFromUi(mediaType);
            txtFTPPath.Text = credentials.Folder;

            var profile = StorageFactory.CheckRemoteConnection(mediaType, credentials);

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

    private RemoteStorageCredentials CreateRemoteCredentialsFromUi(MediaType mediaType)
    {
        return new RemoteStorageCredentials
        {
            Host = txtFTPServer.Text,
            Port = int.TryParse(txtFTPPort.Text, out var port) ? port : mediaType.DefaultRemotePort(),
            UserName = txtFTPUsername.Text,
            Password = txtFTPPassword.Text,
            Folder = mediaType.IsFtp() ? FtpStorage.GetFtpPath(txtFTPPath.Text) : txtFTPPath.Text,
            Encoding = cboFtpEncoding.SelectedItem?.ToString() ?? "ISO-8859-1",
            // chkFtpEncryption = "force unencrypted connection"
            UseEncryption = !chkFtpEncryption.Checked
        };
    }

    /// <summary>
    /// Builds remote credentials from the current form fields for the selected media type.
    /// </summary>
    public RemoteStorageCredentials GetRemoteCredentials()
    {
        var mediaType = MediaTypeExtensions.FromMediaComboIndex(cboMedia.SelectedIndex);
        return CreateRemoteCredentialsFromUi(mediaType);
    }
}
