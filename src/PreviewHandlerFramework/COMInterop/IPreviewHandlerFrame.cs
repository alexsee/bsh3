// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.InteropServices;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerFramework
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("fec87aaf-35f9-447a-adb7-20234491401a")]
    interface IPreviewHandlerFrame
    {
        void GetWindowContext(IntPtr pinfo);
        [PreserveSig]
        uint TranslateAccelerator(ref MSG pmsg);
    };
}
