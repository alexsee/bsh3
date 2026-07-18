// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;

namespace Brightbits.BSH.Engine.Contracts;

public interface IConfigurationManager
{
    string AutoBackup
    {
        get;
        set;
    }
    string BackupFolder
    {
        get;
        set;
    }
    string BackupSize
    {
        get;
        set;
    }
    int Compression
    {
        get;
        set;
    }
    string DbStatus
    {
        get;
        set;
    }
    string DBVersion
    {
        get;
        set;
    }
    string DeativateAutoBackupsWhenAkku
    {
        get;
        set;
    }
    string DoPastBackups
    {
        get;
        set;
    }
    int Encrypt
    {
        get;
        set;
    }
    string EncryptPassMD5
    {
        get;
        set;
    }
    string ExcludeCompression
    {
        get;
        set;
    }
    string ExcludeFile
    {
        get;
        set;
    }
    string ExcludeFileBigger
    {
        get;
        set;
    }
    string ExcludeFileTypes
    {
        get;
        set;
    }
    string ExcludeFolder
    {
        get;
        set;
    }
    string ExcludeMask
    {
        get;
        set;
    }
    string FreeSpace
    {
        get;
        set;
    }
    string FtpCoding
    {
        get;
        set;
    }
    string FtpEncryptionMode
    {
        get;
        set;
    }
    string FtpFolder
    {
        get;
        set;
    }
    string FtpHost
    {
        get;
        set;
    }
    string FtpPass
    {
        get;
        set;
    }
    string FtpPort
    {
        get;
        set;
    }
    string FtpSslProtocols
    {
        get;
        set;
    }
    string FtpUser
    {
        get;
        set;
    }
    string InfoBackupDone
    {
        get;
        set;
    }
    string IntervallAutoHourBackups
    {
        get;
        set;
    }
    string IntervallDelete
    {
        get;
        set;
    }
    string IsConfigured
    {
        get;
        set;
    }
    string LastBackupDone
    {
        get;
        set;
    }
    string LastVersionDate
    {
        get;
        set;
    }
    string MediaVolumeSerial
    {
        get;
        set;
    }
    string Medium
    {
        get;
        set;
    }
    MediaType MediumType
    {
        get;
        set;
    }
    string OldBackupPrevent
    {
        get;
        set;
    }
    string RemindAfterDays
    {
        get;
        set;
    }
    string RemindSpace
    {
        get;
        set;
    }
    string ScheduleFullBackup
    {
        get;
        set;
    }
    string ShowLocalizedPath
    {
        get;
        set;
    }
    string ShowWaitOnMediaAutoBackups
    {
        get;
        set;
    }
    string SourceFolder
    {
        get;
        set;
    }
    TaskType TaskType
    {
        get;
        set;
    }
    string UNCPassword
    {
        get;
        set;
    }
    string UNCUsername
    {
        get;
        set;
    }

    Task InitializeAsync();
}