// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Runtime.InteropServices;

namespace BSH.MainApp;

internal static class NativeMessage
{
    public static void Show(string caption, string text)
    {
        _ = MessageBox(IntPtr.Zero, text, caption, 0x00000030);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}
