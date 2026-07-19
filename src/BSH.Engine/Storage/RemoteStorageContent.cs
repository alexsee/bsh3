// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Compression;
using Brightbits.BSH.Engine.Security;

namespace Brightbits.BSH.Engine.Storage;

/// <summary>
/// Shared compress / encrypt orchestration for remote storage providers.
/// </summary>
public static class RemoteStorageContent
{
    public static bool CopyCompressed(
        string localFile,
        string remoteFile,
        Func<string, string, bool> copyFileToStorage)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".zip";

        try
        {
            using (var zipFile = ZipFile.Open(tmpFile, ZipArchiveMode.Create))
            {
                zipFile.CreateEntryFromFile(localFile, Path.GetFileName(localFile), CompressionLevel.Optimal);
            }

            return copyFileToStorage(tmpFile, remoteFile + ".zip");
        }
        finally
        {
            DeleteIfExists(tmpFile);
        }
    }

    public static bool CopyEncrypted(
        string localFile,
        string remoteFile,
        string password,
        Func<string, string, bool> copyFileToStorage)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";

        try
        {
            var crypto = new Encryption();
            if (!crypto.Encode(localFile, tmpFile, password))
            {
                return false;
            }

            var file = new FileInfo(tmpFile);
            if (!file.Exists || file.Length == 0)
            {
                return false;
            }

            return copyFileToStorage(tmpFile, remoteFile + ".enc");
        }
        finally
        {
            DeleteIfExists(tmpFile);
        }
    }

    public static bool CopyFromCompressed(
        string localFile,
        string remoteFile,
        Func<string, string, bool> copyFileFromStorage)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".zip";

        try
        {
            if (!copyFileFromStorage(tmpFile, remoteFile + ".zip"))
            {
                return false;
            }

            EnsureParentDirectory(localFile);

            using var zipFile = ZipFile.OpenRead(tmpFile);
            var entry = zipFile.GetEntry(Path.GetFileName(localFile));
            if (entry == null)
            {
                return false;
            }

            entry.ExtractToFile(localFile, overwrite: true);
            return true;
        }
        finally
        {
            DeleteIfExists(tmpFile);
        }
    }

    public static bool CopyFromEncrypted(
        string localFile,
        string remoteFile,
        string password,
        Func<string, string, bool> copyFileFromStorage)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";

        try
        {
            if (!copyFileFromStorage(tmpFile, remoteFile + ".enc"))
            {
                return false;
            }

            EnsureParentDirectory(localFile);

            var crypto = new Encryption();
            return crypto.Decode(tmpFile, localFile, password);
        }
        finally
        {
            DeleteIfExists(tmpFile);
        }
    }

    public static bool DecryptOnStorage(
        string remoteFile,
        string password,
        Func<string, string, bool> copyFileFromStorage,
        Func<string, string, bool> copyFileToStorage,
        Func<string, bool> deleteEncrypted)
    {
        var tmpEncrypted = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";
        var tmpDecrypted = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            if (!copyFileFromStorage(tmpEncrypted, remoteFile + ".enc"))
            {
                return false;
            }

            var crypto = new Encryption();
            if (!crypto.Decode(tmpEncrypted, tmpDecrypted, password))
            {
                return false;
            }

            if (!copyFileToStorage(tmpDecrypted, remoteFile))
            {
                return false;
            }

            return deleteEncrypted(remoteFile);
        }
        finally
        {
            DeleteIfExists(tmpEncrypted);
            DeleteIfExists(tmpDecrypted);
        }
    }

    public static void WriteVersionFile(int versionId, Func<string, string, bool> uploadFile)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bshv");
        File.WriteAllText(tmpFile, versionId.ToString());

        try
        {
            uploadFile(tmpFile, "backup.bshv");
        }
        finally
        {
            DeleteIfExists(tmpFile);
        }
    }

    private static void EnsureParentDirectory(string localFile)
    {
        var directory = Path.GetDirectoryName(localFile);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
