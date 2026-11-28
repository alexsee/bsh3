// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using Brightbits.BSH.Engine.Providers.Ports;
using BSH.Service.Shared;
using Serilog;
using ServiceWire;
using ServiceWire.NamedPipes;

namespace Brightbits.BSH.Engine.Services;

public sealed class VolumeShadowCopyClient : IVssClient
{
    private readonly string pipeName;

    public VolumeShadowCopyClient()
        : this(VssPipeNames.Stable)
    {
    }

    public VolumeShadowCopyClient(string pipeName)
    {
        this.pipeName = VssPipeNames.Normalize(pipeName);
    }

    public bool CopyFile(string fileName, string destFileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(destFileName);

        try
        {
            var endpoint = new NpEndPoint(pipeName);
            var proxy = ProxyFactory.CreateProxy<IVSSRemoteObject>(typeof(VssPipeChannel), typeof(NpEndPoint),
                endpoint, new DefaultSerializer(), new DefaultCompressor(), logger: null, stats: null);
            using var channel = (VssPipeChannel)proxy;

            var serviceFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var destination = destFileName.Replace("\\\\", "\\", StringComparison.OrdinalIgnoreCase);

            if (proxy.CopyFileWithVSS(serviceFilePath, fileName, destination))
            {
                return true;
            }

            Log.Error(proxy.GetException(), "Could not copy file via VSS.");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not communicate with Backup Service Home Service");
            return false;
        }
    }
}

// ServiceWire generates a subclass of this channel to implement IVSSRemoteObject.
public class VssPipeChannel : StreamingChannel
{
    public VssPipeChannel(Type serviceType, NpEndPoint endpoint, ISerializer serializer, ICompressor compressor,
        ILog logger, IStats stats) : base(serializer, compressor, logger, stats)
    {
        _serviceType = serviceType;
        ChannelIdentifier = new PipeChannelIdentifier(endpoint.ServerName, endpoint.PipeName);

        // PipeDirection.InOut would request GENERIC_WRITE, which includes the right to create server instances.
        var pipe = new NamedPipeClientStream(endpoint.ServerName, endpoint.PipeName,
            PipeAccessRights.ReadWrite | PipeAccessRights.Synchronize, PipeOptions.None,
            TokenImpersonationLevel.Impersonation, HandleInheritability.None);
        try
        {
            pipe.Connect(endpoint.ConnectTimeOutMs);
            _stream = new BufferedStream(pipe);
            _binReader = new BinaryReader(new BufferedStream(pipe));
            _binWriter = new BinaryWriter(_stream);
            SyncInterface(serviceType);
        }
        catch
        {
            pipe.Dispose();
            throw;
        }
    }

    protected override IChannelIdentifier ChannelIdentifier { get; }

    protected override bool SupportsConcurrentStreamIO => false;

    private sealed record PipeChannelIdentifier(string ServerName, string PipeName) : IChannelIdentifier;
}
