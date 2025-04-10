﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Humanizer;
using static Brightbits.BSH.Engine.Win32Stuff;

namespace SmartPreview;

public partial class frmSmartPreview
{
    private ElementHost elementHost;
    private System.Windows.Controls.MediaElement mediaPlayer;

    public void InitMediaPlayer()
    {
        elementHost = new ElementHost();
        elementHost.Dock = DockStyle.Fill;
        elementHost.Visible = false;

        plContent.Controls.Add(elementHost);

        mediaPlayer = new System.Windows.Controls.MediaElement();
        mediaPlayer.Focusable = true;
        mediaPlayer.KeyDown += MediaPlayer_KeyDown;
        elementHost.Child = mediaPlayer;
    }

    private void MediaPlayer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Space)
        {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Shift)
            {
                lblMax_Click(sender, null);
            }
            else
            {
                lblClose_Click(sender, null);
            }
        }
    }

    private void lblClose_Click(object sender, EventArgs e)
    {
        phhcMain.Unload();
        phhcMain.Dispose();
        Close();
        Dispose();
    }

    private void frmSmartPreview_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            if (e.Shift)
            {
                lblMax_Click(sender, null);
            }
            else
            {
                lblClose_Click(sender, null);
            }

            e.SuppressKeyPress = false;
        }
    }

    public void lblMax_Click(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Maximized)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
            TopMost = false;
            plContent.BackColor = Color.FromArgb(64, 64, 64);
            Panel1.BackColor = Color.FromArgb(64, 64, 64);
            lblMax.Text = "1";
        }
        else
        {
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            plContent.BackColor = Color.Black;
            Panel1.BackColor = Color.Black;
            lblMax.Text = "2";
        }
    }

    public bool ShowPreview(string fileName, bool showFailed = false)
    {
        try
        {
            var fileExt = System.IO.Path.GetExtension(fileName).ToLower();

            switch (fileExt)
            {
                // pictures
                case ".jpg":
                case ".gif":
                case ".bmp":
                case ".png":
                case ".jpeg":
                case ".wmf":
                    {
                        var previewItem = new PictureBox();
                        previewItem.Image = Image.FromFile(fileName);
                        if (previewItem.Image.Width > Width || previewItem.Height > Height)
                        {
                            previewItem.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                        else
                        {
                            previewItem.SizeMode = PictureBoxSizeMode.CenterImage;
                        }

                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        break;
                    }

                case ".psd":
                    {
                        var previewItem = new PictureBox();
                        var psd = new SimplePsd.CPSD();
                        psd.Load(fileName);
                        previewItem.Image = Image.FromHbitmap(psd.GetHBitmap());
                        previewItem.SizeMode = PictureBoxSizeMode.Zoom;
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        psd = null;
                        break;
                    }

                // documents
                case ".txt":
                case ".rtf":
                case ".ini":
                case ".inf":
                    {
                        var previewItem = new RichTextBox();
                        previewItem.LoadFile(fileName, (fileExt == ".txt" || fileExt == ".ini" || fileExt == ".inf") ? RichTextBoxStreamType.PlainText : RichTextBoxStreamType.RichText); previewItem.ReadOnly = true;
                        previewItem.BackColor = Color.White;
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        break;
                    }

                // websites etc.
                case ".xml":
                case ".xps":
                case ".config":
                case ".psq":
                case ".htm":
                case ".html":
                    {
                        var previewItem = new WebBrowser();
                        previewItem.Navigate(fileName);
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        previewItem.WebBrowserShortcutsEnabled = false;
                        break;
                    }

                // video media
                case ".avi":
                case ".wmv":
                case ".wma":
                case ".mp3":
                case ".mpg":
                case ".mpeg":
                case ".mp4":
                case ".m4v":
                case ".m4a":
                    {
                        elementHost.Visible = true;
                        mediaPlayer.Source = new Uri(fileName);
                        mediaPlayer.Play();
                        break;
                    }

                // source code
                case ".vb":
                    {
                        var previewItem = new WebBrowser();
                        var code = new Manoli.Utils.CSharpFormat.VisualBasicFormat();
                        string rdr = System.IO.File.ReadAllText(fileName);
                        previewItem.DocumentText = "<style>" + My.Resources.Resources.csharp + "</style>\r\n" + code.FormatCode(rdr);
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        previewItem.WebBrowserShortcutsEnabled = false;
                        break;
                    }

                case ".cs":
                    {
                        var previewItem = new WebBrowser();
                        var code = new Manoli.Utils.CSharpFormat.CSharpFormat();
                        string rdr = System.IO.File.ReadAllText(fileName);
                        previewItem.DocumentText = "<style>" + My.Resources.Resources.csharp + "</style>\r\n" + code.FormatCode(rdr);
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        previewItem.WebBrowserShortcutsEnabled = false;
                        break;
                    }

                case ".sql":
                    {
                        var previewItem = new WebBrowser();
                        var code = new Manoli.Utils.CSharpFormat.TsqlFormat();
                        string rdr = System.IO.File.ReadAllText(fileName);
                        previewItem.DocumentText = "<style>" + My.Resources.Resources.csharp + "</style>\r\n" + code.FormatCode(rdr);
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        previewItem.WebBrowserShortcutsEnabled = false;
                        break;
                    }

                case ".js":
                    {
                        var previewItem = new WebBrowser();
                        var code = new Manoli.Utils.CSharpFormat.JavaScriptFormat();
                        string rdr = System.IO.File.ReadAllText(fileName);
                        previewItem.DocumentText = "<style>" + My.Resources.Resources.csharp + "</style>\r\n" + code.FormatCode(rdr);
                        plContent.Controls.Add(previewItem);
                        previewItem.Dock = DockStyle.Fill;
                        previewItem.WebBrowserShortcutsEnabled = false;
                        break;
                    }

                default:
                    {
                        try
                        {
                            phhcMain.FilePath = fileName;
                            phhcMain.Dock = DockStyle.Fill;
                            phhcMain.Visible = true;

                            if (!phhcMain.HasHandler)
                            {
                                plFileDetails.Visible = true;
                                plFileDetails.Dock = DockStyle.Fill;

                                // read file details
                                try
                                {
                                    lblFileName.Text = System.IO.Path.GetFileName(fileName);
                                    picIcon.Image = Icon.ExtractAssociatedIcon(fileName).ToBitmap();

                                    // retrieve file type
                                    var shFi = new SHFILEINFO();
                                    SHGetFileInfo(fileName, 0, out shFi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shFi), (uint)SHGFI.SHGFI_TYPENAME);

                                    lblFileType.Text = shFi.szTypeName;

                                    // retrieve file info
                                    var FileInfo = new System.IO.FileInfo(fileName);
                                    Label TmpLabel;

                                    // creation date
                                    TmpLabel = new Label();
                                    TmpLabel.AutoSize = true;
                                    TmpLabel.Text = "Erstellt am: " + FileInfo.CreationTime.ToString("dd. MMM yyyy 'um' HH:mm");
                                    flpFileDetails.Controls.Add(TmpLabel);
                                    TmpLabel.Dock = DockStyle.Fill;

                                    // last modified date
                                    TmpLabel = new Label();
                                    TmpLabel.AutoSize = true;
                                    TmpLabel.Text = "Geändert am: " + FileInfo.LastWriteTime.ToString("dd. MMM yyyy 'um' HH:mm");
                                    flpFileDetails.Controls.Add(TmpLabel);
                                    TmpLabel.Dock = DockStyle.Fill;

                                    // file size
                                    TmpLabel = new Label();
                                    TmpLabel.AutoSize = true;
                                    TmpLabel.Text = "Größe: " + FileInfo.Length.Bytes().Humanize();
                                    flpFileDetails.Controls.Add(TmpLabel);
                                    TmpLabel.Dock = DockStyle.Fill;
                                }
                                catch
                                {
                                    // ignore error
                                }

                                Height = 480;
                                return default;
                            }
                        }
                        catch
                        {
                            // ignore error
                        }

                        break;
                    }
            }
        }
        catch
        {
            // ignore error
        }

        return default;
    }
}