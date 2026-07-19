// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

namespace Brightbits.BSH.Engine.Types;

public class FileTableRow
{
    public string VersionId
    {
        get; set;
    }

    public string FileId
    {
        get; set;
    }

    public string FileName
    {
        get; set;
    }

    public string FilePath
    {
        get; set;
    }

    public double FileSize
    {
        get; set;
    }

    public DateTime FileDateCreated
    {
        get; set;
    }

    public DateTime FileDateModified
    {
        get; set;
    }

    public string FilePackage
    {
        get; set;
    }

    public string FileType
    {
        get; set;
    }

    public string FileStatus
    {
        get; set;
    }

    public string FileRoot
    {
        get; set;
    }

    public DateTime FileVersionDate
    {
        get; set;
    }

    public string FileLongFileName
    {
        get; set;
    }

    public string VersionStatus
    {
        get; set;
    }

    public string FileNamePath()
    {
        if (string.IsNullOrEmpty(FileRoot))
        {
            return Path.Combine(FilePath, FileName);
        }
        else
        {
            if (string.IsNullOrEmpty(FilePath) || FilePath == "\\")
            {
                return Path.Combine(FileRoot, FileName);
            }
            else
            {
                return Path.Combine(FileRoot, FilePath, FileName);
            }
        }
    }
}
