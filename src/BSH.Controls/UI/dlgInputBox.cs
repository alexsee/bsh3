// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows.Forms;

namespace BSH.Controls.UI
{
    public partial class dlgInputBox : Form
    {
        public dlgInputBox()
        {
            InitializeComponent();
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
