// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Security.Cryptography;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Security;

namespace Brightbits.BSH.Engine.Services;

/// <summary>
/// Canonical config updates for local / UNC / FTP backup targets (setup + media switch + settings).
/// </summary>
public static class MediaTargetApplier
{
    public static bool IsUncPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalized = path.Replace('/', '\\');
        return normalized.StartsWith(@"\\", StringComparison.Ordinal) && normalized.Length > 2;
    }

    /// <summary>
    /// Drive root for <see cref="Win32Stuff.GetVolumeSerial"/>, or null when UNC/short (avoids <c>[..3]</c> on UNC).
    /// </summary>
    public static string? GetVolumeSerialRoot(string path)
    {
        if (string.IsNullOrEmpty(path) || IsUncPath(path) || path.Length < 3)
        {
            return null;
        }

        return path[..3];
    }

    /// <summary>
    /// Resolves a volume serial for a local path, or empty for UNC / short / missing / "0".
    /// </summary>
    public static string ResolveVolumeSerial(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }

        var root = GetVolumeSerialRoot(path);
        if (root is null)
        {
            return "";
        }

        var serial = Win32Stuff.GetVolumeSerial(root);
        return string.IsNullOrEmpty(serial) || serial == "0" ? "" : serial;
    }

    public static string BuildLocalBackupFolder(string driveRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveRoot);

        var root = driveRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return Path.Combine(root, "Backups", Environment.MachineName, Environment.UserName);
    }

    /// <summary>
    /// Sets BackupFolder + MediaVolumeSerial and clears UNC credentials (LocalDevice).
    /// </summary>
    public static void ApplyLocalTarget(
        IConfigurationManager configurationManager,
        string? backupFolder,
        string? mediaVolumeSerial = null)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        if (string.IsNullOrWhiteSpace(backupFolder))
        {
            throw new ArgumentException("Local backup folder is required.", nameof(backupFolder));
        }

        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = backupFolder;
        configurationManager.MediaVolumeSerial = NormalizeVolumeSerial(mediaVolumeSerial);
        configurationManager.UNCUsername = "";
        configurationManager.UNCPassword = "";
    }

    /// <summary>
    /// Sets BackupFolder + UNC credentials and clears MediaVolumeSerial (LocalDevice).
    /// </summary>
    public static void ApplyUncTarget(
        IConfigurationManager configurationManager,
        string? uncPath,
        string? username,
        string? password)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        if (string.IsNullOrWhiteSpace(uncPath))
        {
            throw new ArgumentException("UNC path is required.", nameof(uncPath));
        }

        var normalized = uncPath.Replace('/', '\\');
        if (!IsUncPath(normalized))
        {
            throw new ArgumentException("Path must be a UNC path (\\\\server\\share).", nameof(uncPath));
        }

        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = normalized;
        configurationManager.MediaVolumeSerial = "";
        configurationManager.UNCUsername = username ?? "";
        configurationManager.UNCPassword = string.IsNullOrEmpty(password)
            ? ""
            : Crypto.EncryptString(password, DataProtectionScope.LocalMachine);
    }

    /// <summary>
    /// Sets FTP fields and clears local/UNC target state (FileTransferServer).
    /// </summary>
    public static void ApplyFtpTarget(
        IConfigurationManager configurationManager,
        string? host,
        string? port,
        string? user,
        string? password,
        string? folder,
        string? encoding,
        string? encryptionMode = null,
        string? sslProtocols = null)
    {
        ArgumentNullException.ThrowIfNull(configurationManager);

        configurationManager.MediumType = MediaType.FileTransferServer;
        configurationManager.BackupFolder = "";
        configurationManager.MediaVolumeSerial = "";
        configurationManager.UNCUsername = "";
        configurationManager.UNCPassword = "";
        configurationManager.FtpHost = host ?? "";
        configurationManager.FtpPort = string.IsNullOrEmpty(port) ? "21" : port;
        configurationManager.FtpUser = user ?? "";
        configurationManager.FtpPass = password ?? "";
        configurationManager.FtpFolder = folder ?? "";
        configurationManager.FtpCoding = encoding ?? "UTF8";

        if (encryptionMode is not null)
        {
            configurationManager.FtpEncryptionMode = encryptionMode;
        }

        if (sslProtocols is not null)
        {
            configurationManager.FtpSslProtocols = sslProtocols;
        }
    }

    private static string NormalizeVolumeSerial(string? mediaVolumeSerial)
    {
        return string.IsNullOrEmpty(mediaVolumeSerial) || mediaVolumeSerial == "0"
            ? ""
            : mediaVolumeSerial;
    }
}
