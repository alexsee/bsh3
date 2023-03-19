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

using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Jobs;
using Brightbits.BSH.Engine.Security;
using Brightbits.BSH.Engine.Services;
using BSH.Test.Mocks;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BSH.Test
{
    public class BackupTests
    {
        private EngineService engineService;

        [SetUp]
        public async Task Setup()
        {
            if (engineService != null)
            {
                this.engineService.DbClientFactory.ClosePool();
                this.engineService = null;
            }

            // start with clean database
            if (File.Exists("testdb.db"))
            {
                File.Delete("testdb.db");
            }

            this.engineService = new EngineService(Environment.CurrentDirectory + "\\testdb.db");
            await this.engineService.InitAsync();
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
            Assert.ThrowsAsync<NoSourceFolderSelectedException>(async () => await backupJob.BackupAsync(token));
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
            Assert.ThrowsAsync<DeviceNotReadyException>(async () => await backupJob.BackupAsync(token));
        }

        [Test]
        public async Task TestSimpleFullAndIncremental()
        {
            var fs = new StorageMock();
            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            await backupJob.BackupAsync(token);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.AreEqual("1", version.Id);

            // start second backup
            await backupJob.BackupAsync(token);

            // check version
            version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public async Task TestCompressedFull()
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
            await backupJob.BackupAsync(token);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public async Task TestEncryptedFull()
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
            await backupJob.BackupAsync(token);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public async Task TestFullFail()
        {
            // generate backup job
            var fs = new StorageMock(false, true);

            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\Meine Dokumente";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            await backupJob.BackupAsync(token);

            Assert.NotZero(backupJob.FileErrorList.Count);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.Null(version);
        }

        [Test]
        public async Task TestFullDrive()
        {
            // generate backup job
            var fs = new StorageMock();

            var backupJob = new BackupJob(fs, this.engineService.DbClientFactory, this.engineService.QueryManager);
            backupJob.SourceFolder = "D:\\";
            backupJob.Title = "Blub";
            backupJob.Description = "";

            // start backup
            var token = new CancellationTokenSource().Token;
            await backupJob.BackupAsync(token);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.AreEqual("1", version.Id);
        }

        [Test]
        public async Task TestCancellation()
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

            await backupJob.BackupAsync(token);

            // check version
            var version = await this.engineService.QueryManager.GetLastBackupAsync();
            Assert.Null(version);
        }
    }
}