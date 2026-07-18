// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

public partial class frmChangeMedia
{
    private const int MediaIndexLocal = 0;
    private const int MediaIndexFtp = 1;
    private const int MediaIndexUnc = 2;

    private Panel plUNC;
    private TextBox txtUncPath;
    private TextBox txtUncUsername;
    private TextBox txtUncPassword;

    public frmChangeMedia()
    {
        InitializeComponent();
        EnsureUncUi();
    }

    public bool IsUncSelected => cboMedia.SelectedIndex == MediaIndexUnc;

    public string UncPath => txtUncPath?.Text?.Trim() ?? "";

    public string UncUsername => txtUncUsername?.Text ?? "";

    public string UncPassword => txtUncPassword?.Text ?? "";

    private void EnsureUncUi()
    {
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChangeMedia));
        if (cboMedia.Items.Count == 2)
        {
            cboMedia.Items.Add(resources.GetString("cboMedia.Items2") ?? "Netzwerkfreigabe (UNC)");
        }

        plUNC = new Panel
        {
            Location = plFTP.Location,
            Size = plFTP.Size,
            BackColor = Color.Transparent,
            Visible = false,
            Name = "plUNC",
        };

        var lblPath = new Label { Text = "Pfad:", AutoSize = true, Location = new Point(3, 18) };
        txtUncPath = new TextBox { Location = new Point(120, 15), Width = Math.Max(120, plFTP.Width - 130), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

        var lblUser = new Label { Text = "Benutzername:", AutoSize = true, Location = new Point(3, 55) };
        txtUncUsername = new TextBox { Location = new Point(120, 52), Width = 180 };

        var lblPass = new Label { Text = "Kennwort:", AutoSize = true, Location = new Point(3, 92) };
        txtUncPassword = new TextBox
        {
            Location = new Point(120, 89),
            Width = 180,
            UseSystemPasswordChar = true,
        };

        var cmdCheck = new Button
        {
            Text = "Prüfen…",
            Location = new Point(120, 130),
            AutoSize = true,
        };
        cmdCheck.Click += (_, _) => ValidateUncTarget(showSuccessMessage: true);

        plUNC.Controls.AddRange([lblPath, txtUncPath, lblUser, txtUncUsername, lblPass, txtUncPassword, cmdCheck]);
        Controls.Add(plUNC);
        plUNC.BringToFront();
    }

    private void cboMedia_SelectedIndexChanged(object sender, EventArgs e)
    {
        var index = cboMedia.SelectedIndex;
        plDevice.Visible = index == MediaIndexLocal;
        plFTP.Visible = index == MediaIndexFtp;
        plUNC.Visible = index == MediaIndexUnc;

        if (index == MediaIndexLocal)
        {
            PopulateDrives();
        }
    }

    private void PopulateDrives()
    {
        var drives = DriveInfo.GetDrives();
        lvBackupDrive.Items.Clear();

        foreach (var entry in drives)
        {
            if (!entry.IsReady)
            {
                continue;
            }

            var iImageKey = 2;
            var gGroup = lvBackupDrive.Groups[0];
            if (entry.DriveType == DriveType.Fixed)
            {
                iImageKey = 2;
                gGroup = lvBackupDrive.Groups[0];
            }

            if (entry.DriveType == DriveType.Removable)
            {
                iImageKey = 3;
                gGroup = lvBackupDrive.Groups[1];
            }

            if (entry.DriveType == DriveType.Network)
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
        if (cboMedia.SelectedIndex == MediaIndexLocal)
        {
            if (!ValidateLocalTarget())
            {
                return;
            }
        }
        else if (cboMedia.SelectedIndex == MediaIndexUnc)
        {
            if (!ValidateUncTarget(showSuccessMessage: false))
            {
                return;
            }
        }
        else
        {
            if (!ValidateFtpTarget())
            {
                return;
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private bool ValidateLocalTarget()
    {
        if (lvBackupDrive.SelectedItems.Count <= 0)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TEXT, Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        var backupDb = lvBackupDrive.SelectedItems[0].Tag.ToString() + @"Backups\" + Environment.MachineName + '\\' + Environment.UserName + @"\backup.bshdb";
        if (File.Exists(backupDb))
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return true;
    }

    private bool ValidateUncTarget(bool showSuccessMessage)
    {
        if (!MediaTargetApplier.IsUncPath(UncPath))
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TEXT, Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        var path = UncPath.Replace('/', '\\');
        try
        {
            using var networkConnection = new NetworkConnection(path, UncUsername, UncPassword);
            if (!Directory.Exists(path))
            {
                MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (File.Exists(Path.Combine(path, "backup.bshdb")))
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT + Environment.NewLine + ex.Message, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (showSuccessMessage)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_INFO_FTP_SUCCESSFUL_TEXT, Resources.DLG_CHANGE_MEDIA_MSG_INFO_FTP_SUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        return true;
    }

    private bool ValidateFtpTarget()
    {
        try
        {
            txtFTPPath.Text = FtpStorage.GetFtpPath(txtFTPPath.Text);

            using var storage = new FtpStorage(
                txtFTPServer.Text,
                int.Parse(txtFTPPort.Text),
                txtFTPUsername.Text,
                txtFTPPassword.Text,
                txtFTPPath.Text,
                cboFtpEncoding.SelectedItem.ToString(),
                !chkFtpEncryption.Checked,
                0);
            storage.Open();

            if (storage.FileExists("backup.bshdb"))
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message, Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return true;
    }

    private void frmChangeMedia_Load(object sender, EventArgs e)
    {
        cboMedia.SelectedIndex = MediaIndexLocal;
    }

    private void cmdFTPCheck_Click(object sender, EventArgs e)
    {
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
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message, Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
