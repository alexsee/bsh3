// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;

namespace Brightbits.BSH.Engine.Utils;

public static class CompressionUtils
{
    private static readonly string[] EXCLUDED_FILE_EXTENSIONS =
    {
        ".zip",
        ".lnk",
        ".avi",
        ".cr2",
        ".jpg",
        ".gif",
        ".mov",
        ".mp4",
        ".nef",
        ".png",
        ".odg",
        ".odt",
        ".thm",
        ".docx",
        ".xlsx",
        ".pptx",
        ".7z",
        ".rar",
        ".jar",
        ".war",
        ".tgz",
        ".gz",
    };

    public static bool IsCompressedFile(string fileExt)
    {
        return EXCLUDED_FILE_EXTENSIONS.Contains(fileExt);
    }
}
