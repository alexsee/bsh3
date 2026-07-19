// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;
using Brightbits.BSH.Engine.Utils;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

public partial class ucDoConfigure : IMainTabs
{
    private ComboBox cboRemoteProtocol;
    private ComboBox cboRemoteProtocolImport;

    public ucDoConfigure()
    {
        InitializeComponent();
        InitializeRemoteProtocolSelectors();
    }

    private string tmpFolderStore = "";

    private void InitializeRemoteProtocolSelectors()
    {
        cboRemoteProtocol = CreateRemoteProtocolComboBox();
        cboRemoteProtocol.Location = new System.Drawing.Point(194, 232);
        cboRemoteProtocol.SelectedIndexChanged += (_, _) => ApplyRemoteProtocolUi(isImport: false);
        TabPage2.Controls.Add(cboRemoteProtocol);

        var protocolLabel = new Label
        {
            AutoSize = true,
            Location = new System.Drawing.Point(24, 236),
            Text = Resources.DLG_REMOTE_PROTOCOL_LABEL
        };
        TabPage2.Controls.Add(protocolLabel);

        cboRemoteProtocolImport = CreateRemoteProtocolComboBox();
        cboRemoteProtocolImport.Location = new System.Drawing.Point(194, 232);
        cboRemoteProtocolImport.SelectedIndexChanged += (_, _) => ApplyRemoteProtocolUi(isImport: true);
        TabPage4.Controls.Add(cboRemoteProtocolImport);

        var protocolLabelImport = new Label
        {
            AutoSize = true,
            Location = new System.Drawing.Point(27, 236),
            Text = Resources.DLG_REMOTE_PROTOCOL_LABEL
        };
        TabPage4.Controls.Add(protocolLabelImport);

        ApplyRemoteProtocolUi(isImport: false);
        ApplyRemoteProtocolUi(isImport: true);
    }

    private static ComboBox CreateRemoteProtocolComboBox()
    {
        var comboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FormattingEnabled = true,
            Size = new System.Drawing.Size(200, 33)
        };
        comboBox.Items.Add(Resources.DLG_REMOTE_PROTOCOL_FTP);
        comboBox.Items.Add(Resources.DLG_REMOTE_PROTOCOL_WEBDAV);
        comboBox.SelectedIndex = 0;
        return comboBox;
    }

    private MediaType GetSelectedRemoteMediaType(bool isImport = false)
    {
        // Protocol combo: 0 = FTP, 1 = WebDAV → media combo indices 1 and 2
        var comboBox = isImport ? cboRemoteProtocolImport : cboRemoteProtocol;
        return MediaTypeExtensions.FromMediaComboIndex(comboBox.SelectedIndex + 1);
    }

    private void ApplyRemoteProtocolUi(bool isImport)
    {
        var mediaType = GetSelectedRemoteMediaType(isImport);
        var useWebDav = mediaType.IsWebDav();
        var portBox = isImport ? txtFTPPort2 : txtFTPPort;
        var encoding = isImport ? cboFtpEncoding2 : cboFtpEncoding;
        var encodingLabel = isImport ? Label19 : Label18;
        var encryption = isImport ? chkFtpEncryption2 : chkFtpEncryption;

        encoding.Visible = !useWebDav;
        encodingLabel.Visible = !useWebDav;
        encryption.Visible = !useWebDav;

        portBox.Text = mediaType.AdjustPortForProtocol(portBox.Text);
    }

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

    public UserControl UserControlInstance => this;

    public string Title => Resources.DLG_UC_DO_CONFIGURE_TITLE;

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
                    MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_NO_SOURCE_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_NO_SOURCE_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_NO_TARGET_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_NO_TARGET_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    // directory is not empty?
                    var BackupFolder = new DirectoryInfo(Convert.ToString(lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + '\\' + Environment.UserName);

                    if (BackupFolder.Exists)
                    {
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_TARGET_NOT_EMPTY_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_TARGET_NOT_EMPTY_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    BackupFolder.Create();
                }
                else if (tcSource.SelectedIndex == 1)
                {
                    // check remote credentials (FTP/WebDAV)
                    try
                    {
                        var mediaType = GetSelectedRemoteMediaType();
                        var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: false);
                        txtFTPPath.Text = credentials.Folder;

                        var isRemoteConnectionValid = StorageFactory.CheckRemoteConnection(mediaType, credentials);

                        if (!isRemoteConnectionValid)
                        {
                            // remote storage unavailable
                            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_DIRECTORY_NOT_FOUND_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_DIRECTORY_NOT_FOUND_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // ftp credentials exception
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        using var networkConnection = new NetworkConnection(txtUNCPath.Text, txtUNCUsername.Text, txtUNCPassword.Text);
                        if (!Directory.Exists(txtUNCPath.Text))
                        {
                            // network directory credentials exception
                            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }
                    }
                    catch
                    {
                        // credentials exception
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NETWORK_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_OPTION_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_OPTION_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    var configurationManager = BackupLogic.ConfigurationManager;
                    configurationManager.TaskType = rdFullAutomated.Checked ? TaskType.Auto : TaskType.Manual;

                    if (tcSource.SelectedIndex == 0)
                    {
                        // directory
                        configurationManager.MediumType = MediaType.LocalDevice;
                        configurationManager.BackupFolder = Convert.ToString(lvBackupDrive.SelectedItems[0].Tag) + @"Backups\" + Environment.MachineName + @"\" + Environment.UserName;
                        configurationManager.MediaVolumeSerial = Win32Stuff.GetVolumeSerial(configurationManager.BackupFolder.Substring(0, 1) + @":\");
                        if (configurationManager.MediaVolumeSerial == null || configurationManager.MediaVolumeSerial == "0")
                        {
                            configurationManager.MediaVolumeSerial = "";
                        }
                    }
                    else if (tcSource.SelectedIndex == 1)
                    {
                        // FTP or WebDAV server
                        var mediaType = GetSelectedRemoteMediaType();
                        var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: false);
                        credentials.ApplyTo(configurationManager, mediaType);
                    }
                    else
                    {
                        // UNC directory
                        configurationManager.MediumType = MediaType.LocalDevice;
                        configurationManager.BackupFolder = txtUNCPath.Text;
                        configurationManager.MediaVolumeSerial = "";

                        if (txtUNCPath.Text.StartsWith(@"\\"))
                        {
                            BackupLogic.ConfigurationManager.UNCUsername = txtUNCUsername.Text;
                            BackupLogic.ConfigurationManager.UNCPassword = Crypto.EncryptString(txtUNCPassword.Text, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                        }
                        else
                        {
                            BackupLogic.ConfigurationManager.UNCUsername = "";
                            BackupLogic.ConfigurationManager.UNCPassword = "";
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
                    await BackupLogic.StartSystemAsync(true);

                    // create first backup (fire-and-forget; balloon notifies the user)
                    _ = BackupLogic.BackupController.CreateBackupAsync(Resources.BACKUP_TITLE_FIRST, "", false);
                    NotificationController.Current.ShowIconBalloon(5000, Resources.INFO_BACKUP_FIRST_RUN_TITLE, Resources.INFO_BACKUP_FIRST_RUN_TEXT, ToolTipIcon.Info);
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
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_DEVICE_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_DEVICE_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                                var computerName = computerDirectory.Substring(computerDirectory.LastIndexOf('\\') + 1);
                                try
                                {
                                    lvBackups.Groups.Add(new ListViewGroup(computerName, Resources.DLG_UC_DO_CONFIGURE_COMPUTER + computerName));
                                }
                                catch
                                {
                                    // ignore error
                                }

                                // add to list
                                var newSicherung = lvBackups.Items.Add(userDirectory.Replace(computerDirectory + '\\', ""), 4);
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
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

                    // check remote server credentials
                    try
                    {
                        var mediaType = GetSelectedRemoteMediaType(isImport: true);
                        var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: true);
                        txtFTPPath2.Text = credentials.Folder;

                        using var storage = StorageFactory.CreateRemote(mediaType, credentials);

                        storage.Open();
                        if (!storage.FileExists("backup.bshdb"))
                        {
                            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            iWizardStep -= 1;
                            await ShowWizardStepAsync(iWizardStep);
                            return;
                        }

                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        // remote credentials wrong
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                lblTitle.Text = Resources.DLG_UC_DO_CONFIGURE_STATUS_IMPORT_WAIT_TEXT;
                lblStatus.Text = Resources.DLG_UC_DO_CONFIGURE_STATUS_IMPORT_TEXT;
                Application.DoEvents();

                // local or network directory
                if (tcStep5.SelectedIndex == 0)
                {
                    // no backup selected?
                    if (lvBackups.SelectedItems.Count <= 0)
                    {
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_NO_BACKUP_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_NO_BACKUP_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    // copy database from backup device
                    DeleteCurrentDatabaseFile();
                    File.Copy(lvBackups.SelectedItems[0].Tag.ToString() + @"\backup.bshdb", BackupLogic.DatabaseFile, true);
                }
                else if (tcStep5.SelectedIndex == 1)
                {
                    // FTP or WebDAV server
                    try
                    {
                        // download backup database
                        DeleteCurrentDatabaseFile();

                        var mediaType = GetSelectedRemoteMediaType(isImport: true);
                        var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: true);
                        txtFTPPath2.Text = credentials.Folder;
                        using IStorageProvider storage = StorageFactory.CreateRemote(mediaType, credentials);
                        storage.Open();
                        storage.CopyFileFromStorage(BackupLogic.DatabaseFile, "backup.bshdb");
                    }
                    catch (Exception ex)
                    {
                        // credentials wrong
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_NO_BACKUP_FOUND_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        iWizardStep -= 1;
                        await ShowWizardStepAsync(iWizardStep);
                        return;
                    }

                    // copy database from backup device
                    DeleteCurrentDatabaseFile();
                    File.Copy(txtPath.Text + @"\backup.bshdb", BackupLogic.DatabaseFile, true);
                }

                // start backup logic
                await BackupLogic.StartupAsync();

                // adjust configuration
                if (tcStep5.SelectedIndex == 0)
                {
                    // refresh directory
                    BackupLogic.ConfigurationManager.BackupFolder = lvBackups.SelectedItems[0].Tag.ToString();
                    BackupLogic.ConfigurationManager.MediumType = MediaType.LocalDevice;

                    await BackupFileType.RemapAllToAsync(BackupLogic.DbClientFactory, MediaType.LocalDevice.ToStorageProviderKind());
                }
                else if (tcStep5.SelectedIndex == 1)
                {
                    // refresh remote credentials
                    var mediaType = GetSelectedRemoteMediaType(isImport: true);
                    var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: true);
                    credentials.ApplyTo(BackupLogic.ConfigurationManager, mediaType);

                    await BackupFileType.RemapAllToAsync(BackupLogic.DbClientFactory, mediaType.ToStorageProviderKind());
                }
                else
                {
                    // refresh directory
                    BackupLogic.ConfigurationManager.BackupFolder = txtPath.Text;
                }

                // show controls
                tbControl.SelectedTab = tbStep7;
                lblTitle.Text = Resources.WELCOMETITLE_STEP_7;
                lblDescription.Text = Resources.WELCOME_STEP_7;
                cmdBack.Enabled = false;
                cmdNext.Enabled = true;
                lvSourceDirs.Items.Clear();

                foreach (var entry in BackupLogic.ConfigurationManager.SourceFolder.Split('|'))
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

                foreach (var version in BackupLogic.QueryManager.GetVersions())
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

                    await BackupLogic.DbClientFactory.ExecuteNonQueryAsync("UPDATE versiontable SET versionSources = \"" + string.Join("|", sources.Select(x => x.Path).ToArray()) + "\" WHERE versionID = " + version.Id);
                }

                BackupLogic.ConfigurationManager.SourceFolder = sourcePaths;
                BackupLogic.ConfigurationManager.TaskType = TaskType.Manual;

                // show overview window
                SuperBase.CurrentTab = frmMain.AvailableTabs.TabOverview;

                // start backup system
                await BackupLogic.StartSystemAsync(true);
                break;
        }
    }

    private static void DeleteCurrentDatabaseFile()
    {
        DbClientFactory.ClosePool();

        // delete old database file
        File.Delete(BackupLogic.DatabaseFile);
        try
        {
            File.Delete(BackupLogic.DatabaseFile + "-wal");
        }
        catch
        {
            // ignore error
        }
        try
        {
            File.Delete(BackupLogic.DatabaseFile + "-shm");
        }
        catch (Exception)
        {
            // ignore error
        }
    }

    private sealed class VersionEntry
    {
        public VersionEntry(string path, bool changed)
        {
            Path = path;
            Changed = changed;
        }

        public string Path
        {
            get; set;
        }

        public bool Changed
        {
            get; set;
        }
    }

    private void cmdAdd_Click(object sender, EventArgs e)
    {
        using var dlgFolderBrowser = new FolderBrowserDialog();
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
                MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_SAME_SOURCE_SELECTED_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_SAME_SOURCE_SELECTED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        lvSourceFolders.Items.Add(dlgFolderBrowser.SelectedPath, 0);
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
        // check remote credentials (FTP/WebDAV)
        try
        {
            var mediaType = GetSelectedRemoteMediaType();
            var credentials = CreateRemoteCredentialsFromUi(mediaType, isImport: false);
            txtFTPPath.Text = credentials.Folder;
            var isRemoteConnectionValid = StorageFactory.CheckRemoteConnection(mediaType, credentials);

            if (!isRemoteConnectionValid)
            {
                MessageBox.Show(Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT, Resources.DLG_UC_CONFIG_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_INFO_FTP_SUCCESS_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // credentials wrong
            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TEXT + ex.Message.ToString(), Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_FTP_UNSUCCESSFUL_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        using var dlgFolderBrowser = new FolderBrowserDialog();
        if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
        {
            txtPath.Text = dlgFolderBrowser.SelectedPath;
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

    private static void PopulateDrives(ListView view)
    {
        // retrieve drives
        var drives = DriveInfo.GetDrives();
        view.Items.Clear();

        foreach (var entry in drives.Where(x => x.IsReady))
        {
            var iImageKey = 1;
            var gGroup = view.Groups[0];
            if (entry.DriveType == DriveType.Fixed)
            {
                iImageKey = 1;
                gGroup = view.Groups[0];
            }

            if (entry.DriveType == DriveType.Removable)
            {
                iImageKey = 2;
                gGroup = view.Groups[1];
            }

            if (entry.DriveType == DriveType.Network)
            {
                iImageKey = 3;
                gGroup = view.Groups[2];
            }

            var newEntry = view.Items.Add(entry.Name + " (" + entry.VolumeLabel + ")", iImageKey);
            newEntry.Group = gGroup;
            newEntry.Tag = entry.RootDirectory.FullName;
        }
    }

    private RemoteStorageCredentials CreateRemoteCredentialsFromUi(MediaType mediaType, bool isImport)
    {
        if (isImport)
        {
            return new RemoteStorageCredentials
            {
                Host = txtFTPServer2.Text,
                Port = int.TryParse(txtFTPPort2.Text, out var importPort) ? importPort : mediaType.DefaultRemotePort(),
                UserName = txtFTPUser2.Text,
                Password = txtFTPPass2.Text,
                Folder = mediaType.IsFtp() ? FtpStorage.GetFtpPath(txtFTPPath2.Text) : txtFTPPath2.Text,
                Encoding = cboFtpEncoding2.SelectedItem?.ToString() ?? "ISO-8859-1",
                // chkFtpEncryption = "force unencrypted connection"
                UseEncryption = !chkFtpEncryption2.Checked
            };
        }

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

    private void cmdChange_Click(object sender, EventArgs e)
    {
        if (lvSourceDirs.SelectedItems.Count <= 0)
        {
            return;
        }

        // change source directory
        using var dlgFolderBrowser = new FolderBrowserDialog();
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
            MessageBox.Show(Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_CANNOT_CHANGE_DIRECTORY_TEXT, Resources.DLG_UC_DO_CONFIGURE_MSG_ERROR_CANNOT_CHANGE_DIRECTORY_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
