// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.IO;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Storage;

/// <summary>
/// Shared helpers for remote (device-side) file paths and
/// type-dispatched copy operations.
/// </summary>
public static class StoragePath
{
    /// <summary>
    /// Builds the remote path of a backed-up file version.
    /// </summary>
    public static string BuildRemoteFilePath(string versionDate, string filePath, string fileName, string longFileName)
    {
        if (!string.IsNullOrEmpty(longFileName))
        {
            return versionDate + "\\_LONGFILES_\\" + longFileName;
        }

        return versionDate + filePath + fileName;
    }

    /// <summary>
    /// Builds the remote path of a backed-up file version from a database reader.
    /// </summary>
    public static string BuildRemoteFilePath(IDataReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        return BuildRemoteFilePath(
            reader.GetString("versionDate"),
            reader.GetString("filePath"),
            reader.GetString("fileName"),
            reader.GetString("longfilename"));
    }

    /// <summary>
    /// Builds the version-qualified long-file storage path used for
    /// local-storage kinds (1/2/6).
    /// </summary>
    public static string BuildLongFilePath(string versionDate, string longFileName)
    {
        return Path.Combine(versionDate, "_LONGFILES_", longFileName);
    }

    /// <summary>
    /// Copies a single file from the backup device to the local file system,
    /// dispatching on the file type. Unknown types are a no-op.
    /// </summary>
    public static void CopyFromStorageByType(IStorageProvider storage, FileTypeKind kind, string localFilePath, string remoteFilePath, string password)
    {
        ArgumentNullException.ThrowIfNull(storage);

        switch (kind)
        {
            case FileTypeKind.RegularCopy:
            case FileTypeKind.StoredCopy:
                storage.CopyFileFromStorage(localFilePath, remoteFilePath);
                break;
            case FileTypeKind.Compressed:
            case FileTypeKind.StoredCompressed:
                storage.CopyFileFromStorageCompressed(localFilePath, remoteFilePath);
                break;
            case FileTypeKind.StoredEncrypted:
            case FileTypeKind.Encrypted:
                storage.CopyFileFromStorageEncrypted(localFilePath, remoteFilePath, password);
                break;
        }
    }

    /// <summary>
    /// Deletes a single file from the backup device, dispatching on the file type.
    /// Unknown types are a no-op.
    /// </summary>
    public static void DeleteFromStorageByType(IStorageProvider storage, FileTypeKind kind, string remoteFile)
    {
        ArgumentNullException.ThrowIfNull(storage);

        switch (kind)
        {
            case FileTypeKind.RegularCopy:
            case FileTypeKind.StoredCopy:
                storage.DeleteFileFromStorage(remoteFile);
                break;
            case FileTypeKind.Compressed:
            case FileTypeKind.StoredCompressed:
                storage.DeleteFileFromStorageCompressed(remoteFile);
                break;
            case FileTypeKind.StoredEncrypted:
            case FileTypeKind.Encrypted:
                storage.DeleteFileFromStorageEncrypted(remoteFile);
                break;
        }
    }
}
