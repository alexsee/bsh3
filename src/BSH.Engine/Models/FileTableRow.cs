// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.IO;
using Brightbits.BSH.Engine.Database;

namespace Brightbits.BSH.Engine.Models;

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

    public static FileTableRow FromReaderFileVersion(IDataReader reader)
    {
        return new FileTableRow()
        {
            FileId = reader.GetInt32("fileID").ToString(),
            FileName = reader.GetString("fileName"),
            FilePath = reader.GetString("filePath"),
            FileSize = reader.GetDouble("fileSize"),
            FileDateCreated = reader.GetDateTime("fileDateCreated"),
            FileDateModified = reader.GetDateTime("fileDateModified"),
            FilePackage = reader.GetInt32("filePackage").ToString(),
            FileType = reader.GetInt32("fileType").ToString(),
            FileStatus = reader.GetInt32("fileStatus").ToString(),
            FileVersionDate = reader.GetDateTimeParsed("versionDate", "dd-MM-yyyy HH-mm-ss"),
            VersionStatus = reader.GetInt32("versionStatus").ToString()
        };
    }

    public static FileTableRow FromReaderFile(IDataReader reader)
    {
        return new FileTableRow()
        {
            VersionId = reader.GetInt32("versionID").ToString(),
            FileId = reader.GetInt32("fileID").ToString(),
            FileName = reader.GetString("fileName"),
            FilePath = reader.GetString("filePath"),
            FileSize = reader.GetDouble("fileSize"),
            FileDateCreated = reader.GetDateTime("fileDateCreated"),
            FileDateModified = reader.GetDateTime("fileDateModified"),
            FilePackage = reader.GetInt32("filePackage").ToString(),
            FileType = reader.GetInt32("fileType").ToString(),
            FileStatus = reader.GetInt32("fileStatus").ToString(),
            FileVersionDate = reader.GetDateTimeParsed("versionDate", "dd-MM-yyyy HH-mm-ss"),
            FileLongFileName = reader.GetString("longfilename")
        };
    }
}
