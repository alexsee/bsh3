// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Storage;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine.Storage;

public class StorageFactory : IStorageFactory
{
    private readonly IConfigurationManager configurationManager;

    public StorageFactory(IConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public IStorageProvider GetCurrentStorageProvider()
    {
        return configurationManager.MediumType switch
        {
            MediaType.FileTransferServer => new FtpStorage(configurationManager),
            MediaType.WebDav => new WebDavStorage(configurationManager),
            _ => new FileSystemStorage(configurationManager),
        };
    }

    /// <summary>
    /// Creates a remote storage provider from credentials (FTP or WebDAV).
    /// </summary>
    public static IStorageProvider CreateRemote(MediaType mediaType, RemoteStorageCredentials credentials, int currentStorageVersion = 0)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        return mediaType switch
        {
            MediaType.FileTransferServer => new FtpStorage(
                credentials.Host,
                credentials.Port > 0 ? credentials.Port : 21,
                credentials.UserName,
                credentials.Password,
                credentials.Folder,
                string.IsNullOrEmpty(credentials.Encoding) ? "ISO-8859-1" : credentials.Encoding,
                credentials.UseEncryption,
                currentStorageVersion),
            MediaType.WebDav => new WebDavStorage(credentials, currentStorageVersion),
            _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, "Expected a remote media type.")
        };
    }

    public static bool CheckRemoteConnection(MediaType mediaType, RemoteStorageCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        return mediaType switch
        {
            MediaType.FileTransferServer => FtpStorage.CheckConnection(
                credentials.Host,
                credentials.Port > 0 ? credentials.Port : 21,
                credentials.UserName,
                credentials.Password,
                credentials.Folder,
                string.IsNullOrEmpty(credentials.Encoding) ? "ISO-8859-1" : credentials.Encoding),
            MediaType.WebDav => WebDavStorage.CheckConnection(credentials),
            _ => false
        };
    }
}
