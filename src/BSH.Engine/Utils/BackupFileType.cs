// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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

    public static bool IsRegular(int fileType)
    {
        return fileType == Local || fileType == Ftp || fileType == WebDav;
    }

    public static bool IsCompressed(int fileType)
    {
        return fileType == LocalCompressed || fileType == FtpCompressed || fileType == WebDavCompressed;
    }

    public static bool IsEncrypted(int fileType)
    {
        return fileType == FtpEncrypted || fileType == LocalEncrypted || fileType == WebDavEncrypted;
    }

    public static bool IsKnown(int fileType)
    {
        return IsRegular(fileType) || IsCompressed(fileType) || IsEncrypted(fileType);
    }
}
