// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
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

    public frmChangeMedia()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Set after a successful OK; null if the dialog was cancelled.
    /// </summary>
    public ChangeMediaResult? Result { get; private set; }

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
        ChangeMediaResult? selection = cboMedia.SelectedIndex switch
        {
            MediaIndexLocal => TryBuildLocalResult(),
            MediaIndexUnc => TryBuildUncResult(showSuccessMessage: false),
            _ => TryBuildFtpResult(),
        };

        if (selection is null)
        {
            return;
        }

        Result = selection;
        DialogResult = DialogResult.OK;
        Close();
    }

    private ChangeMediaResult? TryBuildLocalResult()
    {
        if (lvBackupDrive.SelectedItems.Count <= 0)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TEXT, Resources.DLG_CHANGE_MEDIA_NO_TARGET_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        var driveRoot = Convert.ToString(lvBackupDrive.SelectedItems[0].Tag) ?? "";
        var backupFolder = MediaTargetApplier.BuildLocalBackupFolder(driveRoot);
        if (File.Exists(Path.Combine(backupFolder, "backup.bshdb")))
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        return new ChangeMediaResult
        {
            Kind = ChangeMediaKind.LocalDevice,
            LocalBackupFolder = backupFolder,
        };
    }

    private ChangeMediaResult? TryBuildUncResult(bool showSuccessMessage)
    {
        var uncPath = txtUncPath.Text?.Trim() ?? "";
        if (!MediaTargetApplier.IsUncPath(uncPath))
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_INVALID_UNC_PATH_TEXT, Resources.DLG_CHANGE_MEDIA_INVALID_UNC_PATH_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }

        var path = uncPath.Replace('/', '\\');
        var username = txtUncUsername.Text ?? "";
        var password = txtUncPassword.Text ?? "";

        try
        {
            using var networkConnection = new NetworkConnection(path, username, password);
            if (!Directory.Exists(path))
            {
                MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            if (File.Exists(Path.Combine(path, "backup.bshdb")))
            {
                MessageBox.Show(Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TEXT, Resources.DLG_CHANGE_MEDIA_TARGET_CONTAINS_DATA_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT + Environment.NewLine + ex.Message, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        if (showSuccessMessage)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_INFO_CONNECTION_SUCCESSFUL_TEXT, Resources.DLG_CHANGE_MEDIA_MSG_INFO_CONNECTION_SUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        return new ChangeMediaResult
        {
            Kind = ChangeMediaKind.Unc,
            UncPath = path,
            UncUsername = username,
            UncPassword = password,
        };
    }

    private ChangeMediaResult? TryBuildFtpResult()
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
                return null;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message, Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }

        return new ChangeMediaResult
        {
            Kind = ChangeMediaKind.Ftp,
            FtpHost = txtFTPServer.Text,
            FtpPort = txtFTPPort.Text,
            FtpUsername = txtFTPUsername.Text,
            FtpPassword = txtFTPPassword.Text,
            FtpPath = txtFTPPath.Text,
            FtpEncoding = Convert.ToString(cboFtpEncoding.SelectedItem) ?? "",
        };
    }

    private void frmChangeMedia_Load(object sender, EventArgs e)
    {
        cboMedia.SelectedIndex = MediaIndexLocal;
    }

    private void cmdUncCheck_Click(object sender, EventArgs e)
    {
        TryBuildUncResult(showSuccessMessage: true);
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

            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_INFO_CONNECTION_SUCCESSFUL_TEXT, Resources.DLG_CHANGE_MEDIA_MSG_INFO_CONNECTION_SUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message, Resources.DLG_CHANGE_MEDIA_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
