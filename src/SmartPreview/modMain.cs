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

using System;
using System.Windows.Forms;

namespace SmartPreview
{
    static class modMain
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                Application.EnableVisualStyles();

                string fileName = "";

                string[] Arguments;

                // get file name
                Arguments = Environment.GetCommandLineArgs();
                foreach (string entry in Arguments)
                {
                    if (entry.Substring(0, 6) == "-file:")
                    {
                        fileName = entry.Replace("-file:", "");
                    }
                }

                if (System.IO.File.Exists(fileName))
                {
                    using (var dlgSmartPreview = new frmSmartPreview())
                    {
                        dlgSmartPreview.ShowPreview(fileName);
                        dlgSmartPreview.ShowDialog();
                    }
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