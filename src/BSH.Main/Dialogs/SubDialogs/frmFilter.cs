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

using BSH.Controls.UI;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class frmFilter
    {
        public frmFilter()
        {
            InitializeComponent();
        }

        private string onLoadPath = "";

        private void frmFilter_Load(object sender, EventArgs e)
        {
            try
            {
                // Verzeichnisse ausschließen
                foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFolder.Split('|'))
                {
                    // Verzeichnis gefunden
                    if (!string.IsNullOrEmpty(entry))
                    {
                        lstExcludeFolders.Items.Add(entry);
                    }
                }

                // Dateien ausschließen
                foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileTypes.Split('|'))
                {
                    // Datei gefunden
                    if (!string.IsNullOrEmpty(entry))
                    {
                        lstExcludeFiles.Items.Add(entry);
                    }
                }

                // Dateigröße einschränken
                chkFilesBigger.Checked = string.IsNullOrEmpty(BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileBigger) ? false : true;
                if (chkFilesBigger.Checked)
                {
                    txtFilesBigger.Value = decimal.Parse(BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileBigger);
                }

                // Dateien ausschließen
                foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFile.Split('|'))
                {
                    // Datei gefunden
                    if (!string.IsNullOrEmpty(entry))
                    {
                        lstExcludeSingleFile.Items.Add(entry);
                    }
                }

                if (!string.IsNullOrEmpty(BackupLogic.GlobalBackup.ConfigurationManager.ExcludeMask))
                {
                    txtRegEx.Text = BackupLogic.GlobalBackup.ConfigurationManager.ExcludeMask.Replace("|", "\r\n");
                }
            }
            catch
            {
                // ignore error
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            // Einstellungen speichern

            // Verzeichnisse ausschließen
            if (lstExcludeFolders.Items.Count > 0)
            {
                string sExcludeFolders = string.Join("|", lstExcludeFolders.Items.Cast<object>().Select(x => lstExcludeFolders.GetItemText(x)));
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFolder = sExcludeFolders;
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFolder = "";
            }

            // Dateitypen ausschließen
            if (lstExcludeFiles.Items.Count > 0)
            {
                string sExcludeFiles = string.Join("|", lstExcludeFiles.Items.Cast<object>().Select(x => lstExcludeFiles.GetItemText(x)));
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileTypes = sExcludeFiles;
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileTypes = "";
            }

            // Dateigröße einschränken
            if (chkFilesBigger.Checked)
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileBigger = txtFilesBigger.Value.ToString();
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFileBigger = "";
            }

            // check if regex is parsable
            try
            {
                string regularExpression = txtRegEx.Text.Trim().Replace("\r\n", "|").Trim().Replace("||", "|");
                if (!string.IsNullOrEmpty(regularExpression))
                {
                    Regex.IsMatch(@"C:\test.txt", regularExpression, RegexOptions.Singleline);
                }

                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeMask = regularExpression;
            }
            catch
            {
                MessageBox.Show("Der angegebene reguläre Ausdruck (RegEx) ist ungültig. Bitte prüfen Sie den Ausdruck bevor Sie diesen speichern.", "Reguläre Ausdruck ungültig", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Datei ausschließen
            if (lstExcludeSingleFile.Items.Count > 0)
            {
                string sExcludeSingleFile = string.Join("|", lstExcludeSingleFile.Items.Cast<object>().Select(x => lstExcludeSingleFile.GetItemText(x))); ;
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFile = sExcludeSingleFile;
            }
            else
            {
                BackupLogic.GlobalBackup.ConfigurationManager.ExcludeFile = "";
            }

            // Fenster schließen
            Close();
        }

        private void cmdAddFolders_Click(object sender, EventArgs e)
        {
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                dlgFolderBrowser.SelectedPath = onLoadPath;

                if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Verzeichnis soll gefiltert werden
                // Zunächst prüfen, ob Verzeichnis in einem der Quellverzeichnisse
                bool bAdded = false;
                foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder.Split('|'))
                {
                    if (!dlgFolderBrowser.SelectedPath.ToLower().Contains(entry.ToLower()))
                    {
                        continue;
                    }

                    // Verzeichnis kann sortiert werden
                    string sTemp = dlgFolderBrowser.SelectedPath.Replace(entry.Substring(0, entry.LastIndexOf(@"\")), "");
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        // Nachschauen, ob schon drin
                        foreach (object entry2 in lstExcludeFolders.Items)
                        {
                            if ((lstExcludeFolders.GetItemText(entry2) ?? "") == (sTemp ?? ""))
                            {
                                // Eintrag gibts schon
                                onLoadPath = dlgFolderBrowser.SelectedPath;
                                return;
                            }
                        }

                        lstExcludeFolders.Items.Add(sTemp);
                        bAdded = true;
                        onLoadPath = dlgFolderBrowser.SelectedPath;
                        return;
                    }
                }

                if (!bAdded)
                {
                    // Verzeichnis nicht in Quellverzeichnis
                    MessageBox.Show("Ausgewähltes Verzeichnis kann nicht verwendet werden.\r\n\r\nDas ausgewählte Verzeichnis kann nicht ausgeschlossen werden, da es nicht zur Datensicherung gehört.", "Verzeichnis nicht verwendbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                onLoadPath = dlgFolderBrowser.SelectedPath;
            }
        }

        private void cmdDeleteFolders_Click(object sender, EventArgs e)
        {
            try
            {
                while (lstExcludeFolders.SelectedItems.Count > 0)
                {
                    lstExcludeFolders.Items.Remove(lstExcludeFolders.SelectedItem);
                }
            }
            catch
            {
                // ignore error
            }
        }

        private void cmdAddFile_Click(object sender, EventArgs e)
        {
            try
            {
                string sInput = InputBox.ShowInputBox(this, "Dateityp ausschließen.\r\n\r\nGeben Sie einfach die Dateierweiterung derer Dateien ein, die Sie von der Datensicherung ausschließen möchten. Zum Beispiel: doc", "Dateierweiterung ausschließen", "");
                if (string.IsNullOrEmpty(sInput))
                {
                    return;
                }

                if (sInput.StartsWith("*"))
                {
                    sInput = sInput.Substring(1, sInput.Length - 1);
                }

                sInput = sInput.Replace(".", "");

                // Nachschauen, ob schon drin
                foreach (object entry2 in lstExcludeFiles.Items)
                {
                    if ((lstExcludeFiles.GetItemText(entry2) ?? "") == (sInput ?? ""))
                    {
                        // Eintrag gibts schon
                        return;
                    }
                }

                lstExcludeFiles.Items.Add(sInput);
            }
            catch
            {
                // ignore error
            }
        }

        private void cmdDeleteFile_Click(object sender, EventArgs e)
        {
            try
            {
                while (lstExcludeFiles.SelectedItems.Count > 0)
                {
                    lstExcludeFiles.Items.Remove(lstExcludeFiles.SelectedItem);
                }
            }
            catch
            {
                // ignore error
            }
        }

        private void chkFilesBigger_CheckedChanged(object sender, EventArgs e)
        {
            // "Dateien größer als"-aktivieren
            if (chkFilesBigger.Checked)
            {
                txtFilesBigger.Enabled = true;
            }
            else
            {
                txtFilesBigger.Enabled = false;
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://de.wikipedia.org/wiki/Regulärer_Ausdruck");
            }
            catch
            {
                // ignore error
            }
        }

        private void cmdDeleteSingleFile_Click(object sender, EventArgs e)
        {
            try
            {
                while (lstExcludeSingleFile.SelectedItems.Count > 0)
                {
                    lstExcludeSingleFile.Items.Remove(lstExcludeSingleFile.SelectedItem);
                }
            }
            catch
            {
                // ignore error
            }
        }

        private void cmdAddSingleFile_Click(object sender, EventArgs e)
        {
            using (var dlgOpenFile = new OpenFileDialog())
            {
                dlgOpenFile.Multiselect = true;
                dlgOpenFile.ValidateNames = false;
                dlgOpenFile.CheckFileExists = true;
                dlgOpenFile.CheckPathExists = true;

                if (dlgOpenFile.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Dateien soll gefiltert werden
                foreach (string File in dlgOpenFile.FileNames)
                {
                    // Zunächst prüfen, ob Datei in einem der Quellverzeichnisse
                    bool bAdded = false;
                    foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder.Split('|'))
                    {
                        if (!System.IO.Path.GetFullPath(File).ToLower().Contains(entry.ToLower()))
                        {
                            continue;
                        }

                        // Datei kann sortiert werden
                        string sTemp = File.Replace(entry.Substring(0, entry.LastIndexOf(@"\")), "");
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            // Nachschauen, ob schon drin
                            foreach (object entry2 in lstExcludeSingleFile.Items)
                            {
                                if ((lstExcludeSingleFile.GetItemText(entry2) ?? "") == (sTemp ?? ""))
                                {
                                    // Eintrag gibts schon
                                    return;
                                }
                            }

                            lstExcludeSingleFile.Items.Add(sTemp);
                            bAdded = true;
                            break;
                        }
                    }

                    if (!bAdded)
                    {
                        // Verzeichnis nicht in Quellverzeichnis
                        MessageBox.Show("Ausgewählte Datei \"" + File + "\" kann nicht verwendet werden.\r\n\r\nDie ausgewählte Datei kann nicht ausgeschlossen werden, da es nicht zur Datensicherung gehört.", "Datei nicht verwendbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
        }

        private void lstExcludeFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDeleteFolders.Enabled = lstExcludeFolders.SelectedIndex != -1;
        }

        private void lstExcludeFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDeleteFile.Enabled = lstExcludeFiles.SelectedIndex != -1;
        }

        private void lstExcludeSingleFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDeleteSingleFile.Enabled = lstExcludeSingleFile.SelectedIndex != -1;
        }
    }
}