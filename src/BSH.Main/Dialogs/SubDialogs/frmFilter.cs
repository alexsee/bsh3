// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BSH.Controls.UI;
using Resources = BSH.Main.Properties.Resources;

namespace Brightbits.BSH.Main;

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
            foreach (var entry in BackupLogic.ConfigurationManager.ExcludeFolder.Split('|'))
            {
                // Verzeichnis gefunden
                if (!string.IsNullOrEmpty(entry))
                {
                    lstExcludeFolders.Items.Add(entry);
                }
            }

            // Dateien ausschließen
            foreach (var entry in BackupLogic.ConfigurationManager.ExcludeFileTypes.Split('|'))
            {
                // Datei gefunden
                if (!string.IsNullOrEmpty(entry))
                {
                    lstExcludeFiles.Items.Add(entry);
                }
            }

            // Dateigröße einschränken
            chkFilesBigger.Checked = !string.IsNullOrEmpty(BackupLogic.ConfigurationManager.ExcludeFileBigger);
            if (chkFilesBigger.Checked)
            {
                txtFilesBigger.Value = decimal.Parse(BackupLogic.ConfigurationManager.ExcludeFileBigger);
            }

            // Dateien ausschließen
            foreach (var entry in BackupLogic.ConfigurationManager.ExcludeFile.Split('|'))
            {
                // Datei gefunden
                if (!string.IsNullOrEmpty(entry))
                {
                    lstExcludeSingleFile.Items.Add(entry);
                }
            }

            if (!string.IsNullOrEmpty(BackupLogic.ConfigurationManager.ExcludeMask))
            {
                txtRegEx.Text = BackupLogic.ConfigurationManager.ExcludeMask.Replace("|", "\r\n");
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
            var sExcludeFolders = string.Join("|", lstExcludeFolders.Items.Cast<object>().Select(x => lstExcludeFolders.GetItemText(x)));
            BackupLogic.ConfigurationManager.ExcludeFolder = sExcludeFolders;
        }
        else
        {
            BackupLogic.ConfigurationManager.ExcludeFolder = "";
        }

        // Dateitypen ausschließen
        if (lstExcludeFiles.Items.Count > 0)
        {
            var sExcludeFiles = string.Join("|", lstExcludeFiles.Items.Cast<object>().Select(x => lstExcludeFiles.GetItemText(x)));
            BackupLogic.ConfigurationManager.ExcludeFileTypes = sExcludeFiles;
        }
        else
        {
            BackupLogic.ConfigurationManager.ExcludeFileTypes = "";
        }

        // Dateigröße einschränken
        if (chkFilesBigger.Checked)
        {
            BackupLogic.ConfigurationManager.ExcludeFileBigger = txtFilesBigger.Value.ToString();
        }
        else
        {
            BackupLogic.ConfigurationManager.ExcludeFileBigger = "";
        }

        // check if regex is parsable
        try
        {
            var regularExpression = txtRegEx.Text.Trim().Replace("\r\n", "|").Trim().Replace("||", "|");
            if (!string.IsNullOrEmpty(regularExpression))
            {
                Regex.IsMatch(@"C:\test.txt", regularExpression, RegexOptions.Singleline, TimeSpan.FromSeconds(10));
            }

            BackupLogic.ConfigurationManager.ExcludeMask = regularExpression;
        }
        catch
        {
            MessageBox.Show(Resources.DLG_FILTER_MSG_ERROR_INVALID_REGEX_TEXT, Resources.DLG_FILTER_MSG_ERROR_INVALID_REGEX_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
        }

        // Datei ausschließen
        if (lstExcludeSingleFile.Items.Count > 0)
        {
            var sExcludeSingleFile = string.Join("|", lstExcludeSingleFile.Items.Cast<object>().Select(lstExcludeSingleFile.GetItemText));
            BackupLogic.ConfigurationManager.ExcludeFile = sExcludeSingleFile;
        }
        else
        {
            BackupLogic.ConfigurationManager.ExcludeFile = "";
        }

        // Fenster schließen
        Close();
    }

    private void cmdAddFolders_Click(object sender, EventArgs e)
    {
        using var dlgFolderBrowser = new FolderBrowserDialog();
        dlgFolderBrowser.SelectedPath = onLoadPath;

        if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        // Verzeichnis soll gefiltert werden
        // Zunächst prüfen, ob Verzeichnis in einem der Quellverzeichnisse
        foreach (var entry in BackupLogic.ConfigurationManager.SourceFolder.Split('|'))
        {
            if (!dlgFolderBrowser.SelectedPath.Contains(entry, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Verzeichnis kann sortiert werden
            var sTemp = dlgFolderBrowser.SelectedPath.Replace(entry[..entry.LastIndexOf('\\')], "");

            if (!string.IsNullOrEmpty(sTemp))
            {
                // Nachschauen, ob schon drin
                foreach (var entry2 in lstExcludeFolders.Items)
                {
                    if (lstExcludeFolders.GetItemText(entry2) == sTemp)
                    {
                        // Eintrag gibts schon
                        onLoadPath = dlgFolderBrowser.SelectedPath;
                        return;
                    }
                }

                lstExcludeFolders.Items.Add(sTemp);
                onLoadPath = dlgFolderBrowser.SelectedPath;
                return;
            }
        }

        // Verzeichnis nicht in Quellverzeichnis
        MessageBox.Show(Resources.DLG_FILTER_MSG_ERROR_DIRECTORY_INVALID_TEXT, Resources.DLG_FILTER_MSG_ERROR_DIRECTORY_INVALID_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var sInput = InputBox.ShowInputBox(this, Resources.DLG_FILTER_INPUT_FILE_TYPE_TEXT, Resources.DLG_FILTER_INPUT_FILE_TYPE_TITLE, "");
            if (string.IsNullOrEmpty(sInput))
            {
                return;
            }

            if (sInput.StartsWith('*'))
            {
                sInput = sInput[1..];
            }

            if (sInput.StartsWith('.'))
            {
                sInput = sInput[1..];
            }

            // Nachschauen, ob schon drin
            foreach (var entry2 in lstExcludeFiles.Items)
            {
                if (lstExcludeFiles.GetItemText(entry2) == sInput)
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
            Process.Start(Resources.DLG_FILTER_REGEX_LINK);
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
            foreach (var File in dlgOpenFile.FileNames)
            {
                // Zunächst prüfen, ob Datei in einem der Quellverzeichnisse
                var bAdded = false;
                foreach (var entry in BackupLogic.ConfigurationManager.SourceFolder.Split('|'))
                {
                    if (!System.IO.Path.GetFullPath(File).Contains(entry, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Datei kann sortiert werden
                    var sTemp = File.Replace(entry.Substring(0, entry.LastIndexOf('\\')), "");
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        // Nachschauen, ob schon drin
                        foreach (var entry2 in lstExcludeSingleFile.Items)
                        {
                            if ((lstExcludeSingleFile.GetItemText(entry2) ?? "") == sTemp)
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
                    MessageBox.Show(string.Format(Resources.DLG_FILTER_MSG_ERROR_FILE_INVALID_TEXT, File), Resources.DLG_FILTER_MSG_ERROR_FILE_INVALID_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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