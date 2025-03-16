// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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