using Brightbits.BSH.Engine.Storage;
using System.Security;

namespace BSH.Test.Mocks
{
    public class StorageMock : IStorage
    {
        private readonly bool failCheckMedium;
        private readonly bool failAllCopies;

        public StorageMock(bool failCheckMedium = false, bool failAllCopies = false)
        {
            this.failCheckMedium = failCheckMedium;
            this.failAllCopies = failAllCopies;
        }

        public bool CanWriteToStorage()
        {
            return true;
        }

        public bool CheckMedium(bool quickCheck = false)
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

        public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, SecureString password)
        {
            return !failAllCopies;
        }

        public bool CopyFileToStorage(string localFile, string remoteFile)
        {
            return !failAllCopies;
        }

        public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
        {
            return !failAllCopies;
        }

        public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, SecureString password)
        {
            return !failAllCopies;
        }

        public bool DecryptOnStorage(string remoteFile, SecureString password)
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

        public bool DeleteFileFromStorageEncryped(string remoteFile)
        {
            return !failAllCopies;
        }

        public void Dispose()
        {
        }

        public long GetFreeSpace()
        {
            return 0;
        }

        public bool IsPathTooLong(string path)
        {
            return false;
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
