// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using Brightbits.BSH.Engine.Providers.Ports;
using BSH.Service.Shared;
using Serilog;
using ServiceWire.NamedPipes;

namespace Brightbits.BSH.Engine.Services;

public static class VolumeShadowCopyService
{
    private static readonly IVssClient _client = new VolumeShadowCopyClient();

    public static bool CopyFile(string fileName, string destFileName)
    {
        return _client.CopyFile(fileName, destFileName);
    }
}

public sealed class VolumeShadowCopyClient : IVssClient
{
    public bool CopyFile(string fileName, string destFileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(destFileName);

        try
        {
            using var npClient = new NpClient<IVSSRemoteObject>(new NpEndPoint("backupservicehome"));

            var serviceFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var destination = destFileName.Replace("\\\\", "\\", StringComparison.OrdinalIgnoreCase);

            // copy file
            var result = npClient.Proxy.CopyFileWithVSS(serviceFilePath, fileName, destination);

            if (!result)
            {
                Log.Error(npClient.Proxy.GetException(), "Could not copy file via VSS.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not communicate with Backup Service Home Service");
            return false;
        }
    }
}
