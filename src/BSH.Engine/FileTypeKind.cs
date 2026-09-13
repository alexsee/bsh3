// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine;

/// <summary>
/// Storage encoding of a file version (fileversiontable.fileType).
/// The local filesystem storage writes 1/2/6, FTP storage writes 3/4/5.
/// </summary>
public enum FileTypeKind
{
    Unknown = 0,

    /// <summary>Plain copy on local filesystem storage ("Regular copy").</summary>
    RegularCopy = 1,

    /// <summary>Compressed copy on local filesystem storage.</summary>
    Compressed = 2,

    /// <summary>Plain copy on FTP storage ("Stored copy").</summary>
    StoredCopy = 3,

    /// <summary>Compressed copy on FTP storage.</summary>
    StoredCompressed = 4,

    /// <summary>Encrypted copy on FTP storage.</summary>
    StoredEncrypted = 5,

    /// <summary>Encrypted copy on local filesystem storage.</summary>
    Encrypted = 6
}

/// <summary>
/// Helpers for <see cref="FileTypeKind"/> classification.
/// </summary>
public static class FileTypeKindExtensions
{
    public static FileTypeKind ToFileTypeKind(this int value)
    {
        return Enum.IsDefined(typeof(FileTypeKind), value) ? (FileTypeKind)value : FileTypeKind.Unknown;
    }

    public static FileTypeKind ParseFileTypeKind(string value)
    {
        return int.TryParse(value, out var number) ? number.ToFileTypeKind() : FileTypeKind.Unknown;
    }

    public static bool IsPlain(this FileTypeKind kind)
    {
        return kind is FileTypeKind.RegularCopy or FileTypeKind.StoredCopy;
    }

    public static bool IsCompressed(this FileTypeKind kind)
    {
        return kind is FileTypeKind.Compressed or FileTypeKind.StoredCompressed;
    }

    public static bool IsEncrypted(this FileTypeKind kind)
    {
        return kind is FileTypeKind.StoredEncrypted or FileTypeKind.Encrypted;
    }

    /// <summary>
    /// Whether this kind stores long file names under a "_LONGFILES_" folder
    /// (applies to the local-storage kinds 1/2/6).
    /// </summary>
    public static bool UsesLongFileNameStorage(this FileTypeKind kind)
    {
        return kind is FileTypeKind.RegularCopy or FileTypeKind.Compressed or FileTypeKind.Encrypted;
    }
}
