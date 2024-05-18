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
using Brightbits.BSH.Engine.Models;
using BSH.Main.Properties;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public partial class frmFileProperties
{
    public frmFileProperties()
    {
        InitializeComponent();
    }

    public frmBrowser BrowserWindow
    {
        get;
        set;
    }
    public string CurrentFileFolder
    {
        get;
        set;
    }

    private async void cmdPreview_Click(object sender, EventArgs e)
    {
        if (lvVersions.SelectedItems.Count <= 0)
        {
            return;
        }

        if (!await BackupLogic.BackupController.CheckMediaAsync(ActionType.Restore))
        {
            return;
        }

        if (!BackupLogic.BackupController.RequestPassword())
        {
            return;
        }

        // Schnellansicht laden
        try
        {
            var id = int.Parse(((FileTableRow)lvVersions.SelectedItems[0].Tag).FilePackage);

            var password = BackupLogic.BackupService.GetPassword();
            var tmpFile = await BackupLogic.QueryManager.GetFileNameFromDriveAsync(id, lblFileName.Text, CurrentFileFolder, password);

            var procInfo = new ProcessStartInfo(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\SmartPreview.exe", " -file:\"" + tmpFile.Item1 + "\"" + (tmpFile.Item2 ? " -c" : ""));
            procInfo.WindowStyle = ProcessWindowStyle.Normal;

            var proc = Process.Start(procInfo);
            proc.WaitForExit();

            if (tmpFile.Item1 != null && tmpFile.Item2)
            {
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        System.IO.File.Delete(tmpFile.Item1);
                        break;
                    }
                    catch
                    {
                        // next try
                    }
                }
            }
        }
        catch
        {
            // Fehler: Feature nicht installiert?
            MessageBox.Show(Resources.DLG_FEATURE_NOT_AVAILABLE_TEXT, Resources.DLG_FEATURE_NOT_AVAILABLE_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }

    private async void cmdChange_Click(object sender, EventArgs e)
    {
        if (lvVersions.SelectedItems.Count <= 0)
        {
            return;
        }

        BrowserWindow.AVersionList1.SelectItem(((FileTableRow)lvVersions.SelectedItems[0].Tag).FilePackage);
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
        if (lvVersions.SelectedItems.Count > 0)
        {
            await BackupLogic.BackupController.RestoreBackupAsync(lvVersions.SelectedItems[0].Tag.ToString(), CurrentFileFolder + '\\' + lblFileName.Text, Destination);
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