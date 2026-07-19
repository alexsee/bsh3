// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Providers.Ports;
using FluentFTP;
using FluentFTP.Client.BaseClient;
using FluentFTP.Exceptions;
using FluentFTP.Helpers;
using FluentFTP.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace Brightbits.BSH.Engine.Storage;

public class FtpStorage : Storage, IStorageProvider
{
    private static readonly ILogger _logger = Log.ForContext<FtpStorage>();

    private readonly int currentStorageVersion;

    private readonly string serverAddress;

    private readonly int serverPort;

    private readonly string userName;

    private readonly string password;

    private readonly string encoding;

    private readonly string folderPath;

    private readonly bool encryption;

    private FtpClient ftpClient;

    public FtpStorage(IConfigurationManager configurationManager)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        var credentials = RemoteStorageCredentials.FromConfiguration(configurationManager);
        this.serverAddress = credentials.Host;
        this.serverPort = credentials.Port > 0 ? credentials.Port : int.Parse(configurationManager.FtpPort);
        this.userName = credentials.UserName;
        this.password = credentials.Password;
        this.folderPath = credentials.Folder;
        this.encoding = credentials.Encoding;
        this.encryption = credentials.UseEncryption;
        this.currentStorageVersion = int.Parse(configurationManager.OldBackupPrevent);
    }

    public FtpStorage(
        string serverAddress, int serverPort, string userName, string password, string folderPath, string encoding,
        bool encryption, int currentStorageVersion)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        this.userName = userName;
        this.password = password;
        this.folderPath = folderPath;
        this.encoding = encoding;
        this.encryption = encryption;
        this.currentStorageVersion = currentStorageVersion;
    }

    public StorageProviderKind Kind => StorageProviderKind.Ftp;

    private static string Combine(string path1, string path2)
    {
        return path1 + @"/" + path2;
    }

    private FtpProfile GetFtpProfile(bool quickCheck = false)
    {
        Encoding finalEncoding;

        // set encoding
        if (this.encoding == "UTF-8")
        {
            finalEncoding = new UTF8Encoding(false);
        }
        else
        {
            finalEncoding = Encoding.GetEncoding(this.encoding);
        }

        // determine profile
        var profile = new FtpProfile()
        {
            Host = this.serverAddress,
            Credentials = new NetworkCredential(this.userName, this.password),
            Encoding = finalEncoding,
            Timeout = quickCheck ? 15000 : 60000,
            RetryAttempts = 1,
        };

        return profile;
    }

    private static void ConfigureClientLogger(BaseFtpClient client, ILogger logger)
    {
        var ftpLogger = new SerilogLoggerFactory(logger).CreateLogger("FtpClient");
        client.Logger = new FtpLogAdapter(ftpLogger);
        client.Config.LogHost = false;
        client.Config.LogDurations = false;
    }

    public static bool CheckConnection(string host, int port, string userName, string password, string folderPath, string encoding)
    {
        var credentials = new NetworkCredential(userName, password);
        var config = new FtpConfig
        {
            ConnectTimeout = 60000,
            DataConnectionConnectTimeout = 60000,
            DataConnectionReadTimeout = 60000,

            // ignore all certificates
            ValidateAnyCertificate = true
        };

        using var client = new FtpClient(host, credentials, port, config);
        ConfigureClientLogger(client, _logger);

        // set encoding
        if (encoding == "UTF-8")
        {
            client.Encoding = new UTF8Encoding(false);
        }
        else
        {
            client.Encoding = Encoding.GetEncoding(encoding);
        }

        // first, try direct connect
        try
        {
            client.Connect();
        }
        catch
        {
            // failed, so try auto connect
            client.AutoConnect();
        }

        if (!client.IsConnected)
        {
            return false;
        }

        // check if folder exists
        if (!client.DirectoryExists(folderPath.GetFtpPath()))
        {
            client.Disconnect();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckMedium(bool quickCheck = false)
    {
        try
        {
            var credentials = new NetworkCredential(userName, password);
            var config = new FtpConfig()
            {
                // ignore all certificates
                ValidateAnyCertificate = true,
            };

            using var client = new AsyncFtpClient(serverAddress, credentials, serverPort, config);
            client.LoadProfile(GetFtpProfile(quickCheck));
            ConfigureClientLogger(client, _logger);

            if (this.encryption)
            {
                await client.AutoConnect();
            }
            else
            {
                await client.Connect();
            }

            if (!client.IsConnected)
            {
                return false;
            }

            // check if folder exists
            if (!await client.DirectoryExists(folderPath.GetFtpPath()))
            {
                _logger.Warning("FTP server does not have the specified folder.");
                return false;
            }

            // check if backup.bshv file exists
            var remoteBackupVersionFile = Combine(folderPath, "backup.bshv").GetFtpPath();
            var localBackupVersionFile = Path.Combine(Path.GetTempPath(), "backup.bshv");

            if (await client.FileExists(remoteBackupVersionFile))
            {
                await client.DownloadFile(localBackupVersionFile, remoteBackupVersionFile, FtpLocalExists.Overwrite);

                var versionId = await File.ReadAllTextAsync(localBackupVersionFile);
                File.Delete(localBackupVersionFile);

                if (!string.IsNullOrEmpty(versionId) && int.Parse(versionId) != currentStorageVersion)
                {
                    _logger.Warning("FTP server contains an inconsistent state. Version file contains a different version than the computers backup version.");
                    throw new DeviceContainsWrongStateException();
                }
            }

            return true;
        }
        catch (DeviceContainsWrongStateException)
        {
            throw;
        }
        catch (FtpException ex)
        {
            _logger.Error(ex, "FTP Client Exception");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception during connecting to ftp server");
            return false;
        }
    }

    public bool CanWriteToStorage()
    {
        if (this.ftpClient != null)
        {
            return this.ftpClient.IsConnected;
        }

        return false;
    }

    public bool CopyFileToStorage(string localFile, string remoteFile)
    {
        // create directory if not exists
        var remoteFilePath = Combine(folderPath, remoteFile).GetFtpPath();
        var result = ftpClient.UploadFile(GetLocalFileName(localFile), remoteFilePath, FtpRemoteExists.Overwrite, true);
        return result == FtpStatus.Success;
    }

    public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
    {
        return RemoteStorageContent.CopyCompressed(
            GetLocalFileName(localFile),
            remoteFile,
            (tmp, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.UploadFile(tmp, remoteFilePath, FtpRemoteExists.Overwrite, true) == FtpStatus.Success;
            });
    }

    public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password)
    {
        return RemoteStorageContent.CopyEncrypted(
            GetLocalFileName(localFile),
            remoteFile,
            password,
            (tmp, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.UploadFile(tmp, remoteFilePath, FtpRemoteExists.Overwrite, true) == FtpStatus.Success;
            });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (ftpClient != null)
        {
            if (ftpClient.IsConnected)
            {
                ftpClient.Disconnect();
            }

            ftpClient.Dispose();
        }
    }

    public void Open()
    {
        // create instance
        if (ftpClient == null)
        {
            var credentials = new NetworkCredential(userName, password);
            var config = new FtpConfig()
            {
                // ignore all certificates
                ValidateAnyCertificate = true
            };

            ftpClient = new FtpClient(serverAddress, credentials, serverPort, config);
            ftpClient.LoadProfile(GetFtpProfile());
            ConfigureClientLogger(ftpClient, _logger);
        }

        if (this.encryption)
        {
            ftpClient.AutoConnect();
        }
        else
        {
            ftpClient.Connect();
        }
    }

    public void UpdateStorageVersion(int versionId)
    {
        RemoteStorageContent.WriteVersionFile(versionId, (local, remote) =>
            ftpClient.UploadFile(local, Combine(folderPath, remote).GetFtpPath(), FtpRemoteExists.Overwrite) == FtpStatus.Success);
    }

    public bool UploadDatabaseFile(string databaseFile)
    {
        var result = ftpClient.UploadFile(databaseFile, Combine(folderPath, "backup.bshdb").GetFtpPath());
        return result == FtpStatus.Success;
    }

    public bool CopyFileFromStorage(string localFile, string remoteFile)
    {
        var remoteFilePath = Combine(folderPath, remoteFile).GetFtpPath();

        // create directory if not exists
        Directory.CreateDirectory(Path.GetDirectoryName(localFile));

        var result = ftpClient.DownloadFile(GetLocalFileName(localFile), remoteFilePath, FtpLocalExists.Overwrite);
        return result == FtpStatus.Success;
    }

    public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
    {
        return RemoteStorageContent.CopyFromCompressed(
            GetLocalFileName(localFile),
            remoteFile,
            (local, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.DownloadFile(local, remoteFilePath, FtpLocalExists.Overwrite) == FtpStatus.Success;
            });
    }

    public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
    {
        return RemoteStorageContent.CopyFromEncrypted(
            GetLocalFileName(localFile),
            remoteFile,
            password,
            (local, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.DownloadFile(local, remoteFilePath, FtpLocalExists.Overwrite) == FtpStatus.Success;
            });
    }

    public bool DeleteFileFromStorage(string remoteFile)
    {
        var remoteFilePath = Combine(folderPath, remoteFile).GetFtpPath();
        ftpClient.DeleteFile(remoteFilePath);

        return true;
    }

    public bool DeleteFileFromStorageCompressed(string remoteFile)
    {
        return DeleteFileFromStorage(remoteFile + ".zip");
    }

    public bool DeleteFileFromStorageEncrypted(string remoteFile)
    {
        return DeleteFileFromStorage(remoteFile + ".enc");
    }

    public bool DeleteDirectory(string remoteDirectory)
    {
        ftpClient.DeleteDirectory(Combine(folderPath, remoteDirectory).GetFtpPath());
        return true;
    }

    public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget)
    {
        return ftpClient.MoveDirectory(Combine(folderPath, remoteDirectorySource).GetFtpPath(),
            Combine(folderPath, remoteDirectoryTarget).GetFtpPath());
    }

    public bool DecryptOnStorage(string remoteFile, string password)
    {
        return RemoteStorageContent.DecryptOnStorage(
            remoteFile,
            password,
            (local, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.DownloadFile(local, remoteFilePath, FtpLocalExists.Overwrite) == FtpStatus.Success;
            },
            (local, remote) =>
            {
                var remoteFilePath = Combine(folderPath, remote).GetFtpPath();
                return ftpClient.UploadFile(local, remoteFilePath, FtpRemoteExists.Overwrite) == FtpStatus.Success;
            },
            DeleteFileFromStorageEncrypted);
    }

    public bool FileExists(string remoteFile)
    {
        var remoteFilePath = Combine(folderPath, remoteFile).GetFtpPath();
        return ftpClient.FileExists(remoteFilePath);
    }

    public bool IsPathTooLong(string path, bool compression, bool encryption)
    {
        return false;
    }

    public long GetFreeSpace()
    {
        return 0L;
    }

    public static string GetFtpPath(string path)
    {
        return path.GetFtpPath();
    }
}
