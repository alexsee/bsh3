// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.InteropServices;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        private IntPtr hwnd;
        public int message;
        private IntPtr wParam;
        private IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }
}
