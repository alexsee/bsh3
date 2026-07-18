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
        ShowFatalError(e.Exception);
    }

    public static void HandleGlobalException(object sender, UnhandledExceptionEventArgs e)
    {
        ShowFatalError((Exception)e.ExceptionObject);
    }

    private static void ShowFatalError(Exception exception)
    {
        Log.Error(exception, "An unexpected error occurred {Msg}.", exception.Message + "\r\n" + exception.StackTrace);

        using var dlgException = new frmError();
        dlgException.txtError.Text = exception.Message + "\r\n" + exception.StackTrace;

        var dialogResult = dlgException.ShowDialog();
        if (dialogResult == DialogResult.Cancel)
        {
            Application.Exit();
            Environment.Exit(0);
        }
        else if (dialogResult == DialogResult.Retry)
        {
            Application.Restart();
            Environment.Exit(0);
        }
    }
}
