// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Providers.Ports;

public interface IStorageProvider : IDisposable
{
    StorageProviderKind Kind { get; }

    Task<bool> CheckMedium(bool quickCheck = false);

    void Open();

    bool CanWriteToStorage();

    bool CopyFileToStorage(string localFile, string remoteFile);

    bool CopyFileToStorageCompressed(string localFile, string remoteFile);

    bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password);

    bool CopyFileFromStorage(string localFile, string remoteFile);

    bool FileExists(string remoteFile);

    bool CopyFileFromStorageCompressed(string localFile, string remoteFile);

    bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password);

    bool DecryptOnStorage(string remoteFile, string password);

    bool DeleteFileFromStorage(string remoteFile);

    bool DeleteFileFromStorageCompressed(string remoteFile);

    bool DeleteFileFromStorageEncrypted(string remoteFile);

    bool DeleteDirectory(string remoteDirectory);

    bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget);

    bool UploadDatabaseFile(string databaseFile);

    void UpdateStorageVersion(int versionId);

    bool IsPathTooLong(string path, bool compression, bool encryption);

    long GetFreeSpace();
}
