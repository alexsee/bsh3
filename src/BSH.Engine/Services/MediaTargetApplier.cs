// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Security.Cryptography;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Security;

namespace Brightbits.BSH.Engine.Services;

/// <summary>
/// Canonical config updates for local/UNC backup targets (setup + media switch).
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
        configurationManager.MediumType = MediaType.LocalDevice;
        configurationManager.BackupFolder = normalized;
        configurationManager.MediaVolumeSerial = "";

        if (IsUncPath(normalized))
        {
            configurationManager.UNCUsername = username ?? "";
            configurationManager.UNCPassword = string.IsNullOrEmpty(password)
                ? ""
                : Crypto.EncryptString(password, DataProtectionScope.LocalMachine);
        }
        else
        {
            configurationManager.UNCUsername = "";
            configurationManager.UNCPassword = "";
        }
    }
}
