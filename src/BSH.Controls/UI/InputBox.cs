// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
