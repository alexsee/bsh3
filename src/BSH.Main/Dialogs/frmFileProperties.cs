// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Main;

public partial class frmFileProperties
{
    public frmFileProperties()
    {
        InitializeComponent();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public frmBrowser BrowserWindow
    {
        get;
        set;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentFileFolder
    {
        get;
        set;
    }

    internal WinFormsPreviewService PreviewService = new();

    private async void cmdPreview_Click(object sender, EventArgs e)
    {
        await PreviewSelectedFileAsync();
    }

    internal async Task PreviewSelectedFileAsync()
    {
        if (lvVersions.SelectedItems.Count > 0)
        {
            await PreviewService.PreviewAsync(() => (int.Parse(((FileTableRow)lvVersions.SelectedItems[0].Tag).FilePackage),
                lblFileName.Text, CurrentFileFolder));
        }
    }

    private async void cmdChange_Click(object sender, EventArgs e)
    {
        if (lvVersions.SelectedItems.Count <= 0)
        {
            return;
        }

        await BrowserWindow.AVersionList1.SelectItemAsync(((FileTableRow)lvVersions.SelectedItems[0].Tag).FilePackage);
        await BrowserWindow.OpenFolderAsync(CurrentFileFolder);
        Close();
    }

    private async void cmdRestore_Click(object sender, EventArgs e)
    {
        // Zielverzeichnis
        var Destination = "";

        // Rechte Maustaste?
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            using var dlgFolderBrowser = new FolderBrowserDialog();
            if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Destination = dlgFolderBrowser.SelectedPath;
        }

        // Wurde überhaupt irgendetwas ausgewählt?
        if (lvVersions.SelectedItems.Count > 0 && lvVersions.SelectedItems[0].Tag is FileTableRow version)
        {
            await BackupLogic.BackupController.RestoreBackupAsync(version.FilePackage, CurrentFileFolder + '\\' + lblFileName.Text, Destination);
        }
    }

    private void lvVersions_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvVersions.SelectedItems.Count > 0)
        {
            cmdChange.Enabled = ((FileTableRow)lvVersions.SelectedItems[0].Tag).VersionStatus == "0";
            cmdPreview.Enabled = true;
            cmdRestore.Enabled = true;
        }
        else
        {
            cmdChange.Enabled = false;
            cmdPreview.Enabled = false;
            cmdRestore.Enabled = false;
        }
    }
}
