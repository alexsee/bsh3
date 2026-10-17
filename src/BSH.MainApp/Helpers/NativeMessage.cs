// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.InteropServices;

namespace BSH.MainApp;

internal static class NativeMessage
{
    /// <summary>
    /// Tests replace this so a dialog cannot block the process.
    /// </summary>
    internal static Action<string, string>? ShowOverride;

    public static void Show(string caption, string text)
    {
        try
        {
            if (ShowOverride != null)
            {
                ShowOverride(caption, text);
                return;
            }

            _ = MessageBox(IntPtr.Zero, text, caption, 0x00000030);
        }
        catch (Exception)
        {
            // The process is about to exit. A missing dialog must not crash startup.
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}
