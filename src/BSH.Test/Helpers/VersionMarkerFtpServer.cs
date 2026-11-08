// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BSH.Test.Helpers;

internal sealed class VersionMarkerFtpServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
    private readonly Task session;
    private readonly byte[] marker;
    private readonly bool rejectUploads;

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;
    public bool MarkerDownloaded { get; private set; }
    public bool UploadReceived { get; private set; }

    public VersionMarkerFtpServer(string marker, bool rejectUploads = false)
        : this(Encoding.UTF8.GetBytes(marker), rejectUploads)
    {
    }

    public VersionMarkerFtpServer(byte[] marker, bool rejectUploads = false)
    {
        this.marker = marker;
        this.rejectUploads = rejectUploads;
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
                    case "DELE": await writer.WriteLineAsync("250 File deleted"); break;
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
                    case "STOR":
                        await writer.WriteLineAsync("150 Opening data connection");
                        using (var uploadClient = await dataListener.AcceptTcpClientAsync(cancellation.Token))
                        {
                            await uploadClient.GetStream().CopyToAsync(Stream.Null, cancellation.Token);
                        }
                        UploadReceived = true;
                        await writer.WriteLineAsync(rejectUploads ? "550 Upload rejected" : "226 Transfer complete");
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
