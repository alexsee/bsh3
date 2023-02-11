// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Security;

namespace Brightbits.BSH.Engine.Storage
{
    public interface IStorage : IDisposable
    {
        bool CheckMedium(bool quickCheck = false);

        void Open();

        bool CanWriteToStorage();

        bool CopyFileToStorage(string localFile, string remoteFile);

        bool CopyFileToStorageCompressed(string localFile, string remoteFile);

        bool CopyFileToStorageEncrypted(string localFile, string remoteFile, SecureString password);

        bool CopyFileFromStorage(string localFile, string remoteFile);

        bool CopyFileFromStorageCompressed(string localFile, string remoteFile);

        bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, SecureString password);

        bool DecryptOnStorage(string remoteFile, SecureString password);

        bool DeleteFileFromStorage(string remoteFile);

        bool DeleteFileFromStorageCompressed(string remoteFile);

        bool DeleteFileFromStorageEncryped(string remoteFile);

        bool DeleteDirectory(string remoteDirectory);

        bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget);

        bool UploadDatabaseFile(string databaseFile);

        void UpdateStorageVersion(int versionId);

        bool IsPathTooLong(string path, bool compression, bool encryption);

        long GetFreeSpace();
    }
}
