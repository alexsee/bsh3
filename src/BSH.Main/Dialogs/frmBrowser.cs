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

using Brightbits.BSH.Controls.UI;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Models;
using BSH.Main.Properties;
using BSH.Main.Utils;
using Humanizer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Brightbits.BSH.Engine.Win32Stuff;

namespace Brightbits.BSH.Main;

public partial class frmBrowser : IStatusReport
{
    private readonly ILogger _logger = Log.ForContext<frmBrowser>();

    public frmBrowser()
    {
        InitializeComponent();

        // restore window size
        try
        {
            Size = Settings.Default.BrowserSize;
            SplitContainer1.SplitterDistance = Settings.Default.BrowserSplitter;
            lvFiles.View = (View)Settings.Default.BrowserView;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }
    }

    private VersionDetails selectedVersion;

    private bool bSearch = false;

    private bool isMedium = false;

    private string selectedFolder;

    private ListViewColumnSorter lvwColumnSorter;

    private System.IO.DriveInfo drive;

    /// <summary>
    /// Checks if the version is available on the medium and accessible directly. Then it jumps to the version.
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task ChangeVersionAsync(VersionDetails version)
    {
        try
        {
            // check if medium is readable
            isMedium = await BackupLogic.BackupController.CheckMediaAsync(ActionType.Preview, true);
        }
        catch
        {
            isMedium = false;
        }

        if (!isMedium)
        {
            Text = Resources.DLG_BACKUPBROWSER_TITLE_NOT_AVAILABLE;
        }

        // set version
        selectedVersion = version;
        VersionAlsStabilMarkierenToolStripMenuItem.Checked = version.Stable;

        await ReadFavoritsAsync(true);
    }

    /// <summary>
    /// Checks if the file is directly readable from the storage device.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private bool IsFileAvailable(string fileName)
    {
        if (!isMedium || string.IsNullOrEmpty(fileName) || fileName.StartsWith('\\'))
        {
            return false;
        }

        if (drive != null && (drive.DriveType == System.IO.DriveType.Fixed || drive.DriveType == System.IO.DriveType.Removable))
        {
            return System.IO.File.Exists(fileName);
        }

        return false;
    }

    /// <summary>
    /// Retrieves the list of sub folders for a given folder and adds them to the ListView.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task CreateFolderListAsync(string path)
    {
        var folders = await BackupLogic.QueryManager.GetFolderListAsync(selectedVersion.Id, $"\\{path}\\%");

        // determine the above folder
        if (!string.IsNullOrEmpty(selectedFolder))
        {
            var splitF = selectedFolder.Split('\\');
            if (splitF.Length - 1 <= 0)
            {
                btnBack.Enabled = false;
                btnBack.Image = Resources.arrow_upward_gray_icon_48;
            }
            else
            {
                btnBack.Enabled = true;
                btnBack.Image = Resources.arrow_upward_icon_48;
            }
        }

        var currentFolderIdx = path.Split('\\').Length - 1;

        // show folder
        if (lvFiles.SelectedItems.Count <= 0)
        {
            lblFileName.Text = path.Split('\\')[currentFolderIdx];
            lblFileType.Text = @"\" + path + @"\";
            flpColumn2.Visible = false;
            flpColumn3.Visible = false;
            lblIntegrityCheck.Text = "";
            imgFileType.Image = ilBigFolder.Images[1];
        }

        // retrieve sub folders for this folder
        foreach (var folder in folders)
        {
            var folderPaths = folder.Split('\\');

            if (folderPaths.Length - 1 >= currentFolderIdx + 1)
            {
                // determine sub folder
                var newFolder = new ListViewItem
                {
                    Text = folderPaths[currentFolderIdx + 1]
                };

                if (string.IsNullOrEmpty(newFolder.Text))
                {
                    continue;
                }

                var folderTag = "\\" + string.Join("\\", folderPaths.Take(currentFolderIdx + 2)) + "\\";

                // create dummy item
                newFolder.Tag = folderTag;
                newFolder.Name = newFolder.Text;
                if (!lvFiles.Items.ContainsKey(newFolder.Text))
                {
                    newFolder.ImageKey = "folder";
                    newFolder.SubItems.Add(CreateStringListViewSubItem(""));
                    newFolder.SubItems.Add(CreateStringListViewSubItem("Ordner"));
                    newFolder.Group = lvFiles.Groups["Ordner"];

                    // localize folder
                    newFolder.Text = System.IO.Path.GetFileName(await BackupLogic.QueryManager.GetLocalizedPathAsync(folderTag));
                    if (chkFilesOfThisVersion.Checked && !await BackupLogic.QueryManager.HasChangesOrNewAsync(folderTag, selectedVersion.Id))
                    {
                        newFolder.ForeColor = Color.Gray;
                    }

                    lvFiles.Items.Add(newFolder);
                }
            }
        }
    }

    /// <summary>
    /// Retrieves a list of files of the selected folder and adds them to the ListView.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task CreateFilesListAsync(string path)
    {
        var files = await BackupLogic.QueryManager.GetFilesByVersionAsync(selectedVersion.Id, $"\\{path}\\");

        foreach (var file in files)
        {
            if (chkFilesOfThisVersion.Checked && !file.FilePackage.Equals(selectedVersion.Id))
            {
                continue;
            }

            // create listviewitem
            var fileListItem = new ListViewItem();
            fileListItem.Text = file.FileName;

            // add file attributes
            fileListItem.SubItems.Add(CreateFileSizeListViewSubItem(file.FileSize));
            fileListItem.SubItems.Add(CreateStringListViewSubItem(""));
            fileListItem.SubItems.Add(CreateDateTimeListViewSubItem(file.FileDateModified.ToLocalTime()));
            fileListItem.SubItems.Add(CreateDateTimeListViewSubItem(file.FileDateCreated.ToLocalTime()));
            fileListItem.SubItems.Add(CreateStringListViewSubItem(file.FilePackage + " (" + file.FileVersionDate.ToString("dd.MM.yyyy HH:mm") + ")"));
            fileListItem.ForeColor = (file.FileStatus == "1" ? Color.Black : Color.Red);
            fileListItem.Tag = file.FilePath;

            // get icon image
            try
            {
                GetImageKey(file, fileListItem);
            }
            catch (Exception ex)
            {
                // could not retrieve icon
                this._logger.Warning(ex, "Could not retrieve icon for file: %s", file.FileName);
            }

            // add to group files
            fileListItem.Group = lvFiles.Groups["Dateien"];
            lvFiles.Items.Add(fileListItem);
        }
    }

    private static ListViewItem.ListViewSubItem CreateFileSizeListViewSubItem(double fileSize)
    {
        return new ListViewItem.ListViewSubItem()
        {
            Text = fileSize.Bytes().Humanize(),
            Tag = fileSize
        };
    }

    private static ListViewItem.ListViewSubItem CreateDateTimeListViewSubItem(DateTime dateTime)
    {
        return new ListViewItem.ListViewSubItem()
        {
            Text = dateTime.ToString("dd.MM.yyyy HH:mm"),
            Tag = dateTime
        };
    }

    private static ListViewItem.ListViewSubItem CreateStringListViewSubItem(string str)
    {
        return new ListViewItem.ListViewSubItem()
        {
            Text = str,
            Tag = str
        };
    }

    /// <summary>
    /// Opens the folder by retrieving files and folders of the selected path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task OpenFolderAsync(string path)
    {
        if (path == null)
        {
            return;
        }

        // update UI
        lvFiles.BeginUpdate();
        lvFiles.Items.Clear();

        LCFiles.LoadingCircleControl.Active = true;
        tsslblStatus.Text = Resources.DLG_BACKUPBROWSER_STATUS_LOADING_TEXT;

        // adjust path
        if (path.StartsWith('\\'))
        {
            path = path[1..];
        }

        if (path.EndsWith('\\'))
        {
            path = path[..^1];
        }

        // fill navigation
        var destFolder = await BackupLogic.QueryManager.GetFullRestoreFolderAsync(path, selectedVersion.Id);
        if (destFolder == null)
        {
            // update UI
            lvFiles.EndUpdate();
            lvFiles.Focus();

            LCFiles.LoadingCircleControl.Active = false;
            tsslblStatus.Text = Resources.DLG_BACKUPBROWSER_STATUS_NO_SOURCE_FOLDER_TEXT;

            return;
        }

        UcNav.Path = path;
        UcNav.PathLocalized = await BackupLogic.QueryManager.GetLocalizedPathAsync(path);
        UcNav.CreateNavi(destFolder);

        selectedFolder = path;

        // retrieve folder and files list
        try
        {
            await Task.WhenAll(CreateFolderListAsync(path), CreateFilesListAsync(path));
            tsslblStatus.Text = Resources.DLG_BACKUPBROWSER_MSG_INFO_READY;
        }
        catch (Exception ex)
        {
            this._logger.Error(ex, "Exception during backup browser file collection.");
            tsslblStatus.Text = Resources.DLG_BACKUPBROWSER_MSG_ERROR_ERROR_LOAD;
        }

        // update UI
        lvFiles.EndUpdate();
        lvFiles.Focus();

        LCFiles.LoadingCircleControl.Active = false;

        if (lvFiles.Items.Count > 0)
        {
            lvFiles.FocusedItem = lvFiles.Items[0];
        }

        // select first item
        lvFiles.Select();
        lvFiles_SelectedIndexChanged(null, null);
    }

    /// <summary>
    /// Retrieves the icon of the given file and sets it to the ListViewItem.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="listViewItem"></param>
    private void GetImageKey(FileTableRow file, ListViewItem listViewItem)
    {
        // is file directly accessible?
        if (isMedium && file.FileType == "1")
        {
            // get file path
            var fileName = BackupLogic.QueryManager.GetFileNameFromDrive(file);

            if (IsFileAvailable(fileName))
            {
                var fileInfo = new System.IO.FileInfo(fileName);
                var fiIcon = Icon.ExtractAssociatedIcon(fileInfo.FullName);

                if (fileInfo.Extension.ToUpper() == ".EXE")
                {
                    ilBigIcons.Images.Add(fileInfo.Name, fiIcon);
                    ilSmallIcons.Images.Add(fileInfo.Name, fiIcon);
                }
                else if (!ilBigIcons.Images.ContainsKey(fileInfo.Extension.ToUpper()))
                {
                    ilBigIcons.Images.Add(fileInfo.Extension.ToUpper(), fiIcon);
                    ilSmallIcons.Images.Add(fileInfo.Extension.ToUpper(), fiIcon);
                }

                // determine file details
                var shFi = new SHFILEINFO();
                SHGetFileInfo(fileInfo.FullName, 0U, out shFi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shFi), (uint)SHGFI.SHGFI_TYPENAME);
                listViewItem.SubItems[2].Text = shFi.szTypeName;

                // set icon to ListViewItem
                listViewItem.ImageKey = fileInfo.Extension.ToUpper() == ".EXE" ? fileInfo.Name : fileInfo.Extension.ToUpper();
                return;
            }
        }

        // file not directly accessible, so generate temporary file
        if (ilBigIcons.Images.ContainsKey(System.IO.Path.GetExtension(file.FileName).ToUpper()))
        {
            // we already have retrieved file icon
            listViewItem.ImageKey = System.IO.Path.GetExtension(file.FileName).ToUpper().ToString();
        }
        else
        {
            try
            {
                // determine file details
                var tmpFile = System.IO.Path.GetTempFileName();
                var tmpFilePath = System.IO.Path.GetDirectoryName(tmpFile) + @"\bshicon" + System.IO.Path.GetExtension(file.FileName);

                if (System.IO.File.Exists(tmpFilePath))
                {
                    System.IO.File.Delete(tmpFilePath);
                }

                System.IO.File.Move(tmpFile, tmpFilePath);

                // retrieve file icon
                var fileInfo = new System.IO.FileInfo(tmpFilePath);
                var fiIcon = Icon.ExtractAssociatedIcon(fileInfo.FullName);

                if (fileInfo.Exists)
                {
                    ilBigIcons.Images.Add(fileInfo.Extension.ToUpper(), fiIcon);
                    ilSmallIcons.Images.Add(fileInfo.Extension.ToUpper(), fiIcon);
                }

                // set icon to ListViewItem
                listViewItem.ImageKey = fileInfo.Extension.ToUpper();
                System.IO.File.Delete(tmpFilePath);
            }
            catch
            {
                // ignore error
            }
        }
    }

    private void gbBigList_Click(object sender, EventArgs e)
    {
        lvFiles.View = View.LargeIcon;
    }

    private void gbDetails_Click(object sender, EventArgs e)
    {
        lvFiles.View = View.Details;
    }

    private void frmBrowser_FormClosing(object sender, FormClosingEventArgs e)
    {
        try
        {
            StoreColumnSizes();

            Settings.Default.BrowserSplitter = SplitContainer1.SplitterDistance;
            Settings.Default.BrowserSize = Size;
            Settings.Default.BrowserView = (int)lvFiles.View;
            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }

        StatusController.Current.RemoveObserver(this);
    }

    private KeyValuePair<float, float> GetDPISetting()
    {
        using var myGraphics = CreateGraphics();
        return new KeyValuePair<float, float>(myGraphics.DpiX, myGraphics.DpiY);
    }

    private void SetColumnSizes()
    {
        if (string.IsNullOrEmpty(Settings.Default.BrowserColumnsSize))
        {
            return;
        }

        var columnSizes = Settings.Default.BrowserColumnsSize.Split(';');
        if (columnSizes.Length != lvFiles.Columns.Count)
        {
            return;
        }

        for (var i = 0; i < columnSizes.Length; i++)
        {
            lvFiles.Columns[i].Width = int.Parse(columnSizes[i]);
        }
    }

    private void StoreColumnSizes()
    {
        var columnSizes = string.Join(";", lvFiles.Columns.Cast<ColumnHeader>().Select(x => x.Width.ToString()));
        Settings.Default.BrowserColumnsSize = columnSizes;
    }

    private async void frmBrowser_Load(object sender, EventArgs e)
    {
        // load column sorter
        lvwColumnSorter = new ListViewColumnSorter();
        lvFiles.ListViewItemSorter = lvwColumnSorter;

        try
        {
            SetColumnSizes();
        }
        catch
        {
            // do nothing
        }

        // get dpi setting and adjust the icons
        var dpi = GetDPISetting();
        ilSmallIcons.ImageSize = new Size((int)Math.Round(16d / 96d * (double)dpi.Key), (int)Math.Round(16d / 96d * (double)dpi.Value));
        if (16d / 96d * (double)dpi.Key > 16d)
        {
            lvFiles.SmallImageList = ilBigIcons;
        }
        else
        {
            LCFiles.Height = 33;
        }

        // get current drive
        try
        {
            if (BackupLogic.ConfigurationManager.MediumType == MediaType.LocalDevice
                && BackupLogic.ConfigurationManager.BackupFolder[..1] != "/"
                && !BackupLogic.ConfigurationManager.BackupFolder.StartsWith("\\\\"))
            {
                drive = new System.IO.DriveInfo(BackupLogic.ConfigurationManager.BackupFolder[..1]);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
        }

        // register for backup updates
        StatusController.Current.AddObserver(this);

        // load versions
        ReloadBrowser();

        // retrieve favorites
        await ReadFavoritsAsync();
    }

    private async Task ReadFavoritsAsync(bool versionChanged = false)
    {
        // update UI
        lvFavorite.Items.Clear();
        lvFavorite.BeginUpdate();

        try
        {
            if (selectedVersion == null)
            {
                return;
            }

            string sourceFolders;
            if (string.IsNullOrEmpty(selectedVersion.Sources))
            {
                sourceFolders = BackupLogic.ConfigurationManager.SourceFolder;
            }
            else
            {
                sourceFolders = selectedVersion.Sources;
            }

            // retrieve source folders
            foreach (var e in sourceFolders.Split('|'))
            {
                var entry = e;

                if (entry.EndsWith(@"\"))
                {
                    entry = entry[..^1];
                }

                // Ordnername ermitteln
                var newEntry = new ListViewItem
                {
                    Text = await BackupLogic.QueryManager.GetLocalizedPathAsync(entry[(entry.LastIndexOf(@"\") + 1)..]),
                    ImageIndex = 2,
                    Tag = @"\" + entry[(entry.LastIndexOf(@"\") + 1)..] + @"\"
                };

                lvFavorite.Items.Add(newEntry);
            }

            // refresh favorites
            if (string.IsNullOrEmpty(Settings.Default.BrowserFavoritsName))
            {
                foreach (var e in Settings.Default.BrowserFavorits.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var entry = e;

                    // store favorite name
                    if (entry.EndsWith(@"\"))
                    {
                        entry = entry[..^1];
                    }

                    Settings.Default.BrowserFavoritsName += System.IO.Path.GetFileName(await BackupLogic.QueryManager.GetLocalizedPathAsync(entry));
                }
            }

            // read favorites
            var favorites = Settings.Default.BrowserFavorits.Split('|');

            for (var i = 0; i < favorites.Length; i++)
            {
                var entry = favorites[i];

                if (string.IsNullOrEmpty(entry))
                {
                    continue;
                }

                if (entry.EndsWith(@"\"))
                {
                    entry = entry[..^1];
                }

                // determine folder path
                var newEntry = new ListViewItem
                {
                    Text = Settings.Default.BrowserFavoritsName.Split('|')[i],
                    ImageIndex = 1,
                    Tag = @"\" + entry[(entry.IndexOf(@"\") + 1)..] + @"\",
                    ToolTipText = Resources.DLG_BACKUPBROWSER_TT_FOLDER.FormatWith(BackupLogic.QueryManager.GetLocalizedPathAsync(entry[(entry.LastIndexOf(@"\") + 1)..]), entry[(entry.IndexOf(@"\") + 1)..])
                };

                lvFavorite.Items.Add(newEntry);
            }

            // reload window
            await GetExplorerFoldersAsync(versionChanged);
        }
        catch
        {
            // ignore error
        }

        // set favorites list height
        try
        {
            if (lvFavorite.Items.Count > 10)
            {
                plFavorites.Height = lvFavorite.Top + lvFavorite.Items[0].Bounds.Height * 11;
                lvFavorite.Height = lvFavorite.Items[0].Bounds.Height * 11;
                lvFavorite.Scrollable = true;
            }
            else
            {
                plFavorites.Height = lvFavorite.Top + lvFavorite.Items[0].Bounds.Height * lvFavorite.Items.Count + 5;
                lvFavorite.Height = lvFavorite.Items[0].Bounds.Height * lvFavorite.Items.Count;
            }
        }
        catch
        {
            // ignore error
        }

        lvFavorite.EndUpdate();
    }

    private async Task GetExplorerFoldersAsync(bool VersionChanged = false)
    {
        // determine new windows
        var explorerFolders = GetWindowsExplorerPaths();
        var sourceFolders = BackupLogic.ConfigurationManager.SourceFolder.Split('|');
        var result = new List<ExplorerWindow>();

        // only select explorer windows
        foreach (var entry in explorerFolders)
        {
            // filder source folders
            foreach (var folder in sourceFolders)
            {
                if (!entry.Path.ToLower().Contains(folder.ToLower()))
                {
                    continue;
                }

                try
                {
                    var parent = System.IO.Directory.GetParent(folder).FullName;
                    if (!entry.Path.EndsWith(@"\"))
                    {
                        entry.Path += @"\";
                    }

                    result.Add(new ExplorerWindow(entry.Path.Replace(parent, ""), entry.WindowTitle));
                }
                catch
                {
                    // ignore error
                }
            }
        }

        // add folders to favorites
        foreach (var entry in result)
        {
            var newEntry = new ListViewItem
            {
                ImageIndex = 3,
                Text = entry.WindowTitle + " [Windows Explorer]",
                Tag = entry.Path
            };

            lvFavorite.Items.Add(newEntry);
        }

        // reload selected folder if not already loaded
        if (!string.IsNullOrEmpty(selectedFolder))
        {
            if (VersionChanged)
            {
                await OpenFolderAsync(selectedFolder);
            }
        }
        else if (result.Count == 1)
        {
            // navigate to folder
            lvFavorite.Items[lvFavorite.Items.Count - 1].Selected = true;
            await OpenFolderAsync(lvFavorite.Items[lvFavorite.Items.Count - 1].Tag.ToString());
        }
        else
        {
            // navigate to first folder
            lvFavorite.Items[0].Selected = true;
            await OpenFolderAsync(lvFavorite.Items[0].Tag.ToString());
        }
    }

    private async void lvFiles_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            // quick preview
            if (lvFiles.SelectedItems.Count != 0 && lvFiles.SelectedItems[0].ImageKey != "folder")
            {
                ToolStripSchnellansicht_Click(null, null);
            }
        }
        else if (e.KeyCode == Keys.Enter)
        {
            // open folder
            if (lvFiles.SelectedItems.Count == 1 && lvFiles.SelectedItems[0].ImageKey == "folder")
            {
                await OpenFolderAsync(lvFiles.SelectedItems[0].Tag.ToString());
            }
        }
        else if (e.KeyCode == Keys.Back)
        {
            // previous folder
            btnBack_Click(null, null);
        }
    }

    private async void lvFiles_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (lvFiles.SelectedItems.Count == 0)
        {
            return;
        }

        if (lvFiles.SelectedItems[0].ImageKey == "folder")
        {
            // open folder
            await OpenFolderAsync(lvFiles.SelectedItems[0].Tag.ToString());
        }
        else
        {
            // show file settings
            EigenschaftenToolStripMenuItem_Click(null, null);
        }
    }

    private void lvFiles_SelectedIndexChanged(object sender, EventArgs e)
    {
        // show file details
        try
        {
            lblIntegrityCheck.Text = "";
            flpDetails.Visible = false;
            if (lvFiles.SelectedItems.Count == 0)
            {
                flpColumn2.Visible = false;
                flpColumn3.Visible = false;
                ToolStripSchnellansicht.Enabled = false;
                SchnellansichtToolStripMenuItem.Enabled = false;
                EigenschaftenToolStripMenuItem.Enabled = false;
                SchnellansichtToolStripMenuItem1.Enabled = false;
                EigenschaftenToolStripMenuItem1.Enabled = false;

                // show folder name
                var sSelFolderSplit = selectedFolder.Split('\\');
                lblFileName.Text = sSelFolderSplit[sSelFolderSplit.Length - 1];
                lblFileType.Text = @"\" + selectedFolder + @"\";
                imgFileType.Image = ilBigFolder.Images[1];
                flpDetails.Visible = true;
                return;
            }

            flpColumn2.Visible = true;
            flpColumn3.Visible = true;
            {
                var withBlock = lvFiles.SelectedItems[0];
                lblFileName.Text = withBlock.Text;
                imgFileType.Image = ilBigIcons.Images[withBlock.ImageKey];
                if (withBlock.ImageKey == "folder")
                {
                    lblFileType.Text = withBlock.Tag.ToString();
                    flpColumn2.Visible = false;
                    flpColumn3.Visible = false;
                    flpDetails.Visible = true;
                    return;
                }
                else
                {
                    lblFileType.Text = withBlock.SubItems[2].Text;
                    ToolStripSchnellansicht.Enabled = true;
                    SchnellansichtToolStripMenuItem.Enabled = true;
                    EigenschaftenToolStripMenuItem.Enabled = true;
                    SchnellansichtToolStripMenuItem1.Enabled = true;
                    EigenschaftenToolStripMenuItem1.Enabled = true;
                }

                lblFileSize.Text = withBlock.SubItems[1].Text;
                lblFileLastEdited.Text = withBlock.SubItems[3].Text;
                lblFileCreated.Text = withBlock.SubItems[4].Text;
                lblFileVersion.Text = withBlock.SubItems[5].Text;
                lblIntegrityCheck.Text = withBlock.ToolTipText;
            }
        }
        catch
        {
            // ignore error
        }
        finally
        {
            flpDetails.Visible = true;
        }
    }

    private void lvFavorite_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
        try
        {
            // Itemlabel von | befreien
            var newLabel = e.Label.Replace("|", "");

            // Bearbeiten des Labels abgeschlossen, also speichern
            // Ordner suchen
            var saved = Settings.Default.BrowserFavorits.Split('|');
            var savedName = Settings.Default.BrowserFavoritsName.Split('|');

            Settings.Default.BrowserFavorits = "";
            Settings.Default.BrowserFavoritsName = "";

            for (var i = 0; i <= saved.Length - 1; i++)
            {
                var entry = saved[i];

                // Einträge neuschreiben
                if ((entry ?? "") == (lvFavorite.Items[e.Item].Tag.ToString() ?? ""))
                {
                    // Gesuchte Item gefunden, also verändern
                    Settings.Default.BrowserFavorits += entry + "|";
                    Settings.Default.BrowserFavoritsName += newLabel + "|";
                }
                else
                {
                    // Nicht das gesuchte Item, also normal speichern
                    Settings.Default.BrowserFavorits += entry + "|";
                    Settings.Default.BrowserFavoritsName += savedName[i] + "|";
                }
            }
        }
        catch
        {
            // ignore error
        }
    }

    private void lvFavorite_BeforeLabelEdit(object sender, LabelEditEventArgs e)
    {
        try
        {
            // check if is favorite
            if (!string.IsNullOrEmpty(lvFavorite.Items[e.Item].ImageKey))
            {
                // deny editing
                e.CancelEdit = true;
            }
        }
        catch
        {
            // ignore error
        }
    }

    private async void lvFavorite_Click(object sender, MouseEventArgs e)
    {
        try
        {
            if (e.Button == MouseButtons.Left)
            {
                if (lvFavorite.SelectedItems.Count <= 0)
                {
                    return;
                }

                await OpenFolderAsync(lvFavorite.SelectedItems[0].Tag.ToString());
            }
        }
        catch
        {
            // ignore error
        }
    }

    private async Task AVersionList1_ItemClick(aVersionListItem sender)
    {
        // change version
        await ChangeVersionAsync(sender.Version);

        var backupDate = Convert.ToDateTime(sender.Version.CreationDate, CultureInfo.CreateSpecificCulture("de-DE"));
        lblBackupdate.Text = Resources.BACKUP_FROM_DATE + backupDate.ToString("dd. MMMM yyyy 'um' HH:mm");
    }

    private void mnuGoHome_Click(object sender, EventArgs e)
    {
        lvFavorite.Items[0].Selected = true;
        lvFavorite_Click(sender, null);
    }

    private void GroßeSymboleToolStripMenuItem_Click(object sender, EventArgs e)
    {
        lvFiles.View = View.LargeIcon;
        Settings.Default.BrowserView = (int)lvFiles.View;
    }

    private void ListenansichtToolStripMenuItem_Click(object sender, EventArgs e)
    {
        lvFiles.View = View.List;
        Settings.Default.BrowserView = (int)lvFiles.View;
    }

    private void DetailansichtToolStripMenuItem_Click(object sender, EventArgs e)
    {
        lvFiles.View = View.Details;
        Settings.Default.BrowserView = (int)lvFiles.View;
    }

    private async void cmdRestore_MouseClick(object sender, MouseEventArgs e)
    {
        // set destination folder
        var destinationFolder = "";

        // STRG is hold
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            using (var dlgFolderBrowser = new FolderBrowserDialog())
            {
                if (dlgFolderBrowser.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                destinationFolder = dlgFolderBrowser.SelectedPath;
            }
        }

        // selected folders/files restore
        if (lvFiles.SelectedItems.Count > 0)
        {
            var FilesOrFolders = new List<string>();

            foreach (ListViewItem entry in lvFiles.SelectedItems)
            {
                // folder or file
                if (entry.ImageKey == "folder")
                {
                    FilesOrFolders.Add(entry.Tag.ToString());
                }
                else
                {
                    FilesOrFolders.Add(entry.Tag.ToString() + entry.Text);
                }
            }

            await BackupLogic.BackupController.RestoreBackupAsync(selectedVersion.Id, FilesOrFolders, destinationFolder);
        }
        else
        {
            // restore entire folder
            await BackupLogic.BackupController.RestoreBackupAsync(selectedVersion.Id, @"\" + selectedFolder + @"\", destinationFolder);
        }
    }

    private async void ZuOrdnerfavoritenHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (lvFiles.SelectedItems.Count > 0)
        {
            // directory or file
            if (lvFiles.SelectedItems[0].ImageKey == "folder")
            {
                // add
                Settings.Default.BrowserFavorits += "|" + lvFiles.SelectedItems[0].Tag.ToString();
                Settings.Default.BrowserFavoritsName += "|" + lvFiles.SelectedItems[0].Text;

                // store
                Settings.Default.Save();
            }
        }
        else
        {
            // add
            Settings.Default.BrowserFavorits += "|" + @"\" + selectedFolder + @"\";
            Settings.Default.BrowserFavoritsName += "|" + System.IO.Path.GetFileName(await BackupLogic.QueryManager.GetLocalizedPathAsync(@"\" + selectedFolder + @"\"));

            // store
            Settings.Default.Save();
        }

        // reload favorites
        await ReadFavoritsAsync();
    }

    private void cmnuFavorits_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Prüfen, ob Ordner
        if (lvFavorite.SelectedItems.Count <= 0)
        {
            e.Cancel = true;
            return;
        }

        if (lvFavorite.SelectedItems[0].ImageIndex != 1)
        {
            e.Cancel = true;
        }
    }

    private async void LöschenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // delete folder
        if (lvFavorite.SelectedItems.Count <= 0)
        {
            return;
        }

        if (lvFavorite.SelectedItems[0].ImageIndex == 1)
        {
            // determine folder
            var saved = Settings.Default.BrowserFavorits.Split('|');
            var savedName = Settings.Default.BrowserFavoritsName.Split('|');

            Settings.Default.BrowserFavorits = "";
            Settings.Default.BrowserFavoritsName = "";

            for (var i = 0; i < saved.Length; i++)
            {
                var entry = saved[i];

                // rewrite favorites list
                if ((entry ?? "") != (lvFavorite.SelectedItems[0].Tag.ToString() ?? ""))
                {
                    Settings.Default.BrowserFavorits += entry + "|";
                    Settings.Default.BrowserFavoritsName += savedName[i] + "|";
                }
            }

            Settings.Default.BrowserFavorits = Settings.Default.BrowserFavorits[..^1];
            Settings.Default.BrowserFavoritsName = Settings.Default.BrowserFavoritsName[..^1];

            // save settings
            Settings.Default.Save();
        }

        // reload favorites
        await ReadFavoritsAsync();
    }

    private void WiederherstellenToolStripMenuItem_MouseClick(object sender, EventArgs e)
    {
        // restore folder or files
        cmdRestore_MouseClick(null, new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
    }

    private async void VersionBearbeitenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // edit version
        using (var dlgEditVersion = new frmEditVersion())
        using (var dbClient = BackupLogic.DbClientFactory.CreateDbClient())
        {
            using (var dbRead = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM versiontable WHERE versionID = " + selectedVersion.Id, null))
            {
                if (dbRead.Read())
                {
                    dlgEditVersion.txtTitle.Text = dbRead.GetString("versionTitle");
                    dlgEditVersion.txtDescription.Text = dbRead.GetString("versionDescription");
                }

                dbRead.Close();
            }

            if (dlgEditVersion.ShowDialog(this) == DialogResult.OK)
            {
                await BackupLogic.DbClientFactory.ExecuteNonQueryAsync("UPDATE versiontable SET versionTitle = '" + dlgEditVersion.txtTitle.Text + "', versionDescription = '" + dlgEditVersion.txtDescription.Text + "' WHERE versionID = " + selectedVersion.Id);
                foreach (var entry in AVersionList1.Items)
                {
                    if ((entry.VersionID ?? "") == (selectedVersion.Id ?? ""))
                    {
                        entry.ToolTipTitle = dlgEditVersion.txtTitle.Text;
                        entry.ToolTip = Resources.DLG_BACKUPBROWSER_TT_VERSION_SIMPLE.FormatWith(entry.VersionDate, dlgEditVersion.txtDescription.Text).Trim();
                        break;
                    }
                }
            }
        }
    }

    private async void ToolStripSchnellansicht_Click(object sender, EventArgs e)
    {
        // Datei ausgewählt?
        if (lvFiles.SelectedItems.Count > 0 && lvFiles.SelectedItems[0].ImageKey != "folder")
        {
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
                var password = BackupLogic.BackupService.GetPassword();
                var tmpFile = await BackupLogic.QueryManager.GetFileNameFromDriveAsync(int.Parse(selectedVersion.Id), lvFiles.SelectedItems[0].Text, lvFiles.SelectedItems[0].Tag.ToString(), password);

                var procInfo = new ProcessStartInfo(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"\SmartPreview.exe", " -file:\"" + tmpFile.Item1 + "\"" + (tmpFile.Item2 ? " -c" : ""));
                procInfo.WindowStyle = ProcessWindowStyle.Normal;

                var proc = Process.Start(procInfo);
                proc.WaitForExit();

                if (tmpFile.Item1 != null && tmpFile.Item2)
                {
                    for (var i = 0; i <= 5; i++)
                    {
                        try
                        {
                            if (i == 5)
                            {
                                return;
                            }

                            System.IO.File.Delete(tmpFile.Item1);
                            break;
                        }
                        catch
                        {
                            // ignore error
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
    }

    private void SchnellansichtToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ToolStripSchnellansicht_Click(null, null);
    }

    private void txtSearch_Enter(object sender, EventArgs e)
    {
        if (txtSearch.Tag.ToString() == "search")
        {
            txtSearch.Text = "";
            txtSearch.Font = new Font(txtSearch.Font.FontFamily, 10f, FontStyle.Regular);
            txtSearch.Tag = "";
        }
    }

    private bool searchIsBusy = false;

    private async void txtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;

        // Suche starten
        if (txtSearch.Tag.ToString() == "search")
        {
            return;
        }

        // Nur starten, wenn der Benutzer etwas eingegeben aht
        if (!string.IsNullOrEmpty(txtSearch.Text))
        {
            if (searchIsBusy)
            {
                return;
            }

            searchIsBusy = true;

            // Zuvor alten Prozess abbrechen
            bgrWorkSearch.CancelAsync();
            while (bgrWorkSearch.IsBusy)
            {
                Application.DoEvents();
            }

            lvFiles.Items.Clear();

            // Nun neuen Prozess starten
            bSearch = true;
            bgrWorkSearch.RunWorkerAsync();
            searchIsBusy = false;
        }
        else
        {
            if (searchIsBusy)
            {
                return;
            }

            searchIsBusy = true;

            // Zuvor alten Prozess abbrechen
            bgrWorkSearch.CancelAsync();
            while (bgrWorkSearch.IsBusy)
            {
                Application.DoEvents();
            }

            lvFiles.Items.Clear();
            searchIsBusy = false;
            bSearch = false;
            await OpenFolderAsync(selectedFolder);
        }
    }

    private void txtSearch_Leave(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtSearch.Text))
        {
            return;
        }

        txtSearch.Text = Resources.DLG_BACKUPBROWSER_SEARCH_TEXT;
        txtSearch.Font = new Font(txtSearch.Font.FontFamily, 10f, FontStyle.Italic);
        txtSearch.Tag = "search";
    }

    private async void txtSearch_TextChanged(object sender, EventArgs e)
    {
        if (txtSearch.Tag.ToString() == "search" || !string.IsNullOrEmpty(txtSearch.Text))
        {
            return;
        }

        // Zuvor alten Prozess abbrechen
        bgrWorkSearch.CancelAsync();
        while (bgrWorkSearch.IsBusy)
        {
            Application.DoEvents();
        }

        lvFiles.Items.Clear();
        bSearch = false;
        await OpenFolderAsync(selectedFolder);
    }

    private delegate void ThreadSafe_UcNav_Callback();

    private void ThreadSafe_UcNav()
    {
        AVersionList1.SelectItem(selectedVersion.Id, false);
        UcNav.Path = Resources.DLG_BACKUPBROWSER_SEARCH_RESULTS_TEXT.FormatWith(txtSearch.Text);
        UcNav.PathLocalized = Resources.DLG_BACKUPBROWSER_SEARCH_RESULTS_TEXT.FormatWith(txtSearch.Text);
        UcNav.CreateNavi("", true);
        lblFileName.Text = Resources.DLG_BACKUPBROWSER_SEARCH_RESULT_TEXT;
        lblFileType.Text = "";
        lblIntegrityCheck.Text = "";
        flpColumn2.Visible = false;
        flpColumn3.Visible = false;
        ToolStripSchnellansicht.Enabled = false;
        SchnellansichtToolStripMenuItem.Enabled = false;
        EigenschaftenToolStripMenuItem.Enabled = false;
        SchnellansichtToolStripMenuItem1.Enabled = false;
        EigenschaftenToolStripMenuItem1.Enabled = false;
        WiederherstellenToolStripMenuItem1.Enabled = false;
    }

    private async void bgrWorkSearch_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

        // update UI
        LCFiles.LoadingCircleControl.Active = true;
        bSearch = true;

        // select new version
        Invoke(new ThreadSafe_UcNav_Callback(ThreadSafe_UcNav));

        // retrieve folder list from database
        using (var dbClient = BackupLogic.DbClientFactory.CreateDbClient())
        {
            var searchParameters = new[] {
                dbClient.CreateParameter("filePath", DbType.String, default, "%" + txtSearch.Text + "%"),
                dbClient.CreateParameter("fileName", DbType.String, default, "%" + txtSearch.Text + "%"),
                dbClient.CreateParameter("versionID", DbType.String, default, long.Parse(selectedVersion.Id))
            };

            using (var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT filelink.versionID, filetable.fileID, filetable.fileName, filetable.filePath, fileversiontable.fileSize, fileversiontable.fileDateCreated, fileversiontable.fileDateModified, " + "fileversiontable.filePackage, fileversiontable.fileType, fileversiontable.fileStatus, versionDate FROM filetable, fileversiontable, filelink, versiontable WHERE (filetable.filePath LIKE @filePath OR filetable.fileName LIKE @fileName) AND filelink.versionID = @versionID AND filelink.fileversionID = fileversiontable.fileversionID AND filetable.fileID = fileversiontable.fileID AND versiontable.versionID = fileversiontable.filePackage LIMIT 300", searchParameters))
            {
                var resultList = new List<ListViewItem>();

                // fill folder list
                while (reader.Read() && !bgrWorkSearch.CancellationPending)
                {
                    var file = FileTableRow.FromReaderFile(reader);

                    // add file
                    var newEntry = new ListViewItem();
                    newEntry.Text = file.FileName;

                    // populate file attributes
                    newEntry.SubItems.Add(CreateFileSizeListViewSubItem(file.FileSize));
                    newEntry.SubItems.Add(CreateStringListViewSubItem(""));
                    newEntry.SubItems.Add(CreateDateTimeListViewSubItem(file.FileDateModified.ToLocalTime()));
                    newEntry.SubItems.Add(CreateDateTimeListViewSubItem(file.FileDateCreated.ToLocalTime()));
                    newEntry.SubItems.Add(CreateStringListViewSubItem(file.FilePackage + " (" + file.FileVersionDate.ToString("dd.MM.yyyy HH:mm") + ")"));
                    newEntry.Tag = file.FilePath;

                    // retrieve file icon
                    GetImageKey(file, newEntry);

                    newEntry.Group = lvFiles.Groups["Dateien"];
                    resultList.Add(newEntry);
                }

                e.Result = resultList;

                reader.Close();
            }
        }
    }

    private void bgrWorkSearch_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
    {
        // update UI
        lvFiles.BeginUpdate();

        // fill list
        foreach (var entry in (List<ListViewItem>)e.Result)
        {
            lvFiles.Items.Add(entry);
        }

        lvFiles.EndUpdate();
        ThreadSafe_UcNav();
        LCFiles.LoadingCircleControl.Active = false;
    }

    private async Task UcNav_ItemClick(string sPath)
    {
        await OpenFolderAsync(sPath);
    }

    private async void VersionLöschenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(Resources.DLG_BACKUPBROWSER_MSG_WARN_DELETE_BACKUP_TEXT, Resources.DLG_BACKUPBROWSER_MSG_WARN_DELETE_BACKUP_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
        {
            return;
        }

        // delete version
        Enabled = false;

        await BackupLogic.BackupController.DeleteBackupAsync(selectedVersion.Id, true);

        Enabled = true;
        ReloadBrowser();
    }

    private void ReloadBrowser()
    {
        // retrieve versions
        var lstVersions = BackupLogic.QueryManager.GetVersions(true);
        if (lstVersions.Count <= 0)
        {
            Close();
            NotificationController.Current.ShowIconBalloon(1000, Resources.INFO_NO_BACKUP_AVAILABLE_TITLE, Resources.INFO_NO_BACKUP_AVAILABLE_TEXT, ToolTipIcon.Info);
            return;
        }

        AVersionList1.Items.Clear();
        AVersionList1.DrawItems();
        foreach (var entry in lstVersions)
        {
            var newEntry = new aVersionListItem
            {
                VersionDate = entry.CreationDate.HumanizeDate(),
                VersionID = entry.Id,
                Version = entry,
                ToolTipTitle = entry.Title
            };
            newEntry.ToolTip = Resources.DLG_BACKUPBROWSER_TT_VERSION.FormatWith(entry.CreationDate, entry.Size.Bytes().Humanize(), entry.Description).Trim();
            newEntry.VersionStable = entry.Stable;
            AVersionList1.Items.Add(newEntry);
        }

        // switch to current version
        AVersionList1.DrawItems();
        AVersionList1.SelectItem(lstVersions[0].Id);

        Enabled = true;
        Focus();
    }

    private async void VersionAlsStabilMarkierenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await BackupLogic.BackupService.SetStableAsync(selectedVersion.Id, !VersionAlsStabilMarkierenToolStripMenuItem.Checked);
        VersionAlsStabilMarkierenToolStripMenuItem.Checked = !VersionAlsStabilMarkierenToolStripMenuItem.Checked;

        foreach (var entry in AVersionList1.Items)
        {
            if ((entry.VersionID ?? "") == (selectedVersion.Id ?? ""))
            {
                entry.VersionStable = VersionAlsStabilMarkierenToolStripMenuItem.Checked;
                return;
            }
        }
    }

    private async void cmdTakeMeBack_Click(object sender, EventArgs e)
    {
        if (bSearch)
        {
            if (searchIsBusy)
            {
                return;
            }

            // Zuvor alten Prozess abbrechen
            bgrWorkSearch.CancelAsync();
            while (bgrWorkSearch.IsBusy)
            {
                Application.DoEvents();
            }

            // Version suchen, in der die Datei ist
            var backupVersion = await BackupLogic.QueryManager.GetBackVersionWhereFileAsync(selectedVersion.Id, txtSearch.Text);
            if (backupVersion == null)
            {
                return;
            }

            selectedVersion = await BackupLogic.QueryManager.GetVersionByIdAsync(backupVersion);

            // Alles leeren
            lvFiles.Items.Clear();

            // Nun neuen Prozess starten
            bSearch = true;
            searchIsBusy = true;
            bgrWorkSearch.RunWorkerAsync();
            searchIsBusy = false;
        }
        else
        {
            var backupVersion = await BackupLogic.QueryManager.GetBackVersionWhereFilesInFolderAsync(selectedVersion.Id, selectedFolder);
            if (backupVersion == null)
            {
                return;
            }

            AVersionList1.SelectItem(backupVersion);
        }
    }

    private async void cmdTakeMeLater_Click(object sender, EventArgs e)
    {
        if (bSearch)
        {
            if (searchIsBusy)
            {
                return;
            }

            // Zuvor alten Prozess abbrechen
            bgrWorkSearch.CancelAsync();
            while (bgrWorkSearch.IsBusy)
            {
                Application.DoEvents();
            }

            // Version suchen, in der die Datei ist
            var backupVersion = await BackupLogic.QueryManager.GetNextVersionWhereFileAsync(selectedVersion.Id, txtSearch.Text);
            if (backupVersion == null)
            {
                return;
            }

            selectedVersion = await BackupLogic.QueryManager.GetVersionByIdAsync(backupVersion);
            lvFiles.Items.Clear();

            // Nun neuen Prozess starten
            bSearch = true;
            searchIsBusy = true;
            bgrWorkSearch.RunWorkerAsync();
            searchIsBusy = false;
        }
        else
        {
            var backupVersion = await BackupLogic.QueryManager.GetNextVersionWhereFilesInFolderAsync(selectedVersion.Id, selectedFolder);
            if (backupVersion == null)
            {
                return;
            }

            AVersionList1.SelectItem(backupVersion);
        }
    }

    private async void chkFilesOfThisVersion_CheckedChanged(object sender, EventArgs e)
    {
        await OpenFolderAsync(selectedFolder);
    }

    private async void EigenschaftenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (lvFiles.SelectedItems.Count <= 0)
        {
            return;
        }

        if (lvFiles.SelectedItems[0].ImageKey == "folder")
        {
            return;
        }

        // Eigenschaften anzeigen
        using (var dlgFileProperties = new frmFileProperties())
        {
            dlgFileProperties.BrowserWindow = this;
            dlgFileProperties.lblFileName.Text = lvFiles.SelectedItems[0].Text;
            dlgFileProperties.lblFilePath.Text = await BackupLogic.QueryManager.GetFullRestoreFolderAsync(lvFiles.SelectedItems[0].Tag.ToString(), selectedVersion.Id);
            dlgFileProperties.toolTipCtl.SetToolTip(dlgFileProperties.lblFilePath, dlgFileProperties.lblFilePath.Text);
            dlgFileProperties.lblFileType.Text = lvFiles.SelectedItems[0].SubItems[2].Text;
            dlgFileProperties.lblFileSize.Text = lvFiles.SelectedItems[0].SubItems[1].Text;
            dlgFileProperties.lblFileCreated.Text = lvFiles.SelectedItems[0].SubItems[4].Text;
            dlgFileProperties.lblFileModified.Text = lvFiles.SelectedItems[0].SubItems[3].Text;
            dlgFileProperties.CurrentFileFolder = lvFiles.SelectedItems[0].Tag.ToString();
            dlgFileProperties.PictureBox1.Image = imgFileType.Image;

            var versions = await BackupLogic.QueryManager.GetVersionsByFileAsync(lvFiles.SelectedItems[0].Text, lvFiles.SelectedItems[0].Tag.ToString());
            foreach (var item in versions)
            {
                var newItem = new ListViewItem();
                newItem.Text = item.FilePackage + " (" + item.FileVersionDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm") + ")";
                newItem.Tag = item;
                newItem.SubItems.Add(item.FileDateModified.ToLocalTime().ToString("dd.MM.yyyy HH:mm"));
                dlgFileProperties.lvVersions.Items.Add(newItem);
            }

            dlgFileProperties.ShowDialog(this);
        }
    }

    private void UmbenennenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (lvFavorite.SelectedItems.Count > 0)
            {
                lvFavorite.SelectedItems[0].BeginEdit();
            }
        }
        catch
        {
            // ignore error
        }
    }

    private void WiederherstellenToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        WiederherstellenToolStripMenuItem_MouseClick(sender, e);
    }

    private void EigenschaftenToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        EigenschaftenToolStripMenuItem_Click(sender, e);
    }

    private void SchnellansichtToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        SchnellansichtToolStripMenuItem_Click(sender, e);
    }

    private async void MehrereVersionenLöschenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Enabled = false;

        using (var dlgMultiVersionDelete = new frmMultiVersionDeletion())
        {
            foreach (var Version in BackupLogic.QueryManager.GetVersions())
            {
                var newItem = new ListViewItem();
                newItem.Text = Version.CreationDate.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                newItem.Tag = Version.Id;
                dlgMultiVersionDelete.lstVersions.Items.Add(newItem);
            }

            if (dlgMultiVersionDelete.ShowDialog() == DialogResult.OK)
            {
                if (MessageBox.Show(Resources.DLG_BACKUPBROWSER_MSG_WARN_DELETE_BACKUP_TEXT, Resources.DLG_BACKUPBROWSER_MSG_WARN_DELETE_BACKUP_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                var toDeleteItems = new List<string>();
                foreach (ListViewItem item in dlgMultiVersionDelete.lstVersions.Items)
                {
                    if (item.Checked)
                    {
                        toDeleteItems.Add(item.Tag.ToString());
                    }
                }

                await BackupLogic.BackupController.DeleteBackupsAsync(toDeleteItems, true);
            }
        }

        // load versions
        var lstVersions = BackupLogic.QueryManager.GetVersions(true);
        if (lstVersions.Count <= 0)
        {
            Close();
            NotificationController.Current.ShowIconBalloon(1000, Resources.INFO_NO_BACKUP_AVAILABLE_TITLE, Resources.INFO_NO_BACKUP_AVAILABLE_TEXT, ToolTipIcon.Info);

            return;
        }

        AVersionList1.Items.Clear();
        AVersionList1.DrawItems();

        foreach (var entry in lstVersions)
        {
            var newEntry = new aVersionListItem
            {
                VersionDate = entry.CreationDate.HumanizeDate(),
                VersionID = entry.Id,
                Version = entry,
                ToolTipTitle = entry.Title
            };
            newEntry.ToolTip = Resources.DLG_BACKUPBROWSER_TT_VERSION.FormatWith(entry.CreationDate, entry.Size.Bytes().Humanize(), entry.Description).Trim();
            newEntry.VersionStable = entry.Stable;
            AVersionList1.Items.Add(newEntry);
        }

        // change to current version
        AVersionList1.DrawItems();
        AVersionList1.SelectItem(lstVersions[0].Id);
        Enabled = true;
    }

    private void lvFavorite_SizeChanged(object sender, EventArgs e)
    {
        colName.Width = lvFavorite.Width - 25;
    }

    private async void btnBack_Click(object sender, EventArgs e)
    {
        var splitF = selectedFolder.Split('\\');
        var temp = "";
        if (splitF.Length - 1 <= 0)
        {
            return;
        }

        for (int i = 0, loopTo = splitF.Length - 2; i <= loopTo; i++)
        {
            temp += splitF[i] + @"\";
        }

        temp = temp[..^1];
        await OpenFolderAsync(temp);
    }

    private async void DateiOrdnerAusSicherungenLöschenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (lvFiles.SelectedItems.Count <= 0 || lvFiles.SelectedItems.Count >= 2)
        {
            return;
        }

        var selected = lvFiles.SelectedItems[0];

        // file or folder
        if (selected.ImageKey == "folder")
        {
            await BackupLogic.BackupController.DeleteSingleFileAsync("", selected.Tag.ToString() + "%");
        }
        else
        {
            await BackupLogic.BackupController.DeleteSingleFileAsync(selected.Text, selected.Tag.ToString());
        }

        await OpenFolderAsync(selectedFolder);
    }

    private void cmnuListRight_Opening(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Einzelne Datei / Ordner Löschen klappt nur für ein Eintrag
        DateiOrdnerAusSicherungenLöschenToolStripMenuItem.Enabled = lvFiles.SelectedItems.Count == 1;
    }

    private void lvFiles_ColumnClick(object sender, ColumnClickEventArgs e)
    {
        try
        {
            // Determine if clicked column Is already the column that Is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that Is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these New sort options.
            lvFiles.Sort();
        }
        catch
        {
            // ignore error
        }
    }

    private async void AllesWiederherstellenToolStripMenuItem_Click(object sender, EventArgs e)
    {
        await BackupLogic.BackupController.RestoreBackupAsync(selectedVersion.Id, @"\", "");
    }

    public void ReportAction(ActionType action, bool silent)
    {
        // not used
    }

    public void ReportState(JobState jobState)
    {
        if (!IsHandleCreated)
        {
            return;
        }

        Invoke(new Action(() => { if (jobState == JobState.FINISHED) { try { ReloadBrowser(); } catch { } } }));
    }

    public void ReportStatus(string title, string text)
    {
        // not used
    }

    public void ReportProgress(int total, int current)
    {
        // not used
    }

    public void ReportFileProgress(string file)
    {
        // not used
    }

    public void ReportSystemStatus(SystemStatus systemStatus)
    {
        // not used
    }
}