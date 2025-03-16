// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Drawing;
using System.Runtime.InteropServices;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerFramework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(left, top, right, bottom);
        }
    }
}
