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

using System.Windows.Forms;

namespace BSH.Controls.UI
{
    public static class InputBox
    {
        public static string ShowInputBox(IWin32Window owner, string text, string title, string value)
        {
            using (var dlg = new dlgInputBox())
            {
                dlg.lblTitle.Text = title;
                dlg.Text = title;

                dlg.lblText.Text = text;
                dlg.txtInput.Text = value;

                if (dlg.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                {
                    return dlg.txtInput.Text;
                }
            }

            return null;
        }
    }
}
