// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Providers.Ports;

namespace BSH.Test.Mocks
{
    public class StorageMock : IStorageProvider
    {
        private readonly bool failCheckMedium;
        private readonly bool failAllCopies;
        private readonly bool pathTooLong;
        private readonly bool throwIoOnFirstRegularCopy;
        private readonly bool throwOnDelete;
        private readonly string throwOnRemoteContaining;
        private readonly CancellationTokenSource cancelOnCopy;
        private readonly HashSet<string> decryptFailFiles;
        private readonly HashSet<string> decryptedOnce = new(StringComparer.OrdinalIgnoreCase);
        private readonly bool failSecondDecryptOfSameFile;
        private int regularCopyAttempts;

        public StorageMock(
            bool failCheckMedium = false,
            bool failAllCopies = false,
            bool pathTooLong = false,
            bool throwIoOnFirstRegularCopy = false,
            string throwOnRemoteContaining = null,
            CancellationTokenSource cancelOnCopy = null,
            bool throwOnDelete = false,
            IEnumerable<string> decryptFailFiles = null,
            bool failSecondDecryptOfSameFile = false,
            long freeSpaceBytes = 0,
            bool canWrite = true)
        {
            this.failCheckMedium = failCheckMedium;
            this.failAllCopies = failAllCopies;
            this.pathTooLong = pathTooLong;
            this.throwIoOnFirstRegularCopy = throwIoOnFirstRegularCopy;
            this.throwOnRemoteContaining = throwOnRemoteContaining;
            this.cancelOnCopy = cancelOnCopy;
            this.throwOnDelete = throwOnDelete;
            this.decryptFailFiles = decryptFailFiles == null
                ? []
                : new HashSet<string>(decryptFailFiles, StringComparer.OrdinalIgnoreCase);
            this.failSecondDecryptOfSameFile = failSecondDecryptOfSameFile;
            FreeSpaceBytes = freeSpaceBytes;
            CanWrite = canWrite;
        }

        public StorageProviderKind Kind => StorageProviderKind.LocalFileSystem;
        public int CopyFileToStorageCalls { get; private set; }
        public int CopyFileToStorageCompressedCalls { get; private set; }
        public int CopyFileToStorageEncryptedCalls { get; private set; }
        public int CopyFileFromStorageCalls { get; private set; }
        public int CopyFileFromStorageCompressedCalls { get; private set; }
        public int CopyFileFromStorageEncryptedCalls { get; private set; }
        public string LastRemoteFile { get; private set; }
        public List<string> CopiedFromStorageRemoteFiles { get; } = [];
        public List<string> CopiedFromStorageCompressedRemoteFiles { get; } = [];
        public List<string> CopiedFromStorageEncryptedRemoteFiles { get; } = [];
        public List<string> DeletedPlain { get; } = [];
        public List<string> DeletedCompressed { get; } = [];
        public List<string> DeletedEncrypted { get; } = [];
        public List<string> DeletedDirectories { get; } = [];
        public List<(string Source, string Target)> RenamedDirectories { get; } = [];
        public List<string> DecryptedFiles { get; } = [];
        public int UploadDatabaseFileCalls { get; private set; }
        public long FreeSpaceBytes { get; set; }
        public bool CanWrite { get; set; }

        public bool CanWriteToStorage()
        {
            return CanWrite;
        }

        public async Task<bool> CheckMedium(bool quickCheck = false)
        {
            return !failCheckMedium;
        }

        public bool CopyFileFromStorage(string localFile, string remoteFile)
        {
            return RecordCopyFromStorage(
                CopiedFromStorageRemoteFiles,
                () => CopyFileFromStorageCalls++,
                remoteFile);
        }

        public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
        {
            return RecordCopyFromStorage(
                CopiedFromStorageCompressedRemoteFiles,
                () => CopyFileFromStorageCompressedCalls++,
                remoteFile);
        }

        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
        {
            return RecordCopyFromStorage(
                CopiedFromStorageEncryptedRemoteFiles,
                () => CopyFileFromStorageEncryptedCalls++,
                remoteFile);
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
            if (decryptFailFiles.Contains(remoteFile))
            {
                throw new IOException("Decrypt failed.");
            }

            if (failSecondDecryptOfSameFile && !decryptedOnce.Add(remoteFile))
            {
                throw new IOException("File already decrypted.");
            }

            DecryptedFiles.Add(remoteFile);
            return !failAllCopies;
        }

        public bool DeleteDirectory(string remoteDirectory)
        {
            DeletedDirectories.Add(remoteDirectory);
            return !failAllCopies;
        }

        public bool DeleteFileFromStorage(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedPlain.Add(remoteFile);
            return !failAllCopies;
        }

        public bool DeleteFileFromStorageCompressed(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedCompressed.Add(remoteFile);
            return !failAllCopies;
        }

        public bool DeleteFileFromStorageEncrypted(string remoteFile)
        {
            if (throwOnDelete)
            {
                throw new IOException("Delete failed.");
            }

            DeletedEncrypted.Add(remoteFile);
            return !failAllCopies;
        }

        public void Dispose()
        {
        }

        public long GetFreeSpace()
        {
            return FreeSpaceBytes;
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
            RenamedDirectories.Add((remoteDirectorySource, remoteDirectoryTarget));
            return true;
        }

        public void UpdateStorageVersion(int versionId)
        {
        }

        public bool UploadDatabaseFile(string databaseFile)
        {
            UploadDatabaseFileCalls++;
            return true;
        }

        private bool RecordCopyFromStorage(List<string> remoteFiles, Action incrementCalls, string remoteFile)
        {
            if (!string.IsNullOrEmpty(throwOnRemoteContaining) &&
                remoteFile.Contains(throwOnRemoteContaining, StringComparison.Ordinal))
            {
                throw new IOException("Simulated restore failure");
            }

            cancelOnCopy?.Cancel();
            incrementCalls();
            LastRemoteFile = remoteFile;
            remoteFiles.Add(remoteFile);
            return !failAllCopies;
        }
    }
}
