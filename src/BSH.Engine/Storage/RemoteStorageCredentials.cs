// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Contracts;

namespace Brightbits.BSH.Engine.Storage;

/// <summary>
/// Protocol-agnostic remote storage credentials.
/// Persisted via the legacy Ftp* configuration keys for backward compatibility.
/// </summary>
public sealed class RemoteStorageCredentials
{
    public string Host { get; init; } = "";

    public int Port { get; init; } = -1;

    public string UserName { get; init; } = "";

    public string Password { get; init; } = "";

    public string Folder { get; init; } = "";

    /// <summary>FTP-only character encoding; unused for WebDAV.</summary>
    public string Encoding { get; init; } = "ISO-8859-1";

    /// <summary>FTP-only TLS mode; unused for WebDAV.</summary>
    public bool UseEncryption { get; init; }

    public static RemoteStorageCredentials FromConfiguration(IConfigurationManager configurationManager)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        return new RemoteStorageCredentials
        {
            Host = configurationManager.FtpHost ?? "",
            Port = int.TryParse(configurationManager.FtpPort, out var port) ? port : -1,
            UserName = configurationManager.FtpUser ?? "",
            Password = configurationManager.FtpPass ?? "",
            Folder = configurationManager.FtpFolder ?? "",
            Encoding = string.IsNullOrEmpty(configurationManager.FtpCoding)
                ? "ISO-8859-1"
                : configurationManager.FtpCoding,
            UseEncryption = configurationManager.FtpEncryptionMode == "3"
        };
    }

    public void ApplyTo(IConfigurationManager configurationManager, MediaType mediaType)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        configurationManager.MediumType = mediaType;
        configurationManager.BackupFolder = "";
        configurationManager.FtpHost = Host ?? "";
        configurationManager.FtpPort = Port > 0 ? Port.ToString() : mediaType.DefaultRemotePort().ToString();
        configurationManager.FtpUser = UserName ?? "";
        configurationManager.FtpPass = Password ?? "";
        configurationManager.FtpFolder = Folder ?? "";

        if (mediaType.IsWebDav())
        {
            // Clear FTP-only knobs so they cannot linger across protocol switches.
            configurationManager.FtpCoding = "";
            configurationManager.FtpEncryptionMode = "0";
            configurationManager.FtpSslProtocols = "0";
        }
        else if (mediaType.IsFtp())
        {
            configurationManager.FtpCoding = string.IsNullOrEmpty(Encoding) ? "ISO-8859-1" : Encoding;
            configurationManager.FtpEncryptionMode = UseEncryption ? "3" : "0";
            configurationManager.FtpSslProtocols = "0";
        }
    }
}
