// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Security;
using Serilog;

namespace Brightbits.BSH.Engine.Storage;

public class WebDavStorage : Storage, IStorage
{
    private static readonly ILogger _logger = Log.ForContext<WebDavStorage>();

    private static readonly HttpMethod PropFindMethod = new("PROPFIND");

    private static readonly HttpMethod MkColMethod = new("MKCOL");

    private static readonly HttpMethod MoveMethod = new("MOVE");

    private readonly int currentStorageVersion;

    private readonly string serverAddress;

    private readonly int serverPort;

    private readonly string userName;

    private readonly string password;

    private readonly string folderPath;

    private readonly string normalizedFolderPath;

    private HttpClient webDavClient;

    public WebDavStorage(IConfigurationManager configurationManager)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        this.serverAddress = configurationManager.FtpHost;
        this.serverPort = int.TryParse(configurationManager.FtpPort, out var parsedPort) ? parsedPort : -1;
        this.userName = configurationManager.FtpUser;
        this.password = configurationManager.FtpPass;
        this.folderPath = configurationManager.FtpFolder;
        this.normalizedFolderPath = NormalizeRemotePath(this.folderPath);
        this.currentStorageVersion = int.Parse(configurationManager.OldBackupPrevent);
    }

    public WebDavStorage(
        string serverAddress,
        int serverPort,
        string userName,
        string password,
        string folderPath,
        int currentStorageVersion)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        this.userName = userName;
        this.password = password;
        this.folderPath = folderPath;
        this.normalizedFolderPath = NormalizeRemotePath(this.folderPath);
        this.currentStorageVersion = currentStorageVersion;
    }

    public StorageProviderKind Kind => StorageProviderKind.WebDav;

    public async Task<bool> CheckMedium(bool quickCheck = false)
    {
        try
        {
            Open();

            var oldTimeout = webDavClient.Timeout;
            webDavClient.Timeout = quickCheck ? TimeSpan.FromSeconds(15) : TimeSpan.FromSeconds(60);

            try
            {
                if (!await DirectoryExistsAsync(""))
                {
                    _logger.Warning("WebDAV server does not have the specified folder.");
                    return false;
                }

                if (!await CanWriteToStorageAsync())
                {
                    _logger.Warning("WebDAV server is not writable.");
                    return false;
                }

                var versionContent = await ReadTextFileIfExistsAsync("backup.bshv");
                if (!string.IsNullOrWhiteSpace(versionContent)
                    && int.TryParse(versionContent, out var storageVersion)
                    && storageVersion != currentStorageVersion)
                {
                    _logger.Warning("WebDAV server contains an inconsistent state. Version file contains a different version than the computers backup version.");
                    throw new DeviceContainsWrongStateException();
                }

                return true;
            }
            finally
            {
                webDavClient.Timeout = oldTimeout;
            }
        }
        catch (DeviceContainsWrongStateException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception during connecting to WebDAV server.");
            return false;
        }
    }

    public bool CanWriteToStorage()
    {
        return webDavClient != null;
    }

    public bool CopyFileToStorage(string localFile, string remoteFile)
    {
        var remoteFilePath = NormalizeRemotePath(CleanRemoteFileName(remoteFile));
        var directoryPath = GetDirectoryPath(remoteFilePath);

        return EnsureRemoteDirectoryExists(directoryPath)
            && UploadFileToStorage(GetLocalFileName(localFile), remoteFilePath);
    }

    public bool CopyFileToStorageCompressed(string localFile, string remoteFile)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".zip";

        try
        {
            using (var zipFile = ZipFile.Open(tmpFile, ZipArchiveMode.Create))
            {
                zipFile.CreateEntryFromFile(GetLocalFileName(localFile), Path.GetFileName(localFile), CompressionLevel.Optimal);
            }

            return CopyFileToStorage(tmpFile, remoteFile + ".zip");
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public bool CopyFileToStorageEncrypted(string localFile, string remoteFile, string password)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";

        try
        {
            var crypto = new Encryption();
            if (!crypto.Encode(GetLocalFileName(localFile), tmpFile, password))
            {
                return false;
            }

            var file = new FileInfo(tmpFile);
            if (!file.Exists || file.Length == 0)
            {
                return false;
            }

            return CopyFileToStorage(tmpFile, remoteFile + ".enc");
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public bool CopyFileFromStorage(string localFile, string remoteFile)
    {
        var directory = Path.GetDirectoryName(localFile);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return DownloadFileFromStorage(NormalizeRemotePath(CleanRemoteFileName(remoteFile)), GetLocalFileName(localFile));
    }

    public bool CopyFileFromStorageCompressed(string localFile, string remoteFile)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".zip";

        try
        {
            if (!DownloadFileFromStorage(NormalizeRemotePath(CleanRemoteFileName(remoteFile + ".zip")), tmpFile))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(localFile);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var zipFile = ZipFile.OpenRead(tmpFile);
            zipFile.GetEntry(Path.GetFileName(localFile)).ExtractToFile(GetLocalFileName(localFile), true);

            return true;
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public bool CopyFileFromStorageEncrypted(string localFile, string remoteFile, string password)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";

        try
        {
            if (!DownloadFileFromStorage(NormalizeRemotePath(CleanRemoteFileName(remoteFile + ".enc")), tmpFile))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(localFile);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var crypto = new Encryption();
            return crypto.Decode(tmpFile, GetLocalFileName(localFile), password);
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public bool DecryptOnStorage(string remoteFile, string password)
    {
        var tmpFileEncrypted = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) + ".enc";
        var tmpFileDecrypted = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            if (!DownloadFileFromStorage(NormalizeRemotePath(CleanRemoteFileName(remoteFile + ".enc")), tmpFileEncrypted))
            {
                return false;
            }

            var crypto = new Encryption();
            if (!crypto.Decode(tmpFileEncrypted, tmpFileDecrypted, password))
            {
                return false;
            }

            if (!UploadFileToStorage(tmpFileDecrypted, NormalizeRemotePath(CleanRemoteFileName(remoteFile))))
            {
                return false;
            }

            return DeleteFileFromStorageEncrypted(remoteFile);
        }
        finally
        {
            if (File.Exists(tmpFileEncrypted))
            {
                File.Delete(tmpFileEncrypted);
            }

            if (File.Exists(tmpFileDecrypted))
            {
                File.Delete(tmpFileDecrypted);
            }
        }
    }

    public bool DeleteFileFromStorage(string remoteFile)
    {
        var remoteFilePath = NormalizeRemotePath(CleanRemoteFileName(remoteFile));
        return DeleteResource(remoteFilePath);
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
        return DeleteResource(NormalizeRemotePath(CleanRemoteFileName(remoteDirectory)));
    }

    public bool RenameDirectory(string remoteDirectorySource, string remoteDirectoryTarget)
    {
        var sourcePath = NormalizeRemotePath(CleanRemoteFileName(remoteDirectorySource));
        var targetPath = NormalizeRemotePath(CleanRemoteFileName(remoteDirectoryTarget));

        if (!EnsureRemoteDirectoryExists(GetDirectoryPath(targetPath)))
        {
            return false;
        }

        return MoveDirectory(sourcePath, targetPath);
    }

    public bool UploadDatabaseFile(string databaseFile)
    {
        return UploadFileToStorage(databaseFile, "backup.bshdb");
    }

    public void UpdateStorageVersion(int versionId)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), "backup.bshv");
        File.WriteAllText(tmpFile, versionId.ToString());

        try
        {
            UploadFileToStorage(tmpFile, "backup.bshv");
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public bool IsPathTooLong(string path, bool compression, bool encryption)
    {
        return false;
    }

    public long GetFreeSpace()
    {
        return 0L;
    }

    public void Open()
    {
        if (webDavClient != null)
        {
            return;
        }

        var handler = new HttpClientHandler();
        if (!string.IsNullOrWhiteSpace(userName))
        {
            handler.Credentials = new NetworkCredential(userName, password);
            handler.PreAuthenticate = true;
        }

        webDavClient = new HttpClient(handler, disposeHandler: true);
        webDavClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        webDavClient?.Dispose();
    }

    private async Task<bool> CanWriteToStorageAsync()
    {
        var probeFileName = "bsh.writetest." + Guid.NewGuid().ToString("N");
        var probeContent = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));

        if (!await PutFileAsync(probeFileName, probeContent))
        {
            return false;
        }

        await DeleteResourceAsync(probeFileName);
        return true;
    }

    private bool UploadFileToStorage(string localFile, string remoteFile)
    {
        try
        {
            Open();

            var directoryPath = GetDirectoryPath(remoteFile);
            if (!EnsureRemoteDirectoryExists(directoryPath))
            {
                return false;
            }

            using var localFileStream = File.OpenRead(localFile);
            return PutFileAsync(remoteFile, localFileStream).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error uploading file to WebDAV storage.");
            return false;
        }
    }

    private bool DownloadFileFromStorage(string remoteFile, string localFile)
    {
        try
        {
            Open();
            return DownloadFileAsync(remoteFile, localFile).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error downloading file from WebDAV storage.");
            return false;
        }
    }

    private bool DeleteResource(string remotePath)
    {
        try
        {
            Open();
            return DeleteResourceAsync(remotePath).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting resource from WebDAV storage.");
            return false;
        }
    }

    private bool MoveDirectory(string sourcePath, string targetPath)
    {
        try
        {
            Open();

            if (!TryBuildUri(sourcePath, out var sourceUri) || !TryBuildUri(targetPath, out var targetUri))
            {
                return false;
            }

            using var request = new HttpRequestMessage(MoveMethod, sourceUri);
            request.Headers.TryAddWithoutValidation("Destination", targetUri.AbsoluteUri);
            request.Headers.TryAddWithoutValidation("Overwrite", "T");

            using var response = webDavClient.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error moving directory on WebDAV storage.");
            return false;
        }
    }

    private bool EnsureRemoteDirectoryExists(string remoteDirectory)
    {
        try
        {
            Open();
            return EnsureRemoteDirectoryExistsAsync(remoteDirectory).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating directory structure on WebDAV storage.");
            return false;
        }
    }

    private async Task<bool> EnsureRemoteDirectoryExistsAsync(string remoteDirectory)
    {
        var normalizedPath = NormalizeRemotePath(remoteDirectory);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return true;
        }

        var currentPath = "";
        foreach (var segment in normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? segment : currentPath + "/" + segment;

            if (await DirectoryExistsAsync(currentPath))
            {
                continue;
            }

            if (!await CreateDirectoryAsync(currentPath))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> DirectoryExistsAsync(string relativeDirectory)
    {
        if (!TryBuildUri(relativeDirectory, out var directoryUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(PropFindMethod, directoryUri);
        request.Headers.TryAddWithoutValidation("Depth", "0");
        request.Content = new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\" ?><d:propfind xmlns:d=\"DAV:\"><d:prop><d:resourcetype/></d:prop></d:propfind>", Encoding.UTF8, "application/xml");

        using var response = await webDavClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        return response.StatusCode == HttpStatusCode.MultiStatus || response.IsSuccessStatusCode;
    }

    private async Task<bool> CreateDirectoryAsync(string relativeDirectory)
    {
        if (!TryBuildUri(relativeDirectory, out var directoryUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(MkColMethod, directoryUri);
        using var response = await webDavClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        // some providers return 405 when the folder already exists
        return response.StatusCode == HttpStatusCode.MethodNotAllowed;
    }

    private async Task<bool> PutFileAsync(string remoteFile, byte[] content)
    {
        using var memoryStream = new MemoryStream(content);
        return await PutFileAsync(remoteFile, memoryStream);
    }

    private async Task<bool> PutFileAsync(string remoteFile, Stream sourceStream)
    {
        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, remoteFileUri);
        request.Content = new StreamContent(sourceStream);
        using var response = await webDavClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> DownloadFileAsync(string remoteFile, string localFile)
    {
        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return false;
        }

        using var response = await webDavClient.GetAsync(remoteFileUri, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        await using var sourceStream = await response.Content.ReadAsStreamAsync();
        await using var destinationStream = File.Create(localFile);
        await sourceStream.CopyToAsync(destinationStream);

        return true;
    }

    private async Task<bool> DeleteResourceAsync(string remotePath)
    {
        if (!TryBuildUri(remotePath, out var resourceUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, resourceUri);
        using var response = await webDavClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return true;
        }

        return response.IsSuccessStatusCode;
    }

    private async Task<string> ReadTextFileIfExistsAsync(string remoteFile)
    {
        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return null;
        }

        using var response = await webDavClient.GetAsync(remoteFileUri, HttpCompletionOption.ResponseContentRead);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    private bool TryBuildUri(string remotePath, out Uri targetUri)
    {
        targetUri = null;

        if (!TryParseRootUri(out var rootUri, out var hostPath))
        {
            _logger.Warning("Invalid WebDAV host configuration: {host}", serverAddress);
            return false;
        }

        var mergedPath = CombinePath(hostPath, normalizedFolderPath);
        mergedPath = CombinePath(mergedPath, NormalizeRemotePath(remotePath));
        var encodedPath = EncodePathSegments(mergedPath);

        targetUri = string.IsNullOrEmpty(encodedPath)
            ? rootUri
            : new Uri(rootUri, encodedPath);

        return true;
    }

    private bool TryParseRootUri(out Uri rootUri, out string hostPath)
    {
        rootUri = null;
        hostPath = "";

        if (string.IsNullOrWhiteSpace(serverAddress))
        {
            return false;
        }

        var host = serverAddress.Trim();
        if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            host = "https://" + host;
        }

        if (!Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var builder = new UriBuilder(uri)
        {
            Path = "/",
            Query = ""
        };

        if (serverPort > 0 && uri.IsDefaultPort)
        {
            builder.Port = serverPort;
        }

        rootUri = builder.Uri;
        hostPath = NormalizeRemotePath(Uri.UnescapeDataString(uri.AbsolutePath));

        return true;
    }

    private static string NormalizeRemotePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        return path.Replace('\\', '/').Trim().Trim('/');
    }

    private static string GetDirectoryPath(string filePath)
    {
        var normalizedPath = NormalizeRemotePath(filePath);
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return "";
        }

        var lastIndex = normalizedPath.LastIndexOf('/');
        return lastIndex < 0 ? "" : normalizedPath[..lastIndex];
    }

    private static string CombinePath(string path1, string path2)
    {
        var first = NormalizeRemotePath(path1);
        var second = NormalizeRemotePath(path2);

        if (string.IsNullOrEmpty(first))
        {
            return second;
        }

        if (string.IsNullOrEmpty(second))
        {
            return first;
        }

        return first + "/" + second;
    }

    private static string EncodePathSegments(string path)
    {
        var normalizedPath = NormalizeRemotePath(path);
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return "";
        }

        var segments = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = Uri.EscapeDataString(segments[i]);
        }

        return string.Join("/", segments);
    }
}
