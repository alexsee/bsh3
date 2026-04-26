// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;

namespace BSH.Test.Mocks
{
    public class StorageMock : IStorage
    {
        private readonly bool failCheckMedium;
        private readonly bool failAllCopies;
        private readonly bool pathTooLong;
        private readonly bool throwIoOnFirstRegularCopy;
        private readonly long freeSpace;
        private int regularCopyAttempts;

        public StorageMock(
            bool failCheckMedium = false,
            bool failAllCopies = false,
            bool pathTooLong = false,
            bool throwIoOnFirstRegularCopy = false,
            long freeSpace = 0)
        {
            this.failCheckMedium = failCheckMedium;
            this.failAllCopies = failAllCopies;
            this.pathTooLong = pathTooLong;
            this.throwIoOnFirstRegularCopy = throwIoOnFirstRegularCopy;
            this.freeSpace = freeSpace;
        }

        public StorageProviderKind Kind => StorageProviderKind.LocalFileSystem;
        public int CopyFileToStorageCalls { get; private set; }
        public int CopyFileToStorageCompressedCalls { get; private set; }
        public int CopyFileToStorageEncryptedCalls { get; private set; }
        public string LastRemoteFile { get; private set; }

        public bool CanWriteToStorage()
        {
            return true;
        }

        public async Task<bool> CheckMedium(bool quickCheck = false)
        {
            return !failCheckMedium;
        }

        public bool CopyFileFromStorage(string localFile, string remoteFile)
        {
            return !failAllCopies;
        }

        public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
        {
            return !failAllCopies;
        }

        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
        {
            return !failAllCopies;
        }

        public bool CopyFileToStorage(string localFile, string remoteFile)
        {
            CopyFileToStorageCalls++;
            LastRemoteFile = remoteFile;
            regularCopyAttempts++;

            if (throwIoOnFirstRegularCopy && regularCopyAttempts == 1)
            {
                throw new IOException("Simulated IO failure");
            }

            return !failAllCopies;
        }

        public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
        {
            CopyFileToStorageCompressedCalls++;
            LastRemoteFile = remoteFile;
            return !failAllCopies;
        }

        public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password)
        {
            CopyFileToStorageEncryptedCalls++;
            LastRemoteFile = remoteFile;
            return !failAllCopies;
        }

        public bool DecryptOnStorage(string remoteFile, string password)
        {
            return !failAllCopies;
        }

        public bool DeleteDirectory(string remoteDirectory)
        {
            return !failAllCopies;
        }

        public bool DeleteFileFromStorage(string remoteFile)
        {
            return !failAllCopies;
        }

        public bool DeleteFileFromStorageCompressed(string remoteFile)
        {
            return !failAllCopies;
        }

        public bool DeleteFileFromStorageEncrypted(string remoteFile)
        {
            return !failAllCopies;
        }

        public void Dispose()
        {
        }

        public long GetFreeSpace()
        {
            return freeSpace;
        }

        public bool IsPathTooLong(string path, bool compression, bool encryption)
        {
            return pathTooLong;
        }

        public void Open()
        {
        }

        public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget)
        {
            return true;
        }

        public void UpdateStorageVersion(int versionId)
        {
        }

        public bool UploadDatabaseFile(string databaseFile)
        {
            return true;
        }
    }
}
