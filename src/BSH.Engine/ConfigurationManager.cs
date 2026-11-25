// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Data;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Security;

namespace Brightbits.BSH.Engine;

public class ConfigurationManager : IConfigurationManager
{
    private const string DefaultFtpCoding = "ISO-8859-1";
    private const string DefaultFtpEncryptionMode = "3";
    private const string DefaultFtpSslProtocols = "0";

    private static readonly FieldInfo[] SettingsFields = typeof(Settings).GetFields(BindingFlags.Instance | BindingFlags.Public);

    private readonly IDbClientFactory dbClientFactory;
    private Settings settings = new();

    public TaskType TaskType
    {
        get => settings.TaskType;
        set
        {
            settings.TaskType = value;
            SaveProperty(nameof(TaskType), ((int)value).ToString());
        }
    }

    public string SourceFolder
    {
        get => settings.SourceFolder;
        set
        {
            settings.SourceFolder = value;
            SaveProperty(nameof(SourceFolder), value);
        }
    }

    public string BackupFolder
    {
        get => settings.BackupFolder;
        set
        {
            settings.BackupFolder = value;
            SaveProperty(nameof(BackupFolder), value);
        }
    }

    public string RemindAfterDays
    {
        get => settings.RemindAfterDays;
        set
        {
            settings.RemindAfterDays = value;
            SaveProperty(nameof(RemindAfterDays), value);
        }
    }

    public string Medium
    {
        get => settings.Medium;
        set
        {
            settings.Medium = value;
            SaveProperty(nameof(Medium), value);
        }
    }

    public MediaType MediumType
    {
        get => settings.MediumType;
        set
        {
            settings.MediumType = value;
            SaveProperty(nameof(MediumType), ((int)value).ToString());
        }
    }

    public string LastBackupDone
    {
        get => settings.LastBackupDone;
        set
        {
            settings.LastBackupDone = value;
            SaveProperty(nameof(LastBackupDone), value);
        }
    }

    public string LastBackupResult
    {
        get => settings.LastBackupResult;
        set
        {
            settings.LastBackupResult = value;
            SaveProperty(nameof(LastBackupResult), value);
        }
    }

    public string LastVersionDate
    {
        get => settings.LastVersionDate;
        set
        {
            settings.LastVersionDate = value;
            SaveProperty(nameof(LastVersionDate), value);
        }
    }

    public string OldBackupPrevent
    {
        get => settings.OldBackupPrevent;
        set
        {
            settings.OldBackupPrevent = value;
            SaveProperty(nameof(OldBackupPrevent), value);
        }
    }

    public int Compression
    {
        get => settings.Compression;
        set
        {
            settings.Compression = value;
            SaveProperty(nameof(Compression), value.ToString());
        }
    }

    public string ExcludeCompression
    {
        get => settings.ExcludeCompression;
        set
        {
            settings.ExcludeCompression = value;
            SaveProperty(nameof(ExcludeCompression), value);
        }
    }

    public int Encrypt
    {
        get => settings.Encrypt;
        set
        {
            settings.Encrypt = value;
            SaveProperty(nameof(Encrypt), value.ToString());
        }
    }

    public string EncryptPassMD5
    {
        get => settings.EncryptPassMD5;
        set
        {
            settings.EncryptPassMD5 = value;
            SaveProperty(nameof(EncryptPassMD5), value);
        }
    }

    public string ExcludeFolder
    {
        get => settings.ExcludeFolder;
        set
        {
            settings.ExcludeFolder = value;
            SaveProperty(nameof(ExcludeFolder), value);
        }
    }

    public string ExcludeFileTypes
    {
        get => settings.ExcludeFileTypes;
        set
        {
            settings.ExcludeFileTypes = value;
            SaveProperty(nameof(ExcludeFileTypes), value);
        }
    }

    public string ExcludeFileBigger
    {
        get => settings.ExcludeFileBigger;
        set
        {
            settings.ExcludeFileBigger = value;
            SaveProperty(nameof(ExcludeFileBigger), value);
        }
    }

    public string ExcludeMask
    {
        get => settings.ExcludeMask;
        set
        {
            settings.ExcludeMask = value;
            SaveProperty(nameof(ExcludeMask), value);
        }
    }

    public string ExcludeFile
    {
        get => settings.ExcludeFile;
        set
        {
            settings.ExcludeFile = value;
            SaveProperty(nameof(ExcludeFile), value);
        }
    }

    public string IncludeSystemFolders
    {
        get => settings.IncludeSystemFolders;
        set
        {
            settings.IncludeSystemFolders = value;
            SaveProperty(nameof(IncludeSystemFolders), value);
        }
    }

    public string FreeSpace
    {
        get => settings.FreeSpace;
        set
        {
            settings.FreeSpace = value;
            SaveProperty(nameof(FreeSpace), value);
        }
    }

    public string RemindSpace
    {
        get => settings.RemindSpace;
        set
        {
            settings.RemindSpace = value;
            SaveProperty(nameof(RemindSpace), value);
        }
    }

    public string DoPastBackups
    {
        get => settings.DoPastBackups;
        set
        {
            settings.DoPastBackups = value;
            SaveProperty(nameof(DoPastBackups), value);
        }
    }

    public string FtpHost
    {
        get => settings.FtpHost;
        set
        {
            settings.FtpHost = value;
            SaveProperty(nameof(FtpHost), value);
        }
    }

    public string FtpUser
    {
        get => settings.FtpUser;
        set
        {
            settings.FtpUser = value;
            SaveProperty(nameof(FtpUser), value);
        }
    }

    public string FtpPass
    {
        get => settings.FtpPass;
        set
        {
            settings.FtpPass = value ?? "";
            SaveProperty(nameof(FtpPass), ProtectFtpPassword(settings.FtpPass));
        }
    }

    public string FtpFolder
    {
        get => settings.FtpFolder;
        set
        {
            settings.FtpFolder = value;
            SaveProperty(nameof(FtpFolder), value);
        }
    }

    public string FtpPort
    {
        get => settings.FtpPort;
        set
        {
            settings.FtpPort = value;
            SaveProperty(nameof(FtpPort), value);
        }
    }

    public string FtpCoding
    {
        get => string.IsNullOrEmpty(settings.FtpCoding) ? DefaultFtpCoding : settings.FtpCoding;
        set
        {
            settings.FtpCoding = value;
            SaveProperty(nameof(FtpCoding), value);
        }
    }

    public string FtpEncryptionMode
    {
        get => string.IsNullOrEmpty(settings.FtpEncryptionMode) ? DefaultFtpEncryptionMode : settings.FtpEncryptionMode;
        set
        {
            settings.FtpEncryptionMode = value;
            SaveProperty(nameof(FtpEncryptionMode), value);
        }
    }

    public string FtpSslProtocols
    {
        get => string.IsNullOrEmpty(settings.FtpSslProtocols) ? DefaultFtpSslProtocols : settings.FtpSslProtocols;
        set
        {
            settings.FtpSslProtocols = value;
            SaveProperty(nameof(FtpSslProtocols), value);
        }
    }

    public string IsConfigured
    {
        get => settings.IsConfigured;
        set
        {
            settings.IsConfigured = value;
            SaveProperty(nameof(IsConfigured), value);
        }
    }

    public string DbStatus
    {
        get => settings.DbStatus;
        set
        {
            settings.DbStatus = value;
            SaveProperty(nameof(DbStatus), value);
        }
    }

    public string DeativateAutoBackupsWhenAkku
    {
        get => settings.DeativateAutoBackupsWhenAkku;
        set
        {
            settings.DeativateAutoBackupsWhenAkku = value;
            SaveProperty(nameof(DeativateAutoBackupsWhenAkku), value);
        }
    }

    public string IntervallDelete
    {
        get => settings.IntervallDelete;
        set
        {
            settings.IntervallDelete = value;
            SaveProperty(nameof(IntervallDelete), value);
        }
    }

    public string DBVersion
    {
        get => settings.DBVersion;
        set
        {
            settings.DBVersion = value;
            SaveProperty(nameof(DBVersion), value);
        }
    }

    public string ShowLocalizedPath
    {
        get => settings.ShowLocalizedPath;
        set
        {
            settings.ShowLocalizedPath = value;
            SaveProperty(nameof(ShowLocalizedPath), value);
        }
    }

    public string InfoBackupDone
    {
        get => settings.InfoBackupDone;
        set
        {
            settings.InfoBackupDone = value;
            SaveProperty(nameof(InfoBackupDone), value);
        }
    }

    public string MediaVolumeSerial
    {
        get => settings.MediaVolumeSerial;
        set
        {
            settings.MediaVolumeSerial = value;
            SaveProperty(nameof(MediaVolumeSerial), value);
        }
    }

    public string BackupSize
    {
        get => settings.BackupSize;
        set
        {
            settings.BackupSize = value;
            SaveProperty(nameof(BackupSize), value);
        }
    }

    public string IntervallAutoHourBackups
    {
        get => settings.IntervallAutoHourBackups;
        set
        {
            settings.IntervallAutoHourBackups = value;
            SaveProperty(nameof(IntervallAutoHourBackups), value);
        }
    }

    public string ScheduleFullBackup
    {
        get => settings.ScheduleFullBackup;
        set
        {
            settings.ScheduleFullBackup = value;
            SaveProperty(nameof(ScheduleFullBackup), value);
        }
    }

    public string UNCUsername
    {
        get => settings.UNCUsername;
        set
        {
            settings.UNCUsername = value;
            SaveProperty(nameof(UNCUsername), value);
        }
    }

    public string UNCPassword
    {
        get => settings.UNCPassword;
        set
        {
            settings.UNCPassword = value;
            SaveProperty(nameof(UNCPassword), value);
        }
    }

    public string ShowWaitOnMediaAutoBackups
    {
        get => settings.ShowWaitOnMediaAutoBackups;
        set
        {
            settings.ShowWaitOnMediaAutoBackups = value;
            SaveProperty(nameof(ShowWaitOnMediaAutoBackups), value);
        }
    }

    public ConfigurationManager(IDbClientFactory dbClientFactory)
    {
        this.dbClientFactory = dbClientFactory;
    }

    public async Task InitializeAsync()
    {
        settings = new Settings();

        using var dbClient = dbClientFactory.CreateDbClient();

        foreach (var field in SettingsFields)
        {
            var result = await LoadConfigurationValueAsync(dbClient, field.Name);
            if (result == null || result == DBNull.Value)
            {
                continue;
            }

            switch (field.Name)
            {
                case nameof(FtpPass):
                    LoadPersistedFtpPassword(result);
                    break;
                case nameof(MediumType):
                    var text = result.ToString();
                    if (int.TryParse(text, out var mediumType))
                    {
                        settings.MediumType = (MediaType)mediumType;
                    }
                    else
                    {
                        // Older databases can store the enum name. Invalid values resolve to Unset.
                        Enum.TryParse(text, out settings.MediumType);
                    }
                    break;
                case nameof(TaskType):
                    if (int.TryParse(result.ToString(), out var taskType))
                    {
                        settings.TaskType = (TaskType)taskType;
                    }
                    break;
                case nameof(Compression):
                case nameof(Encrypt):
                    if (int.TryParse(result.ToString(), out var number))
                    {
                        field.SetValue(settings, number);
                    }
                    break;
                default:
                    field.SetValue(settings, result);
                    break;
            }
        }
    }

    private void LoadPersistedFtpPassword(object persistedValue)
    {
        var persistedPassword = persistedValue.ToString() ?? "";
        if (string.IsNullOrEmpty(persistedPassword))
        {
            settings.FtpPass = "";
            return;
        }

        if (Crypto.TryDecryptString(persistedPassword, DataProtectionScope.LocalMachine, out var plaintextPassword))
        {
            settings.FtpPass = plaintextPassword;
            return;
        }

        // Older databases stored FTP passwords as plaintext. Keep the value usable
        // in memory and replace the persisted legacy value with protected data.
        settings.FtpPass = persistedPassword;
        SaveProperty(nameof(FtpPass), ProtectFtpPassword(settings.FtpPass));
    }

    private static string ProtectFtpPassword(string password)
    {
        return string.IsNullOrEmpty(password)
            ? ""
            : Crypto.EncryptString(password, DataProtectionScope.LocalMachine);
    }

    private static async Task<object> LoadConfigurationValueAsync(DbClient dbClient, string propertyName)
    {
        var parameters = new (string, object)[]
        {
            ("value", NormalizePropertyName(propertyName))
        };

        return await dbClient.ExecuteScalarAsync(
            CommandType.Text,
            "SELECT confValue FROM configuration WHERE confProperty = @value LIMIT 1",
            parameters);
    }

    private void SaveProperty(string property, string value)
    {
        using var dbClient = dbClientFactory.CreateDbClient();

        var parameters = new (string, object)[]
        {
            ("value", value ?? ""),
            ("prop", NormalizePropertyName(property))
        };

        var result = dbClient.ExecuteNonQuery(CommandType.Text, "UPDATE configuration SET confValue = @value WHERE confProperty = @prop", parameters);

        if (result == 0)
        {
            dbClient.ExecuteNonQuery(CommandType.Text, "INSERT INTO configuration VALUES (@prop, @value)", parameters);
        }
    }

    private static string NormalizePropertyName(string propertyName)
    {
        return propertyName.Replace("_", "").ToLowerInvariant();
    }

    // Field names match the public properties and are used as the database keys.
    private sealed class Settings
    {
        public TaskType TaskType = TaskType.Auto;
        public string SourceFolder = "";
        public string BackupFolder = "";
        public string RemindAfterDays = "7";
        public string Medium = "1";
        public MediaType MediumType = MediaType.LocalDevice;
        public string LastBackupDone = "";
        public string LastBackupResult = "";
        public string LastVersionDate = "";
        public string OldBackupPrevent = "0";
        public int Compression;
        public string ExcludeCompression = ".zip|.rar|.7zip";
        public int Encrypt;
        public string EncryptPassMD5 = "";
        public string ExcludeFolder = "";
        public string ExcludeFileTypes = "";
        public string ExcludeFileBigger = "";
        public string ExcludeMask = "";
        public string ExcludeFile = "";
        public string IncludeSystemFolders = "0";
        public string FreeSpace = "0";
        public string RemindSpace = "-1";
        public string DoPastBackups = "0";
        public string FtpHost = "";
        public string FtpUser = "";
        public string FtpPass = "";
        public string FtpFolder;
        public string FtpPort = "21";
        public string FtpCoding = DefaultFtpCoding;
        public string FtpEncryptionMode = DefaultFtpEncryptionMode;
        public string FtpSslProtocols = DefaultFtpSslProtocols;
        public string IsConfigured = "0";
        public string DbStatus = "0";
        public string DeativateAutoBackupsWhenAkku = "1";
        public string IntervallDelete = "";
        public string DBVersion = "";
        public string ShowLocalizedPath = "1";
        public string InfoBackupDone = "0";
        public string MediaVolumeSerial = "";
        public string BackupSize = "";
        public string IntervallAutoHourBackups = "24";
        public string ScheduleFullBackup = "";
        public string UNCUsername = "";
        public string UNCPassword = "";
        public string ShowWaitOnMediaAutoBackups = "0";
    }
}
