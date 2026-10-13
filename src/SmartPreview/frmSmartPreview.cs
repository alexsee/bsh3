// Copyright (c) Alexander Seeliger. All Rights Reserved.
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
    private PictureBox imagePreview;
    private ElementHost elementHost;
    private System.Windows.Controls.MediaElement mediaPlayer;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClearPreviewImage();
            ClearImage(picIcon);
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private static void ClearImage(PictureBox pictureBox)
    {
        if (pictureBox == null)
        {
            return;
        }

        var image = pictureBox.Image;
        pictureBox.Image = null;
        image?.Dispose();
    }

    private void ClearPreviewImage()
    {
        ClearImage(imagePreview);
        imagePreview?.Dispose();
        imagePreview = null;
    }

    private static Image LoadPsdImage(string fileName)
    {
        var psd = new SimplePsd.CPSD();
        try
        {
            psd.Load(fileName);
            return Image.FromHbitmap(psd.GetHBitmap());
        }
        finally
        {
            var bitmap = psd.GetHBitmap();
            if (bitmap != IntPtr.Zero)
            {
                SimplePsd.WinInvoke32.DeleteObject(bitmap);
            }
        }
    }

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

    private void ShowSourcePreview(string fileName, string fileExt)
    {
        var previewItem = new WebBrowser();
        Manoli.Utils.CSharpFormat.SourceFormat code = fileExt switch
        {
            ".vb" => new Manoli.Utils.CSharpFormat.VisualBasicFormat(),
            ".cs" => new Manoli.Utils.CSharpFormat.CSharpFormat(),
            ".sql" => new Manoli.Utils.CSharpFormat.TsqlFormat(),
            _ => new Manoli.Utils.CSharpFormat.JavaScriptFormat()
        };
        var source = System.IO.File.ReadAllText(fileName);
        previewItem.DocumentText = "<style>" + My.Resources.Resources.csharp + "</style>\r\n" + code.FormatCode(source);
        plContent.Controls.Add(previewItem);
        previewItem.Dock = DockStyle.Fill;
        previewItem.WebBrowserShortcutsEnabled = false;
    }

    public bool ShowPreview(string fileName)
    {
        try
        {
            phhcMain.Unload();
            ClearPreviewImage();
            ClearImage(picIcon);
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
                        imagePreview = new PictureBox();
                        imagePreview.Image = Image.FromFile(fileName);
                        if (imagePreview.Image.Width > Width || imagePreview.Height > Height)
                        {
                            imagePreview.SizeMode = PictureBoxSizeMode.Zoom;
                        }
                        else
                        {
                            imagePreview.SizeMode = PictureBoxSizeMode.CenterImage;
                        }

                        plContent.Controls.Add(imagePreview);
                        imagePreview.Dock = DockStyle.Fill;
                        break;
                    }

                case ".psd":
                    {
                        imagePreview = new PictureBox();
                        imagePreview.Image = LoadPsdImage(fileName);
                        imagePreview.SizeMode = PictureBoxSizeMode.Zoom;
                        plContent.Controls.Add(imagePreview);
                        imagePreview.Dock = DockStyle.Fill;
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
                case ".cs":
                case ".sql":
                case ".js":
                    {
                        ShowSourcePreview(fileName, fileExt);
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
                                    using var icon = Icon.ExtractAssociatedIcon(fileName);
                                    var iconImage = icon.ToBitmap();
                                    ClearImage(picIcon);
                                    picIcon.Image = iconImage;

                                    // retrieve file type
                                    SHGetFileInfo(fileName, 0, out SHFILEINFO shFi, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SHFILEINFO)), (uint)SHGFI.SHGFI_TYPENAME);

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
            ClearPreviewImage();
        }

        return default;
    }
}