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

using Brightbits.BSH.Engine.Database;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Brightbits.BSH.Engine
{
    public class ConfigurationManager
    {
        private readonly DbClientFactory dbClientFactory;

        private string taskType = "2";

        public TaskType TaskType
        {
            get { return (TaskType)int.Parse(taskType); }
            set { taskType = ((int)value).ToString(); SaveProperty(nameof(TaskType), ((int)value).ToString()); }
        }

        private string sourceFolder = "";

        public string SourceFolder
        {
            get { return sourceFolder; }
            set { sourceFolder = value; SaveProperty(nameof(SourceFolder), value); }
        }

        private string backupFolder = "";

        public string BackupFolder
        {
            get { return backupFolder; }
            set { backupFolder = value; SaveProperty(nameof(BackupFolder), value); }
        }

        private string remindAfterDays = "7";

        public string RemindAfterDays
        {
            get { return remindAfterDays; }
            set { remindAfterDays = value; SaveProperty(nameof(RemindAfterDays), value); }
        }

        private string autoBackup = "";

        public string AutoBackup
        {
            get { return autoBackup; }
            set { autoBackup = value; SaveProperty(nameof(AutoBackup), value); }
        }

        private string medium = "1";

        public string Medium
        {
            get { return medium; }
            set { medium = value; SaveProperty(nameof(Medium), value); }
        }

        private int mediumType = 1;

        public int MediumType
        {
            get { return mediumType; }
            set { mediumType = value; SaveProperty(nameof(MediumType), value.ToString()); }
        }

        private string lastBackupDone = "";

        public string LastBackupDone
        {
            get { return lastBackupDone; }
            set { lastBackupDone = value; SaveProperty(nameof(LastBackupDone), value); }
        }

        private string lastVersionDate = "";

        public string LastVersionDate
        {
            get { return lastVersionDate; }
            set { lastVersionDate = value; SaveProperty(nameof(LastVersionDate), value); }
        }

        private string oldBackupPrevent = "0";

        public string OldBackupPrevent
        {
            get { return oldBackupPrevent; }
            set { oldBackupPrevent = value; SaveProperty(nameof(OldBackupPrevent), value); }
        }

        private int compression = 0;

        public int Compression
        {
            get { return compression; }
            set { compression = value; SaveProperty(nameof(Compression), value.ToString()); }
        }

        private string compressionLevel = "9";

        public string CompressionLevel
        {
            get { return compressionLevel; }
            set { compressionLevel = value; SaveProperty(nameof(CompressionLevel), value.ToString()); }
        }


        private string excludeCompression = ".zip|.rar|.7zip";

        public string ExcludeCompression
        {
            get { return excludeCompression; }
            set { excludeCompression = value; SaveProperty(nameof(ExcludeCompression), value); }
        }

        private int encrypt = 0;

        public int Encrypt
        {
            get { return encrypt; }
            set { encrypt = value; SaveProperty(nameof(Encrypt), value.ToString()); }
        }

        private string encryptPassMd5 = "";

        public string EncryptPassMD5
        {
            get { return encryptPassMd5; }
            set { encryptPassMd5 = value; SaveProperty(nameof(EncryptPassMD5), value.ToString()); }
        }

        private string excludeFolder = "";

        public string ExcludeFolder
        {
            get { return excludeFolder; }
            set { excludeFolder = value; SaveProperty(nameof(ExcludeFolder), value); }
        }

        private string exludeFileTypes = "";

        public string ExcludeFileTypes
        {
            get { return exludeFileTypes; }
            set { exludeFileTypes = value; SaveProperty(nameof(ExcludeFileTypes), value); }
        }

        private string excludeFileBigger = "";

        public string ExcludeFileBigger
        {
            get { return excludeFileBigger; }
            set { excludeFileBigger = value; SaveProperty(nameof(ExcludeFileBigger), value); }
        }

        private string excludeMask = "";

        public string ExcludeMask
        {
            get { return excludeMask; }
            set { excludeMask = value; SaveProperty(nameof(ExcludeMask), value); }
        }

        private string excludeFile = "";

        public string ExcludeFile
        {
            get { return excludeFile; }
            set { excludeFile = value; SaveProperty(nameof(ExcludeFile), value); }
        }

        private string freeSpace = "0";

        public string FreeSpace
        {
            get { return freeSpace; }
            set { freeSpace = value; SaveProperty(nameof(FreeSpace), value.ToString()); }
        }

        private string remindSpace = "-1";

        public string RemindSpace
        {
            get { return remindSpace; }
            set { remindSpace = value; SaveProperty(nameof(RemindSpace), value.ToString()); }
        }

        private string doPastBackups = "0";

        public string DoPastBackups
        {
            get { return doPastBackups; }
            set { doPastBackups = value; SaveProperty(nameof(DoPastBackups), value.ToString()); }
        }

        private string ftpHost = "";

        public string FtpHost
        {
            get { return ftpHost; }
            set { ftpHost = value; SaveProperty(nameof(FtpHost), value.ToString()); }
        }

        private string ftpUser = "";

        public string FtpUser
        {
            get { return ftpUser; }
            set { ftpUser = value; SaveProperty(nameof(FtpUser), value.ToString()); }
        }

        private string ftpPass = "";

        public string FtpPass
        {
            get { return ftpPass; }
            set { ftpPass = value; SaveProperty(nameof(FtpPass), value.ToString()); }
        }

        private string ftpFolder;

        public string FtpFolder
        {
            get { return ftpFolder; }
            set { ftpFolder = value; SaveProperty(nameof(FtpFolder), value.ToString()); }
        }

        private string ftpPort = "21";

        public string FtpPort
        {
            get { return ftpPort; }
            set { ftpPort = value; SaveProperty(nameof(FtpPort), value.ToString()); }
        }

        private string ftpCoding = "ISO-8859-1";

        public string FtpCoding
        {
            get { return string.IsNullOrEmpty(ftpCoding) ? "ISO-8859-1" : ftpCoding; }
            set { ftpCoding = value; SaveProperty(nameof(FtpCoding), value.ToString()); }
        }

        private string ftpEncryptionMode = "3";

        public string FtpEncryptionMode
        {
            get { return string.IsNullOrEmpty(ftpEncryptionMode) ? "3" : ftpEncryptionMode; }
            set { ftpEncryptionMode = value; SaveProperty(nameof(FtpEncryptionMode), value.ToString()); }
        }

        private string ftpSslProtocols = "0";

        public string FtpSslProtocols
        {
            get { return string.IsNullOrEmpty(ftpSslProtocols) ? "0" : ftpSslProtocols; }
            set { ftpSslProtocols = value; SaveProperty(nameof(FtpSslProtocols), value.ToString()); }
        }

        private string isConfigured = "0";

        public string IsConfigured
        {
            get { return isConfigured; }
            set { isConfigured = value; SaveProperty(nameof(IsConfigured), value.ToString()); }
        }

        private string dbStatus = "0";

        public string DbStatus
        {
            get { return dbStatus; }
            set { dbStatus = value; SaveProperty(nameof(DbStatus), value.ToString()); }
        }

        private string deactivateAutoBackupsWhenAkku = "1";

        public string DeativateAutoBackupsWhenAkku
        {
            get { return deactivateAutoBackupsWhenAkku; }
            set { deactivateAutoBackupsWhenAkku = value; SaveProperty(nameof(DeativateAutoBackupsWhenAkku), value.ToString()); }
        }

        private string intervallDelete = "";

        public string IntervallDelete
        {
            get { return intervallDelete; }
            set { intervallDelete = value; SaveProperty(nameof(IntervallDelete), value.ToString()); }
        }

        private string dbVersion = "";

        public string DBVersion
        {
            get { return dbVersion; }
            set { dbVersion = value; SaveProperty(nameof(DBVersion), value.ToString()); }
        }

        private string showLocalizedPath = "1";

        public string ShowLocalizedPath
        {
            get { return showLocalizedPath; }
            set { showLocalizedPath = value; SaveProperty(nameof(ShowLocalizedPath), value.ToString()); }
        }

        private string infoBackupDone = "0";

        public string InfoBackupDone
        {
            get { return infoBackupDone; }
            set { infoBackupDone = value; SaveProperty(nameof(InfoBackupDone), value.ToString()); }
        }

        private string mediaVolumeSerial = "";

        public string MediaVolumeSerial
        {
            get { return mediaVolumeSerial; }
            set { mediaVolumeSerial = value; SaveProperty(nameof(MediaVolumeSerial), value?.ToString()); }
        }

        private string backupSize = "";

        public string BackupSize
        {
            get { return backupSize; }
            set { backupSize = value; SaveProperty(nameof(BackupSize), value.ToString()); }
        }

        private string intervallAutoHourBackups = "24";

        public string IntervallAutoHourBackups
        {
            get { return intervallAutoHourBackups; }
            set { intervallAutoHourBackups = value; SaveProperty(nameof(IntervallAutoHourBackups), value.ToString()); }
        }

        private string scheduleFullBackup = "";

        public string ScheduleFullBackup
        {
            get { return scheduleFullBackup; }
            set { scheduleFullBackup = value; SaveProperty(nameof(ScheduleFullBackup), value.ToString()); }
        }

        private string uncUserName = "";

        public string UNCUsername
        {
            get { return uncUserName; }
            set { uncUserName = value; SaveProperty(nameof(UNCUsername), value.ToString()); }
        }

        private string uncPassword = "";

        public string UNCPassword
        {
            get { return uncPassword; }
            set { uncPassword = value; SaveProperty(nameof(UNCPassword), value.ToString()); }
        }

        private string showWaitOnMediaAutoBackups = "0";

        public string ShowWaitOnMediaAutoBackups
        {
            get { return showWaitOnMediaAutoBackups; }
            set { showWaitOnMediaAutoBackups = value; SaveProperty(nameof(ShowWaitOnMediaAutoBackups), value.ToString()); }
        }


        public ConfigurationManager(DbClientFactory dbClientFactory)
        {
            this.dbClientFactory = dbClientFactory;
        }

        public async Task LoadConfigurationAsync()
        {
            using (var dbClient = dbClientFactory.CreateDbClient())
            {
                foreach (var configEntry in GetType().GetProperties())
                {
                    if (configEntry == null)
                    {
                        continue;
                    }

                    var parameters = new IDataParameter[] {
                       dbClient.CreateParameter("value", DbType.String, 1024, configEntry.Name.Replace("_", ""))
                    };

                    var result = await dbClient.ExecuteScalarAsync(CommandType.Text, "SELECT confValue FROM configuration WHERE confProperty LIKE @value LIMIT 1", parameters);

                    if (result == null || result.Equals(DBNull.Value))
                    {
                        continue;
                    }

                    if (configEntry.Name == "Status" || configEntry.Name == "TaskType" || configEntry.Name == "Compression" || configEntry.Name == "Encrypt" || configEntry.Name == "MediumType")
                    {
                        if (int.TryParse(result.ToString(), out int val))
                        {
                            configEntry.SetValue(this, val);
                        }
                    }
                    else
                    {
                        configEntry.SetValue(this, result);
                    }
                }
            }
        }

        private void SaveProperty(string property, string value)
        {
            using (var dbClient = dbClientFactory.CreateDbClient())
            {
                var parameters = new IDataParameter[]
                {
                    dbClient.CreateParameter("value", DbType.String, 255, value),
                    dbClient.CreateParameter("prop", DbType.String, 255, property.ToLower())
                };

                var result = dbClient.ExecuteNonQuery(CommandType.Text, "UPDATE configuration SET confValue = @value WHERE confProperty = @prop", parameters);

                if (result == 0)
                {
                    dbClient.ExecuteNonQuery(CommandType.Text, "INSERT INTO configuration VALUES (@prop, @value)", parameters);
                }
            }
        }
    }
}
