// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Providers.Ports;
using Serilog;

namespace Brightbits.BSH.Engine.Storage;

public class WebDavStorage : Storage, IStorage
{
    private static readonly ILogger Logger = Log.ForContext<WebDavStorage>();

    private readonly int currentStorageVersion;
    private readonly string serverAddress;
    private readonly int serverPort;
    private readonly string userName;
    private readonly string password;
    private readonly string folderPath;
    private readonly HttpMessageHandler messageHandler;

    private WebDavClient webDavClient;

    public WebDavStorage(IConfigurationManager configurationManager)
        : this(RemoteStorageCredentials.FromConfiguration(configurationManager),
            int.TryParse(configurationManager.OldBackupPrevent, out var version) ? version : 0)
    {
    }

    public WebDavStorage(RemoteStorageCredentials credentials, int currentStorageVersion)
        : this(
            credentials.Host,
            credentials.Port,
            credentials.UserName,
            credentials.Password,
            credentials.Folder,
            currentStorageVersion)
    {
    }

    public WebDavStorage(
        string serverAddress,
        int serverPort,
        string userName,
        string password,
        string folderPath,
        int currentStorageVersion,
        HttpMessageHandler messageHandler = null)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        this.userName = userName;
        this.password = password;
        this.folderPath = folderPath;
        this.currentStorageVersion = currentStorageVersion;
        this.messageHandler = messageHandler;
    }

    public StorageProviderKind Kind => StorageProviderKind.WebDav;

    public static bool CheckConnection(string host, int port, string userName, string password, string folderPath)
    {
        return CheckConnection(new RemoteStorageCredentials
        {
            Host = host,
            Port = port,
            UserName = userName,
            Password = password,
            Folder = folderPath
        });
    }

    public static bool CheckConnection(RemoteStorageCredentials credentials)
    {
        try
        {
            using var storage = new WebDavStorage(credentials, currentStorageVersion: 0);
            storage.Open();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            return storage.webDavClient.DirectoryExistsAsync("", timeoutCts.Token).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception during WebDAV connection check.");
            return false;
        }
    }

    public async Task<bool> CheckMedium(bool quickCheck = false)
    {
        try
        {
            Open();

            using var timeoutCts = new CancellationTokenSource(quickCheck ? TimeSpan.FromSeconds(15) : TimeSpan.FromSeconds(60));
            var cancellationToken = timeoutCts.Token;

            if (!await webDavClient.DirectoryExistsAsync("", cancellationToken))
            {
                Logger.Warning("WebDAV server does not have the specified folder.");
                return false;
            }

            if (!await webDavClient.CanWriteAsync(cancellationToken))
            {
                Logger.Warning("WebDAV server is not writable.");
                return false;
            }

            var versionContent = await webDavClient.ReadTextFileIfExistsAsync("backup.bshv", cancellationToken);
            if (!string.IsNullOrWhiteSpace(versionContent)
                && int.TryParse(versionContent, out var storageVersion)
                && storageVersion != currentStorageVersion)
            {
                Logger.Warning("WebDAV server contains an inconsistent state. Version file contains a different version than the computers backup version.");
                throw new DeviceContainsWrongStateException();
            }

            return true;
        }
        catch (DeviceContainsWrongStateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception during connecting to WebDAV server.");
            return false;
        }
    }

    public bool CanWriteToStorage()
    {
        try
        {
            Open();
            return webDavClient.CanWriteAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Exception while probing WebDAV writability.");
            return false;
        }
    }

    public bool CopyFileToStorage(string localFile, string remoteFile)
    {
        var remoteFilePath = WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteFile));
        return UploadFileToStorage(GetLocalFileName(localFile), remoteFilePath);
    }

    public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
    {
        return RemoteStorageContent.CopyCompressed(
            GetLocalFileName(localFile),
            remoteFile,
            (tmp, remote) => CopyFileToStorage(tmp, remote));
    }

    public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password)
    {
        return RemoteStorageContent.CopyEncrypted(
            GetLocalFileName(localFile),
            remoteFile,
            password,
            (tmp, remote) => CopyFileToStorage(tmp, remote));
    }

    public bool CopyFileFromStorage(string localFile, string remoteFile)
    {
        var directory = Path.GetDirectoryName(localFile);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return DownloadFileFromStorage(
            WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteFile)),
            GetLocalFileName(localFile));
    }

    public bool FileExists(string remoteFile)
    {
        var remoteFilePath = WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteFile));

        try
        {
            Open();
            return webDavClient.FileExistsAsync(remoteFilePath).GetAwaiter().GetResult();
        }
        catch
        {
            return false;
        }
    }

    public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
    {
        return RemoteStorageContent.CopyFromCompressed(
            GetLocalFileName(localFile),
            remoteFile,
            (local, remote) => DownloadFileFromStorage(
                WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remote)),
                local));
    }

    public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
    {
        return RemoteStorageContent.CopyFromEncrypted(
            GetLocalFileName(localFile),
            remoteFile,
            password,
            (local, remote) => DownloadFileFromStorage(
                WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remote)),
                local));
    }

    public bool DecryptOnStorage(string remoteFile, string password)
    {
        return RemoteStorageContent.DecryptOnStorage(
            remoteFile,
            password,
            (local, remote) => DownloadFileFromStorage(
                WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remote)),
                local),
            (local, remote) => UploadFileToStorage(
                local,
                WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remote))),
            DeleteFileFromStorageEncrypted);
    }

    public bool DeleteFileFromStorage(string remoteFile)
    {
        var remoteFilePath = WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteFile));
        return DeleteResource(remoteFilePath);
    }

    public bool DeleteFileFromStorageCompressed(string remoteFile) =>
        DeleteFileFromStorage(remoteFile + ".zip");

    public bool DeleteFileFromStorageEncrypted(string remoteFile) =>
        DeleteFileFromStorage(remoteFile + ".enc");

    public bool DeleteDirectory(string remoteDirectory) =>
        DeleteResource(WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteDirectory)));

    public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget)
    {
        var sourcePath = WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteDirectorySource));
        var targetPath = WebDavClient.NormalizeRemotePath(CleanRemoteFileName(remoteDirectoryTarget));

        if (!EnsureRemoteDirectoryExists(WebDavClient.GetDirectoryPath(targetPath)))
        {
            return false;
        }

        try
        {
            Open();
            return webDavClient.MoveDirectoryAsync(sourcePath, targetPath).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error moving directory on WebDAV storage.");
            return false;
        }
    }

    public bool UploadDatabaseFile(string databaseFile) =>
        UploadFileToStorage(databaseFile, "backup.bshdb");

    public void UpdateStorageVersion(int versionId)
    {
        RemoteStorageContent.WriteVersionFile(versionId, (local, remote) => UploadFileToStorage(local, remote));
    }

    public bool IsPathTooLong(string path, bool compression, bool encryption) => false;

    public long GetFreeSpace() => 0L;

    public void Open()
    {
        if (webDavClient != null)
        {
            return;
        }

        webDavClient = new WebDavClient(
            serverAddress,
            serverPort,
            userName,
            password,
            folderPath,
            messageHandler);
        webDavClient.Open();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        webDavClient?.Dispose();
        webDavClient = null;
    }

    private bool UploadFileToStorage(string localFile, string remoteFile)
    {
        try
        {
            Open();

            var directoryPath = WebDavClient.GetDirectoryPath(remoteFile);
            if (!EnsureRemoteDirectoryExists(directoryPath))
            {
                return false;
            }

            using var localFileStream = File.OpenRead(localFile);
            return webDavClient.PutFileAsync(remoteFile, localFileStream).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error uploading file to WebDAV storage.");
            return false;
        }
    }

    private bool DownloadFileFromStorage(string remoteFile, string localFile)
    {
        try
        {
            Open();
            return webDavClient.DownloadFileAsync(remoteFile, localFile).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error downloading file from WebDAV storage.");
            return false;
        }
    }

    private bool DeleteResource(string remotePath)
    {
        try
        {
            Open();
            return webDavClient.DeleteResourceAsync(remotePath).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error deleting resource from WebDAV storage.");
            return false;
        }
    }

    private bool EnsureRemoteDirectoryExists(string remoteDirectory)
    {
        try
        {
            Open();
            return webDavClient.EnsureRemoteDirectoryExistsAsync(remoteDirectory).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error creating directory structure on WebDAV storage.");
            return false;
        }
    }
}
