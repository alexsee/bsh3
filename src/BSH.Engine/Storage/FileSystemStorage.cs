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
using System.ComponentModel;
using System.IO;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Security;
using Ionic.Zip;
using Serilog;

namespace Brightbits.BSH.Engine.Storage;

public class FileSystemStorage : Storage, IStorage
{
    private static readonly ILogger _logger = Log.ForContext<FileSystemStorage>();

    private readonly IConfigurationManager configurationManager;

    private string backupFolder;

    private readonly string volumeSerialNumber;

    private readonly int currentStorageVersion;

    private readonly bool networkStorage = false;

    private readonly string networkUserName;

    private readonly string networkPassword;

    private readonly int compressionLevel;

    private NetworkConnection networkConnection;

    public FileSystemStorage(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
        this.backupFolder = configurationManager.BackupFolder;
        this.volumeSerialNumber = configurationManager.MediaVolumeSerial;
        this.currentStorageVersion = int.Parse(configurationManager.OldBackupPrevent);
        this.networkUserName = configurationManager.UNCUsername;
        this.networkPassword = configurationManager.UNCPassword;
        this.compressionLevel = int.Parse(configurationManager.CompressionLevel);

        networkStorage = !string.IsNullOrEmpty(this.networkUserName);
    }

    public FileSystemStorage(IConfigurationManager configurationManager,
        string backupFolder,
        string volumeSerialNumber,
        int currentStorageVersion,
        string networkUserName,
        string networkPassword,
        int compressionLevel)
    {
        this.configurationManager = configurationManager;
        this.backupFolder = backupFolder;
        this.volumeSerialNumber = volumeSerialNumber;
        this.currentStorageVersion = currentStorageVersion;
        this.networkUserName = networkUserName;
        this.networkPassword = networkPassword;
        this.compressionLevel = compressionLevel;

        networkStorage = !string.IsNullOrEmpty(networkUserName);
    }

    public bool CheckMedium(bool quickCheck = false)
    {
        try
        {
            // connect to network
            using var networkConn = new NetworkConnection(backupFolder, networkUserName, networkPassword);

            // check path
            if (Directory.Exists(backupFolder))
            {
                // check if we can write to the folder
                if (!CanWriteToStorage(backupFolder))
                {
                    return false;
                }

                // store volume serial if not present
                var volumeSerial = Win32Stuff.GetVolumeSerial(backupFolder[..3]);

                if (string.IsNullOrEmpty(volumeSerialNumber) && volumeSerial != null)
                {
                    configurationManager.MediaVolumeSerial = volumeSerial;
                }

                // check serial
                if (!string.IsNullOrEmpty(volumeSerial) && !string.IsNullOrEmpty(volumeSerialNumber))
                {
                    if (volumeSerial == volumeSerialNumber)
                    {
                        return IsValidStorage();
                    }
                    else
                    {
                        _logger.Information("Storage device serial number is not equal to the stored id. We are not sure if that is the same device.");
                        return false;
                    }
                }
            }
            else
            {
                _logger.Information("Medium directory {directoryName} not found; searching for device with corresponding serial number.", backupFolder);

                // search for medium (maybe the drive letter has changed)
                if (string.IsNullOrEmpty(volumeSerialNumber))
                {
                    return false;
                }

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable)
                    {
                        continue;
                    }

                    try
                    {
                        // get serial id
                        var volumeSerial = Win32Stuff.GetVolumeSerial(drive.RootDirectory.FullName[..3]);

                        if (!string.IsNullOrEmpty(volumeSerial) && volumeSerial == volumeSerialNumber)
                        {
                            // drive found
                            _logger.Information("Drive letter updated with the serial number {volumeSerialNumber} at {driveLetter}.",
                                volumeSerialNumber, drive.RootDirectory.FullName);

                            var newBackupFolder = drive.RootDirectory.FullName[..1] + configurationManager.BackupFolder[1..];

                            if (Directory.Exists(newBackupFolder) && CanWriteToStorage(newBackupFolder))
                            {
                                // update folder path
                                configurationManager.BackupFolder = newBackupFolder;
                                backupFolder = newBackupFolder;

                                return IsValidStorage();
                            }
                        }
                    }
                    catch (DeviceContainsWrongStateException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // could not access device, ignore that
                    }
                }

                return false;
            }

            // check storage version
            return IsValidStorage();
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Check if we can actually write to the storage
    /// </summary>
    /// <returns></returns>
    public bool CanWriteToStorage()
    {
        return CanWriteToStorage(backupFolder);
    }

    /// <summary>
    /// Check if we can actually write to the storage
    /// </summary>
    /// <returns></returns>
    private static bool CanWriteToStorage(string folder)
    {
        try
        {
            var testFile = Path.Combine(folder, "bsh.writetest");
            File.WriteAllText(testFile, DateTime.Now.ToString());
            File.Delete(testFile);

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Backup device is not writable and throw an exception.");
            return false;
        }
    }

    private bool IsValidStorage()
    {
        // check version file
        var file = Path.Combine(backupFolder, "backup.bshv");

        if (!File.Exists(file))
        {
            return true;
        }

        // read version file
        var versionId = File.ReadAllText(file);
        if (string.IsNullOrEmpty(versionId))
        {
            return true;
        }

        // check version Id
        if (int.TryParse(versionId, out var versionIdInt) && versionIdInt != currentStorageVersion)
        {
            throw new DeviceContainsWrongStateException();
        }

        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Cleanup

        // are we connecting to a network share?
        DisconnectToNetwork();
    }

    public void Open()
    {
        // are we connecting to a network share?
        ConnectToNetwork();

        // create folder etc.
        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }
    }

    private bool ConnectToNetwork()
    {
        if (networkConnection != null || !networkStorage)
        {
            return true;
        }

        try
        {
            networkConnection = new NetworkConnection(backupFolder, networkUserName, networkPassword);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private void DisconnectToNetwork()
    {
        networkConnection?.Dispose();
    }

    public bool UploadDatabaseFile(string databaseFile)
    {
        File.Copy(databaseFile, Path.Combine(backupFolder, "backup.bshdb"), true);
        return true;
    }

    public void UpdateStorageVersion(int versionId)
    {
        File.WriteAllText(Path.Combine(backupFolder, "backup.bshv"), versionId.ToString());
    }

    public bool CopyFileToStorage(string localFile, string remoteFile)
    {
        // create directory if not exists
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile));

        Directory.CreateDirectory(Path.GetDirectoryName(remoteFilePath));
        File.Copy(GetLocalFileName(localFile), remoteFilePath);

        return true;
    }

    public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
    {
        // create directory if not exists
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile));
        Directory.CreateDirectory(Path.GetDirectoryName(remoteFilePath));

        using var fs = new FileStream(remoteFilePath + ".zip", FileMode.Create, FileAccess.ReadWrite);
        using var zipFile = new ZipFile();

        zipFile.ParallelDeflateThreshold = -1;
        zipFile.CompressionLevel = (Ionic.Zlib.CompressionLevel)compressionLevel;
        zipFile.UseZip64WhenSaving = Zip64Option.AsNecessary;
        zipFile.AddFile(GetLocalFileName(localFile), "\\");

        zipFile.Save(fs);

        return true;
    }

    public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password)
    {
        // create directory if not exists
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile) + ".enc");
        Directory.CreateDirectory(Path.GetDirectoryName(remoteFilePath));

        var crypto = new Encryption();
        if (!crypto.Encode(GetLocalFileName(localFile), remoteFilePath, password))
        {
            return false;
        }

        var file = new FileInfo(remoteFilePath);
        return file.Length > 0;
    }

    public bool CopyFileFromStorage(string localFile, string remoteFile)
    {
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile));

        // create directory if not exists
        Directory.CreateDirectory(Path.GetDirectoryName(localFile));

        File.Copy(remoteFilePath, GetLocalFileName(localFile), true);

        return true;
    }

    public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
    {
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile) + ".zip");

        // create directory if not exists
        Directory.CreateDirectory(Path.GetDirectoryName(localFile));

        using var zipFile = new ZipFile(remoteFilePath);
        zipFile[0].Extract(GetLocalFileName(Path.GetDirectoryName(localFile)), ExtractExistingFileAction.OverwriteSilently);

        return true;
    }

    public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
    {
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile) + ".enc");

        // create directory if not exists
        Directory.CreateDirectory(Path.GetDirectoryName(localFile));

        var crypto = new Encryption();
        crypto.Decode(remoteFilePath, GetLocalFileName(localFile), password);

        return true;
    }

    public bool DeleteFileFromStorage(string remoteFile)
    {
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile));

        if (!File.Exists(remoteFilePath))
        {
            return true;
        }

        File.SetAttributes(remoteFilePath, FileAttributes.Normal);
        File.Delete(remoteFilePath);

        return true;
    }

    public bool DeleteFileFromStorageCompressed(string remoteFile)
    {
        return DeleteFileFromStorage(remoteFile + ".zip");
    }

    public bool DeleteFileFromStorageEncryped(string remoteFile)
    {
        return DeleteFileFromStorage(remoteFile + ".enc");
    }

    public bool DeleteDirectory(string remoteDirectory)
    {
        try
        {
            Directory.Delete(Path.Combine(backupFolder, CleanRemoteFileName(remoteDirectory)), true);
        }
        catch
        {
            // ignore exception
        }
        return true;
    }

    public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget)
    {
        Directory.Move(Path.Combine(backupFolder, CleanRemoteFileName(remoteDirectorySource)), Path.Combine(backupFolder, CleanRemoteFileName(remoteDirectoryTarget)));
        return true;
    }

    public bool DecryptOnStorage(string remoteFile, string password)
    {
        var remoteFilePath = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile) + ".enc");
        var remoteFilePathDecrypted = Path.Combine(backupFolder, CleanRemoteFileName(remoteFile));

        var crypto = new Encryption();
        var result = crypto.Decode(remoteFilePath, remoteFilePathDecrypted, password);
        File.Delete(remoteFilePath);

        return result;
    }

    public bool IsPathTooLong(string path, bool compression, bool encryption)
    {
        if (compression || encryption)
        {
            return Path.Combine(backupFolder, path).Length + 4 > 255;
        }

        return Path.Combine(backupFolder, path).Length > 255;
    }

    public long GetFreeSpace()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(backupFolder));
            return driveInfo.AvailableFreeSpace;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
