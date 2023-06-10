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

namespace Brightbits.BSH.Main;

public partial class frmPassword
{
    public frmPassword()
    {
        InitializeComponent();
    }

    private void txtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Button1.PerformClick();
        }
    }

    private void txtPassword_TextChanged(object sender, EventArgs e)
    {
        txtPassword.Text = txtPassword.Text.Replace("\"", "");
        txtPassword.Text = txtPassword.Text.Replace("'", "");
    }
}