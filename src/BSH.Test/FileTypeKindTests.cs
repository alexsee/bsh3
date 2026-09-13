// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using NUnit.Framework;

namespace BSH.Test;

/// <summary>
/// Covers the shared <see cref="FileTypeKind"/> storage-type classification.
/// </summary>
public class FileTypeKindTests
{
    [TestCase(1, FileTypeKind.RegularCopy)]
    [TestCase(2, FileTypeKind.Compressed)]
    [TestCase(3, FileTypeKind.StoredCopy)]
    [TestCase(4, FileTypeKind.StoredCompressed)]
    [TestCase(5, FileTypeKind.StoredEncrypted)]
    [TestCase(6, FileTypeKind.Encrypted)]
    [TestCase(0, FileTypeKind.Unknown)]
    [TestCase(99, FileTypeKind.Unknown)]
    [TestCase(-1, FileTypeKind.Unknown)]
    public void ToFileTypeKindMapsKnownValues(int value, FileTypeKind expected)
    {
        Assert.That(value.ToFileTypeKind(), Is.EqualTo(expected));
    }

    [TestCase("1", FileTypeKind.RegularCopy)]
    [TestCase("6", FileTypeKind.Encrypted)]
    [TestCase("", FileTypeKind.Unknown)]
    [TestCase(null, FileTypeKind.Unknown)]
    [TestCase("abc", FileTypeKind.Unknown)]
    [TestCase("99", FileTypeKind.Unknown)]
    public void ToFileTypeKindHandlesStrings(string? value, FileTypeKind expected)
    {
        Assert.That(value!.ToFileTypeKind(), Is.EqualTo(expected));
    }

    [Test]
    public void UsesLongFileNameStorageIdentifiesLocalStorageKinds()
    {
        Assert.That(FileTypeKind.RegularCopy.UsesLongFileNameStorage(), Is.True);
        Assert.That(FileTypeKind.Compressed.UsesLongFileNameStorage(), Is.True);
        Assert.That(FileTypeKind.Encrypted.UsesLongFileNameStorage(), Is.True);
        Assert.That(FileTypeKind.StoredCopy.UsesLongFileNameStorage(), Is.False);
        Assert.That(FileTypeKind.Unknown.UsesLongFileNameStorage(), Is.False);
    }
}
