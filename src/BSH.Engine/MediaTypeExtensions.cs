// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Providers.Ports;

namespace Brightbits.BSH.Engine;

public static class MediaTypeExtensions
{
    /// <summary>
    /// Combo box order used by WinForms media selectors: Local, FTP, WebDAV.
    /// </summary>
    public static MediaType FromMediaComboIndex(int index)
    {
        return index switch
        {
            0 => MediaType.LocalDevice,
            1 => MediaType.FileTransferServer,
            2 => MediaType.WebDav,
            _ => MediaType.Unset
        };
    }

    public static int ToMediaComboIndex(this MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.LocalDevice => 0,
            MediaType.FileTransferServer => 1,
            MediaType.WebDav => 2,
            _ => 0
        };
    }

    public static bool IsLocal(this MediaType mediaType) => mediaType == MediaType.LocalDevice;

    public static bool IsRemote(this MediaType mediaType) =>
        mediaType is MediaType.FileTransferServer or MediaType.WebDav;

    public static bool IsWebDav(this MediaType mediaType) => mediaType == MediaType.WebDav;

    public static bool IsFtp(this MediaType mediaType) => mediaType == MediaType.FileTransferServer;

    public static int DefaultRemotePort(this MediaType mediaType) =>
        mediaType == MediaType.WebDav ? 443 : 21;

    public static StorageProviderKind ToStorageProviderKind(this MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.FileTransferServer => StorageProviderKind.Ftp,
            MediaType.WebDav => StorageProviderKind.WebDav,
            _ => StorageProviderKind.LocalFileSystem
        };
    }

    public static MediaType ToMediaType(this StorageProviderKind kind)
    {
        return kind switch
        {
            StorageProviderKind.Ftp => MediaType.FileTransferServer,
            StorageProviderKind.WebDav => MediaType.WebDav,
            _ => MediaType.LocalDevice
        };
    }

    public static string AdjustPortForProtocol(this MediaType mediaType, string currentPort)
    {
        var defaultPort = mediaType.DefaultRemotePort().ToString();
        var otherDefault = mediaType == MediaType.WebDav ? "21" : "443";

        if (string.IsNullOrWhiteSpace(currentPort) || currentPort == otherDefault)
        {
            return defaultPort;
        }

        return currentPort;
    }
}
