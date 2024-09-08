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
using Serilog;

namespace Brightbits.BSH.Main;

static class ExceptionController
{
    public static void HandleGlobalException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        Log.Error(e.Exception, "An unexpected error occurred {msg}.", e.Exception.Message.ToString() + "\r\n" + e.Exception.StackTrace.ToString());

        using var dlgException = new frmError();
        dlgException.txtError.Text = e.Exception.Message.ToString() + "\r\n" + e.Exception.StackTrace.ToString();

        var DialogRes = dlgException.ShowDialog();
        if (DialogRes == DialogResult.Cancel)
        {
            Application.Exit();
            Environment.Exit(0);
        }
        else if (DialogRes == DialogResult.Retry)
        {
            Application.Restart();
            Environment.Exit(0);
        }
    }

    public static void HandleGlobalException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception)e.ExceptionObject;
        Log.Error(exception, "An unexpected error occurred {msg}.", exception.Message.ToString() + "\r\n" + exception.StackTrace.ToString());

        using var dlgException = new frmError();
        dlgException.txtError.Text = exception.Message.ToString() + "\r\n" + exception.StackTrace.ToString();

        var DialogRes = dlgException.ShowDialog();
        if (DialogRes == DialogResult.Cancel)
        {
            Application.Exit();
            Environment.Exit(0);
        }
        else if (DialogRes == DialogResult.Retry)
        {
            Application.Restart();
            Environment.Exit(0);
        }
    }
}