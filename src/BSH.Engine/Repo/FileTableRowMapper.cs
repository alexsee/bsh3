// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Data;
using Brightbits.BSH.Engine.Repo.Database;
using Brightbits.BSH.Engine.Types;

namespace Brightbits.BSH.Engine.Repo;

public static class FileTableRowMapper
{
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
