// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Threading.Tasks;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;

namespace BSH.Test.Fakes;

public sealed class FakeConfigurationManager : IConfigurationManager
{
    public string AutoBackup { get; set; } = "";
    public string BackupFolder { get; set; } = "";
    public string BackupSize { get; set; } = "";
    public int Compression { get; set; }
    public string DbStatus { get; set; } = "";
    public string DBVersion { get; set; } = "";
    public string DeativateAutoBackupsWhenAkku { get; set; } = "";
    public string DoPastBackups { get; set; } = "";
    public int Encrypt { get; set; }
    public string EncryptPassMD5 { get; set; } = "";
    public string ExcludeCompression { get; set; } = "";
    public string ExcludeFile { get; set; } = "";
    public string ExcludeFileBigger { get; set; } = "";
    public string ExcludeFileTypes { get; set; } = "";
    public string ExcludeFolder { get; set; } = "";
    public string ExcludeMask { get; set; } = "";
    public string FreeSpace { get; set; } = "";
    public string FtpCoding { get; set; } = "";
    public string FtpEncryptionMode { get; set; } = "";
    public string FtpFolder { get; set; } = "";
    public string FtpHost { get; set; } = "";
    public string FtpPass { get; set; } = "";
    public string FtpPort { get; set; } = "";
    public string FtpSslProtocols { get; set; } = "";
    public string FtpUser { get; set; } = "";
    public string InfoBackupDone { get; set; } = "";
    public string IntervallAutoHourBackups { get; set; } = "";
    public string IntervallDelete { get; set; } = "";
    public string IsConfigured { get; set; } = "";
    public string LastBackupDone { get; set; } = "";
    public string LastVersionDate { get; set; } = "";
    public string MediaVolumeSerial { get; set; } = "";
    public string Medium { get; set; } = "";
    public MediaType MediumType { get; set; }
    public string OldBackupPrevent { get; set; } = "";
    public string RemindAfterDays { get; set; } = "";
    public string RemindSpace { get; set; } = "";
    public string ScheduleFullBackup { get; set; } = "";
    public string ShowLocalizedPath { get; set; } = "";
    public string ShowWaitOnMediaAutoBackups { get; set; } = "";
    public string SourceFolder { get; set; } = "";
    public TaskType TaskType { get; set; }
    public string UNCPassword { get; set; } = "";
    public string UNCUsername { get; set; } = "";

    public Task InitializeAsync() => Task.CompletedTask;
}
