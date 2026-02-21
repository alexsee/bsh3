// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Providers.Ports;
using Brightbits.BSH.Engine.Storage;
using NUnit.Framework;

namespace BSH.Test.Storage;

public class WebDavStorageTests
{
    [Test]
    public void Kind_ReturnsWebDav()
    {
        using var storage = CreateStorage();

        Assert.That(storage.Kind, Is.EqualTo(StorageProviderKind.WebDav));
    }

    [Test]
    public void CanWriteToStorage_ReturnsFalseBeforeOpenAndTrueAfterOpen()
    {
        using var storage = CreateStorage();

        Assert.That(storage.CanWriteToStorage(), Is.False);

        storage.Open();

        Assert.That(storage.CanWriteToStorage(), Is.True);
    }

    [Test]
    public void FileExists_ReturnsTrueWhenHeadSucceeds()
    {
        using var storage = CreateStorage();

        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method.Method == "HEAD" && NormalizeRequestPath(request) == "/folder/nested/file.txt")
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        });

        SetWebDavClient(storage, handler, TimeSpan.FromSeconds(30));

        var exists = storage.FileExists("\\nested\\file.txt");

        Assert.That(exists, Is.True);
        Assert.That(handler.Requests.Count, Is.EqualTo(1));
        Assert.That(handler.Requests[0].Method, Is.EqualTo("HEAD"));
        Assert.That(handler.Requests[0].Path, Is.EqualTo("/folder/nested/file.txt"));
    }

    [Test]
    public void FileExists_FallsBackToGetWhenHeadIsNotSupported()
    {
        using var storage = CreateStorage();

        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method.Method == "HEAD" && NormalizeRequestPath(request) == "/folder/file.txt")
            {
                return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
            }

            if (request.Method.Method == "GET" && NormalizeRequestPath(request) == "/folder/file.txt")
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        });

        SetWebDavClient(storage, handler, TimeSpan.FromSeconds(30));

        var exists = storage.FileExists("file.txt");

        Assert.That(exists, Is.True);
        Assert.That(handler.Requests.Select(r => r.Method).ToArray(), Is.EqualTo(new[] { "HEAD", "GET" }));
    }

    [Test]
    public async Task CheckMedium_ThrowsDeviceContainsWrongState_WhenVersionMismatches()
    {
        using var storage = CreateStorage(currentStorageVersion: 3);

        var handler = new TestHttpMessageHandler(request =>
        {
            var normalizedPath = NormalizeRequestPath(request);

            if (request.Method.Method == "PROPFIND" && normalizedPath == "/folder")
            {
                return new HttpResponseMessage(HttpStatusCode.MultiStatus);
            }

            if (request.Method.Method == "PUT" && normalizedPath.StartsWith("/folder/bsh.writetest.", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            if (request.Method.Method == "DELETE" && normalizedPath.StartsWith("/folder/bsh.writetest.", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (request.Method.Method == "GET" && normalizedPath.EndsWith("/backup.bshv", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("9")
                };
            }

            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        });

        var originalTimeout = TimeSpan.FromSeconds(42);
        var client = SetWebDavClient(storage, handler, originalTimeout);

        Assert.ThrowsAsync<DeviceContainsWrongStateException>(async () => await storage.CheckMedium());

        Assert.That(client.Timeout, Is.EqualTo(originalTimeout));
        Assert.That(handler.Requests.Count(r => r.Method == "PROPFIND"), Is.EqualTo(1));
        Assert.That(handler.Requests.Count(r => r.Method == "PUT"), Is.EqualTo(1));
        Assert.That(handler.Requests.Count(r => r.Method == "DELETE"), Is.EqualTo(1));
        Assert.That(handler.Requests.Count(r => r.Method == "GET"), Is.EqualTo(1));
    }

    [Test]
    public async Task CheckMedium_ReturnsTrue_WhenDirectoryWriteAndVersionAreValid()
    {
        using var storage = CreateStorage(currentStorageVersion: 3);

        var handler = new TestHttpMessageHandler(request =>
        {
            var normalizedPath = NormalizeRequestPath(request);

            if (request.Method.Method == "PROPFIND" && normalizedPath == "/folder")
            {
                return new HttpResponseMessage(HttpStatusCode.MultiStatus);
            }

            if (request.Method.Method == "PUT" && normalizedPath.StartsWith("/folder/bsh.writetest.", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            if (request.Method.Method == "DELETE" && normalizedPath.StartsWith("/folder/bsh.writetest.", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            if (request.Method.Method == "GET" && normalizedPath.EndsWith("/backup.bshv", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("3")
                };
            }

            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        });

        SetWebDavClient(storage, handler, TimeSpan.FromSeconds(35));

        var result = await storage.CheckMedium(quickCheck: true);

        Assert.That(result, Is.True);
    }

    [Test]
    public void RenameDirectory_CreatesTargetParentAndSendsMoveWithDestinationHeader()
    {
        using var storage = CreateStorage();

        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method.Method == "PROPFIND" && NormalizeRequestPath(request) == "/folder/target folder")
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (request.Method.Method == "MKCOL" && NormalizeRequestPath(request) == "/folder/target folder")
            {
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            if (request.Method.Method == "MOVE" && NormalizeRequestPath(request) == "/folder/source/path")
            {
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            throw new InvalidOperationException($"Unexpected request: {request.Method} {request.RequestUri}");
        });

        SetWebDavClient(storage, handler, TimeSpan.FromSeconds(30));

        var renamed = storage.RenameDirectory("source/path", "target folder/new name");

        Assert.That(renamed, Is.True);
        Assert.That(handler.Requests.Select(r => r.Method).ToArray(), Is.EqualTo(new[] { "PROPFIND", "MKCOL", "MOVE" }));

        var moveRequest = handler.Requests.Last();
        Assert.That(moveRequest.Destination, Is.EqualTo("https://dav.example.com/folder/target%20folder/new%20name"));
        Assert.That(moveRequest.Overwrite, Is.EqualTo("T"));
    }

    private static WebDavStorage CreateStorage(int currentStorageVersion = 1)
    {
        return new WebDavStorage(
            serverAddress: "https://dav.example.com",
            serverPort: 443,
            userName: "",
            password: "",
            folderPath: "folder",
            currentStorageVersion: currentStorageVersion);
    }

    private static HttpClient SetWebDavClient(WebDavStorage storage, TestHttpMessageHandler handler, TimeSpan timeout)
    {
        var client = new HttpClient(handler)
        {
            Timeout = timeout
        };

        var field = typeof(WebDavStorage).GetField("webDavClient", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        field?.SetValue(storage, client);

        return client;
    }

    private static string NormalizeRequestPath(HttpRequestMessage request)
    {
        var absolutePath = request.RequestUri?.AbsolutePath ?? string.Empty;
        var unescapedPath = Uri.UnescapeDataString(absolutePath);
        return unescapedPath.TrimEnd('/');
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            this.responseFactory = responseFactory;
        }

        public List<RecordedRequest> Requests { get; } = new();

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(RecordedRequest.From(request));
            return responseFactory(request);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(RecordedRequest.From(request));
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed record RecordedRequest(string Method, string Path, string Destination, string Overwrite)
    {
        public static RecordedRequest From(HttpRequestMessage request)
        {
            return new RecordedRequest(
                request.Method.Method,
                request.RequestUri?.AbsolutePath ?? string.Empty,
                GetSingleHeaderValue(request.Headers, "Destination"),
                GetSingleHeaderValue(request.Headers, "Overwrite"));
        }

        private static string GetSingleHeaderValue(HttpRequestHeaders headers, string key)
        {
            if (!headers.TryGetValues(key, out var values))
            {
                return string.Empty;
            }

            return values.FirstOrDefault() ?? string.Empty;
        }
    }
}
