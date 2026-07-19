// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Utils;

public static class BackupFileType
{
    public const int Local = 1;
    public const int LocalCompressed = 2;
    public const int Ftp = 3;
    public const int FtpCompressed = 4;
    public const int FtpEncrypted = 5;
    public const int LocalEncrypted = 6;
    public const int WebDav = 7;
    public const int WebDavCompressed = 8;
    public const int WebDavEncrypted = 9;

    private static readonly int[] AllRegular = [Local, Ftp, WebDav];
    private static readonly int[] AllCompressed = [LocalCompressed, FtpCompressed, WebDavCompressed];
    private static readonly int[] AllEncrypted = [LocalEncrypted, FtpEncrypted, WebDavEncrypted];

    public static int GetFileType(StorageProviderKind storageKind, bool compress, bool encrypt)
    {
        if (storageKind == StorageProviderKind.LocalFileSystem)
        {
            if (encrypt)
            {
                return LocalEncrypted;
            }

            if (compress)
            {
                return LocalCompressed;
            }

            return Local;
        }

        if (storageKind == StorageProviderKind.WebDav)
        {
            if (encrypt)
            {
                return WebDavEncrypted;
            }

            if (compress)
            {
                return WebDavCompressed;
            }

            return WebDav;
        }

        if (encrypt)
        {
            return FtpEncrypted;
        }

        if (compress)
        {
            return FtpCompressed;
        }

        return Ftp;
    }

    public static bool IsRegular(int fileType) => AllRegular.Contains(fileType);

    public static bool IsCompressed(int fileType) => AllCompressed.Contains(fileType);

    public static bool IsEncrypted(int fileType) => AllEncrypted.Contains(fileType);

    public static bool IsKnown(int fileType) =>
        IsRegular(fileType) || IsCompressed(fileType) || IsEncrypted(fileType);

    /// <summary>
    /// Maps an encrypted file type to its non-encrypted counterpart for the same medium.
    /// </summary>
    public static bool TryGetRegular(int fileType, out int regularType)
    {
        regularType = fileType switch
        {
            LocalEncrypted => Local,
            FtpEncrypted => Ftp,
            WebDavEncrypted => WebDav,
            LocalCompressed => Local,
            FtpCompressed => Ftp,
            WebDavCompressed => WebDav,
            Local or Ftp or WebDav => fileType,
            _ => 0
        };

        return regularType != 0;
    }

    public static string GetDisplayName(int fileType)
    {
        if (IsCompressed(fileType))
        {
            return "Compressed";
        }

        if (IsEncrypted(fileType))
        {
            return "Encrypted";
        }

        if (fileType == Ftp || fileType == WebDav)
        {
            return "Stored copy";
        }

        if (fileType == Local)
        {
            return "Regular copy";
        }

        return "Unknown";
    }

    public static string GetDisplayName(string fileType)
    {
        return int.TryParse(fileType, out var parsed)
            ? GetDisplayName(parsed)
            : "Unknown";
    }

    /// <summary>
    /// Remap batches that convert every known medium's encoding variants to the target medium.
    /// </summary>
    public static IEnumerable<(int TargetType, IReadOnlyList<int> SourceTypes)> GetRemapBatches(StorageProviderKind targetKind)
    {
        yield return (GetFileType(targetKind, compress: false, encrypt: false), AllRegular);
        yield return (GetFileType(targetKind, compress: true, encrypt: false), AllCompressed);
        yield return (GetFileType(targetKind, compress: false, encrypt: true), AllEncrypted);
    }

    public static async Task RemapAllToAsync(IDbClientFactory dbClientFactory, StorageProviderKind targetKind)
    {
        ArgumentNullException.ThrowIfNull(dbClientFactory);

        foreach (var (targetType, sourceTypes) in GetRemapBatches(targetKind))
        {
            var sources = string.Join(", ", sourceTypes);
            await dbClientFactory.ExecuteNonQueryAsync(
                $"UPDATE fileversiontable SET fileType = {targetType} WHERE fileType IN ({sources})");
        }
    }
}
