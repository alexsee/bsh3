using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Mocks;
using NUnit.Framework;
using System;
using System.IO;
using System.Security;
using System.Threading;

namespace BSH.Test
{
    public class BackupTests
    {
        private EngineService engineService;

        [SetUp]
        public void Setup()
        {
            if (engineService != null)
            {
                this.engineService.DbClientFactory.ClosePool();
                this.engineService = null;
            }

            // start with clean database
            if (File.Exists("testdb.db"))
                File.Delete("testdb.db");

            this.engineService = new EngineService(Environment.CurrentDirectory + "\\testdb.db");
        }

        [Test]
        public void TestEmptySources()
        {
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            Assert.Throws<NoSourceFolderSelectedException>(() => backupJob.Backup(token));
        }

        [Test]
        public void TestFailMedium()
        {
            var fs = new StorageMock(failCheckMedium: true);
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            Assert.Throws<DeviceNotReadyException>(() => backupJob.Backup(token));
        }

        [Test]
        public void TestSimpleFullAndIncremental()
        {
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            backupJob.Backup(token);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.AreEqual("1", version.Id);

            // start second backup
            backupJob.Backup(token);

            // check version
            version = this.engineService.QueryManager.GetLastBackup();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public void TestCompressedFull()
        {
            // set compressed state
            this.engineService.ConfigurationManager.Compression = 1;
            this.engineService.ConfigurationManager.CompressionLevel = "9";

            // generate backup job
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            backupJob.Backup(token);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public void TestEncryptedFull()
        {
            // set compressed state
            this.engineService.ConfigurationManager.Encrypt = 1;
            this.engineService.ConfigurationManager.EncryptPassMD5 = "cc03e747a6afbbcbf8be7668acfebee5";

            // generate backup job
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";
            backupJob.Password = Crypto.ToSecureString("test123");

            // start backup
            var token = new CancellationTokenSource().Token;
            backupJob.Backup(token);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public void TestFullFail()
        {
            // generate backup job
            var fs = new StorageMock(false, true);

            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            backupJob.Backup(token);

            Assert.NotZero(backupJob.FileErrorList.Count);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.Null(version);
        }

        [Test]
        public void TestFullDrive()
        {
            // generate backup job
            var fs = new StorageMock();

            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            backupJob.Backup(token);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public void TestCancellation()
        {
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            tokenSource.Cancel();

            backupJob.Backup(token);

            // check version
            var version = this.engineService.QueryManager.GetLastBackup();
            Assert.Null(version);
        }
    }
}