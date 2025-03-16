// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Windows.Forms;

namespace SmartPreview
{
    static class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Application.EnableVisualStyles();

                string fileName = "";

                string[] arguments;

                // get file name
                arguments = Environment.GetCommandLineArgs();
                foreach (string entry in arguments)
                {
                    if (entry.StartsWith("-file:"))
                    {
                        fileName = entry.Replace("-file:", "");
                    }
                }

                if (System.IO.File.Exists(fileName))
                {
                    using var dlgSmartPreview = new frmSmartPreview();
                    dlgSmartPreview.ShowPreview(fileName);
                    dlgSmartPreview.ShowDialog();
                }

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unerwartetes Problem aufgetreten:\r\n\r\n" + ex.Message.ToString() + "\r\n" + ex.Source.ToString(), "Unerwartetes Problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}