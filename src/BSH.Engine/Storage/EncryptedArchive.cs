// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Compression;
using Brightbits.BSH.Engine.Providers.Ports;
using Serilog;

namespace Brightbits.BSH.Engine.Storage;

/// <summary>Composes ZIP compression with the storage provider's existing encryption format.</summary>
internal static class EncryptedArchive
{
    public static bool CopyToStorage(IStorageProvider storage, string localFile, string remoteFile, string password)
    {
        var temporaryArchive = CreateTemporaryArchivePath();
        try
        {
            using (var archive = ZipFile.Open(temporaryArchive, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(localFile, Path.GetFileName(localFile), CompressionLevel.Optimal);
            }

            return storage.CopyFileToStorageEncrypted(temporaryArchive, remoteFile + ".zip", password);
        }
        finally
        {
            DeleteTemporaryArchive(temporaryArchive);
        }
    }

    public static bool CopyFromStorage(IStorageProvider storage, string localFile, string remoteFile, string password)
    {
        var temporaryArchive = CreateTemporaryArchivePath();
        try
        {
            if (!storage.CopyFileFromStorageEncrypted(temporaryArchive, remoteFile + ".zip", password))
            {
                return false;
            }

            using var archive = ZipFile.OpenRead(temporaryArchive);
            if (archive.Entries.Count != 1)
            {
                throw new InvalidDataException("An encrypted file archive must contain exactly one entry.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(localFile));
            archive.Entries[0].ExtractToFile(localFile, overwrite: true);
            return true;
        }
        finally
        {
            DeleteTemporaryArchive(temporaryArchive);
        }
    }

    private static string CreateTemporaryArchivePath()
    {
        return Path.Combine(Path.GetTempPath(), "bsh-archive-" + Guid.NewGuid().ToString("N") + ".zip");
    }

    private static void DeleteTemporaryArchive(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not delete temporary archive {TemporaryArchive}.", path);
        }
    }
}
