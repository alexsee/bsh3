// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Exceptions;
using Brightbits.BSH.Engine.Storage;
using BSH.Test.Fakes;
using NUnit.Framework;

namespace BSH.Test.Storage;

/// <summary>
/// Covers the hardened <see cref="FtpStorage"/> construction and temporary-file
/// handling without requiring a live FTP server.
/// </summary>
public class FtpStorageTests
{
    private string temporaryDirectory;

    [SetUp]
    public void SetUp()
    {
        temporaryDirectory = Path.Combine(Path.GetTempPath(), "bsh-ftp-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    [Test]
    public async Task CheckMediumValidatesDownloadedVersionMarker()
    {
        // Exercise the real FTP transport, including controls that prove validation
        // reached the downloaded marker rather than failing during connection setup.
        foreach (var marker in new[] { "7", "", "8", "corrupt", "2147483648" })
        {
            await using var server = new VersionMarkerFtpServer(marker);
            using var storage = new FtpStorage("127.0.0.1", server.Port, "user", "pass", "/backups", "UTF-8", false, 7);
            if (marker is "7" or "")
            {
                Assert.That(await storage.CheckMedium(), Is.True, marker);
            }
            else
            {
                Assert.ThrowsAsync<DeviceContainsWrongStateException>(async () => await storage.CheckMedium(), marker);
            }
            Assert.That(server.MarkerDownloaded, Is.True, "The check must reach the remote version marker.");
        }
    }

    [Test]
    public void ConstructorFallsBackToDefaultsOnInvalidConfig()
    {
        using var storage = new FtpStorage(new FakeConfigurationManager
        {
            FtpHost = "example.org",
            FtpPort = "not-a-port",
            OldBackupPrevent = "not-a-version"
        });

        Assert.That(GetPrivateField<int>(storage, "serverPort"), Is.EqualTo(21));
        Assert.That(GetPrivateField<int>(storage, "currentStorageVersion"), Is.EqualTo(0));
    }

    [Test]
    public void ConstructorHonorsValidConfig()
    {
        using var storage = new FtpStorage(new FakeConfigurationManager
        {
            FtpHost = "example.org",
            FtpPort = "2121",
            OldBackupPrevent = "7"
        });

        Assert.That(GetPrivateField<int>(storage, "serverPort"), Is.EqualTo(2121));
        Assert.That(GetPrivateField<int>(storage, "currentStorageVersion"), Is.EqualTo(7));
    }

    [Test]
    public void CreateUniqueTempFileReturnsUniqueNonExistentPaths()
    {
        var first = InvokePrivate<string>("CreateUniqueTempFile", ".zip");
        var second = InvokePrivate<string>("CreateUniqueTempFile", ".zip");

        Assert.That(first, Does.EndWith(".zip"));
        Assert.That(second, Does.EndWith(".zip"));
        Assert.That(first, Is.Not.EqualTo(second));
        Assert.That(File.Exists(first), Is.False);
        Assert.That(File.Exists(second), Is.False);
    }

    [Test]
    public void TryDeleteTempFileRemovesExistingFilesAndIgnoresMissingOnes()
    {
        var existing = Path.Combine(temporaryDirectory, "stale.tmp");
        File.WriteAllText(existing, "stale");

        Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", Path.Combine(temporaryDirectory, "missing.tmp")));
        Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", existing));
        Assert.That(File.Exists(existing), Is.False);
    }

    [Test]
    public void TryDeleteTempFileSurvivesLockedFiles()
    {
        var locked = Path.Combine(temporaryDirectory, "locked.tmp");
        File.WriteAllText(locked, "locked");

        using (new FileStream(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // deletion fails on Windows (sharing violation); elsewhere it succeeds silently
            Assert.DoesNotThrow(() => InvokePrivate<object>("TryDeleteTempFile", locked));
        }

        TryDeleteBestEffort(locked);
    }

    [Test]
    public void DisposeWithoutOpenDoesNotThrow()
    {
        using var storage = CreateUnopenedStorage();

        Assert.DoesNotThrow(() => storage.Dispose());
    }

    [Test]
    public void DisposeWithFinalizerPathIsNoOp()
    {
        using var storage = CreateUnopenedStorage();
        var dispose = typeof(FtpStorage).GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(bool)], null);
        Assert.That(dispose, Is.Not.Null);

        Assert.DoesNotThrow(() => dispose.Invoke(storage, [false]));
    }

    [Test]
    public void CopyOperationsRequireAnOpenConnection()
    {
        var source = Path.Combine(temporaryDirectory, "source.bin");
        File.WriteAllText(source, "content");
        var localTarget = Path.Combine(temporaryDirectory, "target.bin");

        using var storage = CreateUnopenedStorage();

        // ftpClient is only created in Open(); without it every operation fails fast
        // while still cleaning up its unique temporary files via finally.
        Assert.Throws<NullReferenceException>(() => storage.CopyFileToStorageCompressed(source, "remote"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileToStorageEncrypted(source, "remote", "password"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileFromStorageCompressed(localTarget, "remote"));
        Assert.Throws<NullReferenceException>(() => storage.CopyFileFromStorageEncrypted(localTarget, "remote", "password"));
    }

    private static FtpStorage CreateUnopenedStorage()
    {
        return new FtpStorage("example.org", 21, "user", "pass", "/backups", "UTF-8", false, 0);
    }

    private static T GetPrivateField<T>(FtpStorage storage, string name)
    {
        var field = typeof(FtpStorage).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null);
        return (T)field.GetValue(storage);
    }

    private static T InvokePrivate<T>(string name, params object[] args)
    {
        var method = typeof(FtpStorage).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        return (T)method.Invoke(null, args);
    }

    private static void TryDeleteBestEffort(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignore; teardown removes the directory
        }
    }

    private sealed class VersionMarkerFtpServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
        private readonly Task session;
        private readonly byte[] marker;

        public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;
        public bool MarkerDownloaded { get; private set; }

        public VersionMarkerFtpServer(string marker)
        {
            this.marker = Encoding.UTF8.GetBytes(marker);
            listener.Start();
            session = RunAsync();
        }

        private async Task RunAsync()
        {
            using var client = await listener.AcceptTcpClientAsync(cancellation.Token);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\r\n" };
            TcpListener dataListener = null;
            try
            {
                await writer.WriteLineAsync("220 Version marker fixture");
                while (await reader.ReadLineAsync(cancellation.Token) is { } command)
                {
                    switch (command.Split(' ')[0])
                    {
                        case "USER": await writer.WriteLineAsync("331 Password required"); break;
                        case "PASS": await writer.WriteLineAsync("230 Logged in"); break;
                        case "SYST": await writer.WriteLineAsync("215 UNIX Type: L8"); break;
                        case "FEAT": await writer.WriteLineAsync("211 No extensions"); break;
                        case "PWD": await writer.WriteLineAsync("257 \"/\""); break;
                        case "CWD": await writer.WriteLineAsync("250 Directory changed"); break;
                        case "TYPE": await writer.WriteLineAsync("200 OK"); break;
                        case "SIZE": await writer.WriteLineAsync($"213 {marker.Length}"); break;
                        case "MDTM": await writer.WriteLineAsync("213 20260101000000"); break;
                        case "EPSV":
                        case "PASV":
                            dataListener?.Stop();
                            dataListener = new TcpListener(IPAddress.Loopback, 0);
                            dataListener.Start();
                            var port = ((IPEndPoint)dataListener.LocalEndpoint).Port;
                            await writer.WriteLineAsync(command.StartsWith("EPSV", StringComparison.Ordinal)
                                ? $"229 Entering Extended Passive Mode (|||{port}|)"
                                : $"227 Entering Passive Mode (127,0,0,1,{port / 256},{port % 256})");
                            break;
                        case "NLST":
                        case "RETR":
                            await writer.WriteLineAsync("150 Opening data connection");
                            using (var dataClient = await dataListener.AcceptTcpClientAsync(cancellation.Token))
                            {
                                var payload = command.StartsWith("NLST", StringComparison.Ordinal)
                                    ? Encoding.ASCII.GetBytes("/backups/backup.bshv\r\n")
                                    : marker;
                                await dataClient.GetStream().WriteAsync(payload, cancellation.Token);
                            }
                            MarkerDownloaded |= command.StartsWith("RETR", StringComparison.Ordinal);
                            await writer.WriteLineAsync("226 Transfer complete");
                            break;
                        case "QUIT": await writer.WriteLineAsync("221 Goodbye"); return;
                        default: await writer.WriteLineAsync("502 Command not implemented"); break;
                    }
                }
            }
            finally
            {
                dataListener?.Stop();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await cancellation.CancelAsync();
            listener.Stop();
            try { await session; }
            catch (OperationCanceledException) { }
            cancellation.Dispose();
        }
    }
}
