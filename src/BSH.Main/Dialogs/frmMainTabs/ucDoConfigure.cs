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
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Storage;
using BSH.Main.Properties;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brightbits.BSH.Main
{
    public partial class ucDoConfigure : IMainTabs
    {
        public ucDoConfigure()
        {
            InitializeComponent();
        }

        private string tmpFolderStore = "";

        #region  Implementation of IMainTabs 
        public void CloseTab()
        {

            // Tab verstecken
            Visible = false;
        }

        public void OpenTab()
        {

            // Tab anzeigen
            Visible = true;
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
                return "Konfigurieren";
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

        private int iWizardStep = 1;

        private async void cmdNext_Click(object sender, EventArgs e)
        {
            // wizard: next step
            iWizardStep += 1;
            await ShowWizardStepAsync(iWizardStep);
        }

        private async void cmdBack_Click(object sender, EventArgs e)
        {
            // wizard: previous step
            iWizardStep -= 1;
            await ShowWizardStepAsync(iWizardStep);
        }

        private async Task ShowWizardStepAsync(int iStep)
        {
            iWizardStep = iStep;

            // show wizard step
            switch (iStep)
            {
                case 1:

                    if (Convert.ToInt32(tbStep1.Tag) == Convert.ToInt32(false))
                    {
                        // add home directory of current user
                        if (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Length > 2)
                        {
                            lvSourceFolders.Items.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 0);
                        }

                        tbStep1.Tag = Convert.ToInt32(true);
                    }

                    // show controls
                    tbControl.SelectedTab = tbStep1;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_1;
                    lblDescription.Text = Resources.WELCOME_STEP_1;
                    cmdBack.Enabled = false;
                    cmdNext.Enabled = true;
                    break;

                case 2:

                    // at least one source folder added
                    if (lvSourceFolders.Items.Count <= 0)
                    {
                        MessageBox.Show("Kein Verzeichnis hinzugefügt.\r\n\r\nSie haben kein Verzeichnis hinzugefügt, dass gesichert werden soll. Um den Vorgang fortzusetzen müssen Sie mindestens ein Verzeichnis der Liste hinzufügen.", "Verzeichnis hinzufügen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    if (Convert.ToInt32(tbStep2.Tag) == Convert.ToInt32(false))
                    {
                        // add drives
                        PopulateDrives(lvBackupDrive);
                        tbStep2.Tag = Convert.ToInt32(true);
                    }

                    // show controls
                    tbControl.SelectedTab = tbStep2;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_2;
                    lblDescription.Text = Resources.WELCOME_STEP_2;
                    cmdBack.Enabled = true;
                    cmdNext.Enabled = true;
                    break;

                case 3:

                    // at least one option selected
                    if (tcSource.SelectedIndex == 0)
                    {
                        if (lvBackupDrive.SelectedItems.Count <= 0)
                        {
                            MessageBox.Show("Kein Sicherungsmedium ausgewählt.\r\n\r\nSie haben kein Medium ausgewählt, auf das gesichert werden soll. Um den Vorgang fortzusetzen müssen Sie ein Medium der Liste auswählen.", "Verzeichnis auswählen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        // directory is not empty?
                        var BackupFolder = new DirectoryInfo(Convert.ToString(lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + @"\" + Environment.UserName);

                        if (BackupFolder.Exists)
                        {
                            MessageBox.Show("Das ausgewählte Sicherungsmedium ist nicht leer und enthält bereits Sicherungen von Backup Service Home, die möglicherweise überschrieben werden könnten. Bitte wählen Sie ein anderes Verzeichnis aus oder löschen Sie zuvor manuell die Sicherungen von dem Sicherungsmedium.", "Verzeichnis auswählen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        BackupFolder.Create();
                    }
                    else if (tcSource.SelectedIndex == 1)
                    {
                        // check ftp credentials
                        try
                        {
                            txtFTPPath.Text = FTPStorage.GetFtpPath(txtFTPPath.Text);

                            var profile = FTPStorage.CheckConnection(txtFTPServer.Text, Convert.ToInt32(txtFTPPort.Text), txtFTPUsername.Text, txtFTPPassword.Text, txtFTPPath.Text, Convert.ToString(cboFtpEncoding.SelectedItem));
                            if (profile == null)
                            {
                                // directory not found
                                MessageBox.Show("Der angegebene Verzeichnispfad wurde nicht gefunden.", "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                iWizardStep -= 1;
                                await ShowWizardStepAsync(iWizardStep);
                                return;
                            }

                            MessageBox.Show("Die Verbindung konnte erfolgreich aufgebaut werden.", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            // ftp credentials exception
                            MessageBox.Show("Die Verbindung konnte nicht aufgebaut werden.\r\n\r\nFTP Server meldete: " + ex.Message.ToString(), "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }
                    }
                    else
                    {
                        txtUNCPath.Text = txtUNCPath.Text.Replace("//", @"\\");
                        try
                        {
                            using (var networkConnection = new NetworkConnection(txtUNCPath.Text, txtUNCUsername.Text, txtUNCPassword.Text))
                            {
                                if (!Directory.Exists(txtUNCPath.Text))
                                {
                                    // network directory credentials exception
                                    MessageBox.Show("Zum Überprüfen des Pfades muss der Ordner, Netzlaufwerk oder Netzwerkfreigabe nun bereit stehen.", "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    iWizardStep -= 1;
                                    await ShowWizardStepAsync(iWizardStep);
                                    return;
                                }
                            }
                        }
                        catch
                        {
                            // credentials exception
                            MessageBox.Show("Zum Überprüfen des Pfades muss der Ordner, Netzlaufwerk oder Netzwerkfreigabe nun bereit stehen.", "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }
                    }

                    // show controls
                    tbControl.SelectedTab = tbStep3;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_3;
                    lblDescription.Text = Resources.WELCOME_STEP_3;
                    cmdBack.Enabled = true;
                    cmdNext.Enabled = true;
                    break;

                case 4:

                    // automatic or manual mode
                    if (!rdFullAutomated.Checked && !rdManualMode.Checked)
                    {
                        MessageBox.Show("Keine Option gewählt.\r\n\r\nUm den Vorgang fortzsetzen muss mindestens eine Option ausgewählt werden. Falls Sie sich unsicher sind, wählen Sie die vollautomatische Datensicherung.", "Option wählen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    // show controls
                    tbControl.SelectedTab = tbProgress;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_4;
                    lblDescription.Text = Resources.WELCOME_STEP_4;
                    cmdBack.Enabled = false;
                    cmdNext.Enabled = false;
                    Application.DoEvents();

                    // set basic configuration
                    {
                        var configurationManager = BackupLogic.GlobalBackup.ConfigurationManager;
                        configurationManager.TaskType = rdFullAutomated.Checked ? TaskType.Auto : TaskType.Manual;

                        if (tcSource.SelectedIndex == 0)
                        {
                            // directory
                            configurationManager.MediumType = 1;
                            configurationManager.BackupFolder = Convert.ToString(lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + @"\" + Environment.UserName;
                            configurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(configurationManager.BackupFolder.Substring(0, 1) + @":\");
                            if (configurationManager.MediaVolumeSerial == null || configurationManager.MediaVolumeSerial == "0")
                            {
                                configurationManager.MediaVolumeSerial = "";
                            }
                        }
                        else if (tcSource.SelectedIndex == 1)
                        {
                            // ftp server
                            configurationManager.MediumType = 3;
                            configurationManager.FtpHost = txtFTPServer.Text;
                            configurationManager.FtpPort = txtFTPPort.Text;
                            configurationManager.FtpUser = txtFTPUsername.Text;
                            configurationManager.FtpPass = txtFTPPassword.Text;
                            configurationManager.FtpFolder = txtFTPPath.Text;
                            configurationManager.FtpCoding = Convert.ToString(cboFtpEncoding.SelectedItem);

                            configurationManager.FtpEncryptionMode = chkFtpEncryption2.Checked ? "0" : "3";
                            configurationManager.FtpSslProtocols = "0";
                        }
                        else
                        {
                            // UNC directory
                            configurationManager.MediumType = 1;
                            configurationManager.BackupFolder = txtUNCPath.Text;
                            configurationManager.MediaVolumeSerial = "";

                            if (txtUNCPath.Text.StartsWith(@"\\"))
                            {
                                BackupLogic.GlobalBackup.ConfigurationManager.UNCUsername = txtUNCUsername.Text;
                                BackupLogic.GlobalBackup.ConfigurationManager.UNCPassword = Crypto.EncryptString(Crypto.ToSecureString(txtUNCPassword.Text), System.Security.Cryptography.DataProtectionScope.LocalMachine);
                            }
                            else
                            {
                                BackupLogic.GlobalBackup.ConfigurationManager.UNCUsername = "";
                                BackupLogic.GlobalBackup.ConfigurationManager.UNCPassword = "";
                            }
                        }

                        var sourceFolders = string.Join("|", lvSourceFolders.Items.Cast<ListViewItem>().Select(x => x.Text));
                        configurationManager.SourceFolder = sourceFolders;
                        configurationManager.Medium = "1";
                        configurationManager.IsConfigured = "1";
                    }

                    if (rdManualMode.Checked)
                    {
                        SuperBase.CurrentTab = frmMain.AvailableTabs.TabConfiguration;
                    }
                    else
                    {
                        // show main window
                        SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;

                        // start backup
                        BackupLogic.StartSystem(true);

                        // create first backup
                        BackupLogic.BackupController.CreateBackupAsync("Erste vollständige Datensicherung", "", false);
                        NotificationController.Current.ShowIconBalloon(5000, "Erste Sicherung läuft...", "Sobald die erste Datensicherung durchgelaufen ist, können Sie diese durch einen Doppelklick hierauf mit dem Backupbrowser ansehen.", ToolTipIcon.Info);
                    }

                    break;

                case 5:

                    // show import steps
                    tbControl.SelectedTab = tbStep5;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_5;
                    lblDescription.Text = Resources.WELCOME_STEP_5;
                    cmdBack.Enabled = false;
                    cmdNext.Enabled = true;

                    if (Convert.ToInt32(tbStep5.Tag) == Convert.ToInt32(false))
                    {
                        // add drives
                        PopulateDrives(lvBackupMedia);
                        tbStep5.Tag = Convert.ToInt32(true);
                    }

                    break;

                case 6:

                    lvBackups.Items.Clear();

                    // determine import
                    if (tcStep5.SelectedIndex == 0)
                    {
                        if (lvBackupMedia.SelectedItems.Count <= 0)
                        {
                            MessageBox.Show("Sie müssen ein Laufwerk auswählen.", "Laufwerk auswählen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        // local or network directory
                        if (Directory.Exists(lvBackupMedia.SelectedItems[0].Tag.ToString() + "Backups"))
                        {
                            // look for sub directories (computer)
                            foreach (var computerDirectory in Directory.GetDirectories(lvBackupMedia.SelectedItems[0].Tag.ToString() + "Backups"))
                            {
                                // look for sub directories (user)
                                foreach (var userDirectory in Directory.GetDirectories(computerDirectory))
                                {
                                    // database file found?
                                    if (!File.Exists(userDirectory + @"\backup.bshdb"))
                                    {
                                        continue;
                                    }

                                    // add directory to list
                                    string computerName = computerDirectory.Substring(computerDirectory.LastIndexOf(@"\") + 1);
                                    try
                                    {
                                        lvBackups.Groups.Add(new ListViewGroup(computerName, "Computer: " + computerName));
                                    }
                                    catch
                                    {
                                        // ignore error
                                    }

                                    // add to list
                                    var newSicherung = lvBackups.Items.Add(userDirectory.Replace(computerDirectory + @"\", ""), 4);
                                    newSicherung.Tag = userDirectory;
                                    newSicherung.Group = lvBackups.Groups[computerName];
                                }
                            }
                        }

                        // show controls
                        lblTitle.Text = Resources.WELCOMETITLE_STEP_6;
                        lblDescription.Text = Resources.WELCOME_STEP_6;
                        tbControl.SelectedTab = tbStep6;
                        cmdBack.Enabled = true;
                        cmdNext.Enabled = true;
                    }
                    else if (tcStep5.SelectedIndex == 2)
                    {
                        tbControl.SelectedTab = tbStep5;
                        cmdBack.Enabled = true;
                        cmdNext.Enabled = true;

                        // local or network directory
                        if (File.Exists(txtPath.Text + @"\backup.bshdb"))
                        {
                            await ShowWizardStepAsync(7);
                        }
                        else
                        {
                            // show controls
                            MessageBox.Show("In dem angegebenen Verzeichnis wurde keine Sicherung gefunden.", "Keine Sicherung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }
                    }
                    else
                    {
                        tbControl.SelectedTab = tbStep5;
                        cmdBack.Enabled = true;
                        cmdNext.Enabled = true;

                        // check ftp server credentials
                        try
                        {
                            txtFTPPath2.Text = FTPStorage.GetFtpPath(txtFTPPath2.Text);

                            using (var storage = new FTPStorage(
                                txtFTPServer2.Text,
                                Convert.ToInt32(txtFTPPort2.Text),
                                txtFTPUser2.Text,
                                txtFTPPass2.Text,
                                txtFTPPath2.Text,
                                Convert.ToString(cboFtpEncoding2.SelectedItem),
                                !chkFtpEncryption2.Checked,
                                0))
                            {
                                storage.Open();
                                if (!storage.FileExists("backup.bshdb"))
                                {
                                    MessageBox.Show("Im angegebenen Verzeichnis wurde keine Sicherung gefunden. Wählen Sie den Ordner aus, der die Backup.bshdb Datei enthält.", "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    iWizardStep -= 1;
                                    await ShowWizardStepAsync(iWizardStep);
                                    return;
                                }
                            }

                            MessageBox.Show("Die Verbindung konnte erfolgreich aufgebaut werden.", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            // ftp credentials wrong
                            MessageBox.Show("Die Verbindung konnte nicht aufgebaut werden.\r\n\r\nFTP Server meldete: " + ex.Message.ToString(), "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        // goto next step
                        await ShowWizardStepAsync(7);
                    }

                    break;

                case 7:

                    tbControl.SelectedTab = tbProgress;
                    cmdBack.Enabled = false;
                    cmdNext.Enabled = false;
                    lblTitle.Text = "Einen Moment bitte...";
                    lblStatus.Text = "Datensicherung wird importiert...";
                    Application.DoEvents();

                    // reset application
                    BackupLogic.GlobalBackup = null;

                    // local or network directory
                    if (tcStep5.SelectedIndex == 0)
                    {
                        // no backup selected?
                        if (lvBackups.SelectedItems.Count <= 0)
                        {
                            MessageBox.Show("Keine Sicherung gewählt.\r\n\r\nUm den Vorgang fortzsetzen müssen Sie die Datensicherung auswählen, die Sie importieren möchten.", "Sicherung wählen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        // copy database from backup device
                        File.Copy(lvBackups.SelectedItems[0].Tag.ToString() + @"\backup.bshdb", BackupLogic.DatabaseFile, true);
                    }
                    else if (tcStep5.SelectedIndex == 1)
                    {
                        // ftp server
                        try
                        {
                            // delete old database file
                            File.Delete(BackupLogic.DatabaseFile);

                            // download backup database
                            using (IStorage storage = new FTPStorage(
                                txtFTPServer2.Text,
                                int.Parse(txtFTPPort2.Text),
                                txtFTPUser2.Text,
                                txtFTPPass2.Text,
                                txtFTPPath2.Text,
                                Convert.ToString(cboFtpEncoding2.SelectedItem),
                                !chkFtpEncryption2.Checked,
                                0))
                            {
                                storage.Open();
                                storage.CopyFileFromStorage(BackupLogic.DatabaseFile, "backup.bshdb");
                            }
                        }
                        catch (Exception ex)
                        {
                            // credentials wrong
                            MessageBox.Show("FTP meldet: " + ex.Message.ToString() + "\r\n\r\nEine Verbindung mit dem FTP-Server konnte nicht aufgebaut werden. Um den Vorgang fortzusetzen muss die Verbindung zum FTP-Server hergestellt werden, um die Konnektivität zu überprüfen.", "FTP-Server nicht erreichbar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            iWizardStep -= 2;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }
                    }
                    else
                    {
                        if (!File.Exists(txtPath.Text + @"\backup.bshdb"))
                        {
                            // database does not exist
                            MessageBox.Show("In dem angegebenen Verzeichnis wurde keine Sicherung gefunden.", "Keine Sicherung", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        // copy database from backup device
                        File.Copy(txtPath.Text + @"\backup.bshdb", BackupLogic.DatabaseFile, true);
                    }

                    // start backup logic
                    await BackupLogic.StartupAsync();

                    // adjust configuration
                    if (tcStep5.SelectedIndex == 0)
                    {
                        // refresh directory
                        BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder = lvBackups.SelectedItems[0].Tag.ToString();
                        BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 1;

                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 1 WHERE fileType = 3");
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 2 WHERE fileType = 4");
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 6 WHERE fileType = 5");
                    }
                    else if (tcStep5.SelectedIndex == 1)
                    {
                        // refresh ftp credentials
                        BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder = "";
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpFolder = txtFTPPath2.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpHost = txtFTPServer2.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpPass = txtFTPPass2.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpPort = txtFTPPort2.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpUser = txtFTPUser2.Text;
                        BackupLogic.GlobalBackup.ConfigurationManager.FtpCoding = Convert.ToString(cboFtpEncoding2.SelectedItem);
                        BackupLogic.GlobalBackup.ConfigurationManager.MediumType = 3;

                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 3 WHERE fileType = 1");
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 4 WHERE fileType = 2");
                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE fileversiontable SET fileType = 5 WHERE fileType = 6");
                    }
                    else
                    {
                        // refresh directory
                        BackupLogic.GlobalBackup.ConfigurationManager.BackupFolder = txtPath.Text;
                    }

                    // show controls
                    tbControl.SelectedTab = tbStep7;
                    lblTitle.Text = Resources.WELCOMETITLE_STEP_7;
                    lblDescription.Text = Resources.WELCOME_STEP_7;
                    cmdBack.Enabled = false;
                    cmdNext.Enabled = true;
                    lvSourceDirs.Items.Clear();

                    foreach (string entry in BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder.Split('|'))
                    {
                        var newSource = new ListViewItem
                        {
                            Text = entry
                        };
                        newSource.SubItems.Add(entry);
                        lvSourceDirs.Items.Add(newSource);
                    }

                    break;

                case 8:

                    // store source directories
                    var sourcePaths = string.Join("|", lvSourceDirs.Items.Cast<ListViewItem>().Select(x => x.Text));

                    foreach (var version in BackupLogic.GlobalBackup.QueryManager.GetVersions())
                    {
                        var sources = version.Sources.Split('|').Select(x => new VersionEntry(x, false)).ToList();

                        foreach (ListViewItem entry in lvSourceDirs.Items)
                        {
                            if (entry.Text.Length > 3)
                            {
                                foreach (var source in sources)
                                {
                                    source.Path = source.Path.Replace(entry.SubItems[1].Text, entry.Text);
                                    source.Changed = true;
                                }
                            }
                        }

                        foreach (ListViewItem entry in lvSourceDirs.Items)
                        {
                            if (entry.Text.Length > 3)
                            {
                                foreach (var source in sources)
                                {
                                    if (!source.Changed)
                                    {
                                        source.Path = source.Path.Replace(entry.SubItems[1].Text, entry.Text);
                                    }
                                }
                            }
                        }

                        await BackupLogic.GlobalBackup.ExecuteNonQueryAsync("UPDATE versiontable SET versionSources = \"" + string.Join("|", sources.Select(x => x.Path).ToArray()) + "\" WHERE versionID = " + version.Id);
                    }

                    BackupLogic.GlobalBackup.ConfigurationManager.SourceFolder = sourcePaths;
                    BackupLogic.GlobalBackup.ConfigurationManager.TaskType = TaskType.Manual;

                    // show overview window
                    SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;

                    // start backup system
                    BackupLogic.StartSystem(true);
                    break;
            }
        }

        private class VersionEntry
        {
            public VersionEntry(string path, bool changed)
            {
                Path = path;
                Changed = changed;
            }

            public string Path { get; set; }

            public bool Changed { get; set; }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                dlgFolderBrowser.SelectedPath = tmpFolderStore;

                if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                tmpFolderStore = dlgFolderBrowser.SelectedPath;

                // search for same directory name
                foreach (ListViewItem entry in lvSourceFolders.Items)
                {
                    if (string.Compare(new DirectoryInfo(entry.Text).Name, new DirectoryInfo(dlgFolderBrowser.SelectedPath).Name, true) == 0)
                    {
                        MessageBox.Show("Gleicher Verzeichnisname.\r\n\r\nEin Verzeichnis mit dem selben Namen wird bereits gesichert. " + Program.APP_TITLE + " kann nur ein Verzeichnis mit dem selben Namen sichern. Bennenen Sie den Ordner um, um diesen zu sichern.", Program.APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                lvSourceFolders.Items.Add(dlgFolderBrowser.SelectedPath, 0);
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (lvSourceFolders.SelectedItems.Count <= 0)
            {
                return;
            }

            lvSourceFolders.Items.Remove(lvSourceFolders.SelectedItems[0]);
        }

        private void lvSourceFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmdDelete.Enabled = lvSourceFolders.SelectedItems.Count > 0;
        }

        private void cmdFTPCheck_Click(object sender, EventArgs e)
        {
            // check ftp credentials
            try
            {
                var profile = FTPStorage.CheckConnection(txtFTPServer.Text, Convert.ToInt32(txtFTPPort.Text), txtFTPUsername.Text, txtFTPPassword.Text, txtFTPPath.Text, Convert.ToString(cboFtpEncoding.SelectedItem));

                if (profile == null)
                {
                    MessageBox.Show("Die Verbindung mit dem FTP Server war nicht erfolgreich.", "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Die Verbindung konnte erfolgreich aufgebaut werden.", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // credentials wrong
                MessageBox.Show("Die Verbindung konnte nicht aufgebaut werden.\r\n\r\nFTP Server meldete: " + ex.Message.ToString(), "Fehlgeschlagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void cmdImport_Click(object sender, EventArgs e)
        {
            await ShowWizardStepAsync(5);
            iWizardStep = 5;
            cmdImport.Visible = false;
            cmdNext.Visible = true;
            cmdBack.Visible = true;
        }

        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = dlgFolderBrowser.SelectedPath;
                }
            }
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            PopulateDrives(lvBackupDrive);
        }

        private void cmdRefresh2_Click(object sender, EventArgs e)
        {
            PopulateDrives(lvBackupMedia);
        }

        private void PopulateDrives(ListView view)
        {
            // retrieve drives
            var drives = DriveInfo.GetDrives();
            view.Items.Clear();

            foreach (var entry in drives.Where(x => x.IsReady))
            {
                int iImageKey = 2;
                var gGroup = view.Groups[0];
                if (entry.DriveType == DriveType.Fixed)
                {
                    iImageKey = 2;
                    gGroup = view.Groups[0];
                }

                if (entry.DriveType == DriveType.Removable)
                {
                    iImageKey = 3;
                    gGroup = view.Groups[1];
                }

                if (entry.DriveType == DriveType.Network)
                {
                    iImageKey = 1;
                    gGroup = view.Groups[2];
                }

                var newEntry = view.Items.Add(entry.Name + " (" + entry.VolumeLabel + ")", iImageKey);
                newEntry.Group = gGroup;
                newEntry.Tag = entry.RootDirectory.FullName;
            }
        }

        private void cmdChange_Click(object sender, EventArgs e)
        {
            if (lvSourceDirs.SelectedItems.Count <= 0)
            {
                return;
            }

            // change source directory
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // check for same name
                if ((new DirectoryInfo(dlgFolderBrowser.SelectedPath).Name ?? "") == (new DirectoryInfo(lvSourceDirs.SelectedItems[0].SubItems[1].Text).Name ?? ""))
                {
                    lvSourceDirs.SelectedItems[0].Text = dlgFolderBrowser.SelectedPath;
                }
                else if (dlgFolderBrowser.SelectedPath.Length == 3 && lvSourceDirs.SelectedItems[0].SubItems[1].Text.Length == 3)
                {
                    lvSourceDirs.SelectedItems[0].Text = dlgFolderBrowser.SelectedPath;
                }
                else
                {
                    MessageBox.Show("Der Speicherort dieses Verzeichnisses kann nicht verändert werden, da der Ordnername identisch sein muss.", "Problem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private async void cmdConfigure_Click(object sender, EventArgs e)
        {
            await ShowWizardStepAsync(1);
            iWizardStep = 1;
            cmdNext.Visible = true;
            cmdBack.Visible = true;
        }
    }
}