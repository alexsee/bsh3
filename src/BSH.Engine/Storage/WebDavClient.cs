// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Brightbits.BSH.Engine.Storage;

/// <summary>
/// Low-level WebDAV HTTP client. Owns URI building and protocol verbs.
/// </summary>
public sealed class WebDavClient : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<WebDavClient>();

    private static readonly HttpMethod PropFindMethod = new("PROPFIND");
    private static readonly HttpMethod MkColMethod = new("MKCOL");
    private static readonly HttpMethod MoveMethod = new("MOVE");

    private readonly string serverAddress;
    private readonly int serverPort;
    private readonly string userName;
    private readonly string password;
    private readonly string normalizedFolderPath;
    private readonly HttpMessageHandler messageHandler;
    private readonly bool ownsHandler;

    private HttpClient httpClient;

    public WebDavClient(
        string serverAddress,
        int serverPort,
        string userName,
        string password,
        string folderPath,
        HttpMessageHandler messageHandler = null)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        this.userName = userName;
        this.password = password;
        this.normalizedFolderPath = NormalizeRemotePath(folderPath);

        if (messageHandler != null)
        {
            this.messageHandler = messageHandler;
            this.ownsHandler = false;
        }
        else
        {
            var handler = new HttpClientHandler();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                handler.Credentials = new NetworkCredential(userName, password);
                handler.PreAuthenticate = true;
            }

            this.messageHandler = handler;
            this.ownsHandler = true;
        }
    }

    public bool IsOpen => httpClient != null;

    public void Open()
    {
        if (httpClient != null)
        {
            return;
        }

        httpClient = new HttpClient(messageHandler, disposeHandler: ownsHandler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    public async Task<bool> DirectoryExistsAsync(string relativeDirectory, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(relativeDirectory, out var directoryUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(PropFindMethod, directoryUri);
        request.Headers.TryAddWithoutValidation("Depth", "0");
        request.Content = new StringContent(
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?><d:propfind xmlns:d=\"DAV:\"><d:prop><d:resourcetype/></d:prop></d:propfind>",
            Encoding.UTF8,
            "application/xml");

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        return response.StatusCode == HttpStatusCode.MultiStatus || response.IsSuccessStatusCode;
    }

    public async Task<bool> EnsureRemoteDirectoryExistsAsync(string remoteDirectory, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        var normalizedPath = NormalizeRemotePath(remoteDirectory);
        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return true;
        }

        var currentPath = "";
        foreach (var segment in normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? segment : currentPath + "/" + segment;

            if (await DirectoryExistsAsync(currentPath, cancellationToken))
            {
                continue;
            }

            if (!await CreateDirectoryAsync(currentPath, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> PutFileAsync(string remoteFile, Stream sourceStream, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, remoteFileUri)
        {
            Content = new StreamContent(sourceStream)
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PutFileAsync(string remoteFile, byte[] content, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream(content);
        return await PutFileAsync(remoteFile, memoryStream, cancellationToken);
    }

    public async Task<bool> DownloadFileAsync(string remoteFile, string localFile, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return false;
        }

        using var response = await httpClient.GetAsync(remoteFileUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destinationStream = File.Create(localFile);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        return true;
    }

    public async Task<bool> FileExistsAsync(string remoteFile, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return false;
        }

        using var headRequest = new HttpRequestMessage(HttpMethod.Head, remoteFileUri);
        using var headResponse = await httpClient.SendAsync(headRequest, cancellationToken);

        if (headResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        if (headResponse.IsSuccessStatusCode)
        {
            return true;
        }

        if (headResponse.StatusCode != HttpStatusCode.MethodNotAllowed
            && headResponse.StatusCode != HttpStatusCode.NotImplemented)
        {
            return false;
        }

        using var getResponse = await httpClient.GetAsync(remoteFileUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (getResponse.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        return getResponse.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteResourceAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(remotePath, out var resourceUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, resourceUri);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return true;
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MoveDirectoryAsync(string sourcePath, string targetPath, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(sourcePath, out var sourceUri) || !TryBuildUri(targetPath, out var targetUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(MoveMethod, sourceUri);
        request.Headers.TryAddWithoutValidation("Destination", targetUri.AbsoluteUri);
        request.Headers.TryAddWithoutValidation("Overwrite", "T");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<string> ReadTextFileIfExistsAsync(string remoteFile, CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        if (!TryBuildUri(remoteFile, out var remoteFileUri))
        {
            return null;
        }

        using var response = await httpClient.GetAsync(remoteFileUri, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound || !response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<bool> CanWriteAsync(CancellationToken cancellationToken = default)
    {
        EnsureOpen();

        var probeFileName = "bsh.writetest." + Guid.NewGuid().ToString("N");
        var probeContent = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O"));

        if (!await PutFileAsync(probeFileName, probeContent, cancellationToken))
        {
            return false;
        }

        await DeleteResourceAsync(probeFileName, cancellationToken);
        return true;
    }

    public void Dispose()
    {
        if (httpClient != null)
        {
            httpClient.Dispose();
            httpClient = null;
            return;
        }

        if (ownsHandler)
        {
            messageHandler?.Dispose();
        }
    }

    private async Task<bool> CreateDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken)
    {
        if (!TryBuildUri(relativeDirectory, out var directoryUri))
        {
            return false;
        }

        using var request = new HttpRequestMessage(MkColMethod, directoryUri);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        // Some providers return 405 when the folder already exists.
        return response.StatusCode == HttpStatusCode.MethodNotAllowed;
    }

    private void EnsureOpen()
    {
        if (httpClient == null)
        {
            throw new InvalidOperationException("WebDAV client is not open. Call Open() first.");
        }
    }

    private bool TryBuildUri(string remotePath, out Uri targetUri)
    {
        targetUri = null;

        if (!TryParseRootUri(out var rootUri, out var hostPath))
        {
            Logger.Warning("Invalid WebDAV host configuration: {host}", serverAddress);
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

    internal static string NormalizeRemotePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        return path.Replace('\\', '/').Trim().Trim('/');
    }

    internal static string GetDirectoryPath(string filePath)
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
