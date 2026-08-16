// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Converters;
using Humanizer;
using NUnit.Framework;

namespace BSH.Test;

public class FileSizeConverterTests
{
    private static object Convert(object value) =>
        new FileSizeConverter().Convert(value, typeof(string), parameter: null!, language: "en");

    [Test]
    public void ConvertFormatsIntLongAndDoubleIdentically()
    {
        var expected = 4096L.Bytes().ToString();

        Assert.That(Convert(4096), Is.EqualTo(expected));
        Assert.That(Convert(4096L), Is.EqualTo(expected));
        Assert.That(Convert(4096d), Is.EqualTo(expected));
    }

    [Test]
    public void ConvertPassesThroughUnsupportedTypes()
    {
        Assert.That(Convert("n/a"), Is.EqualTo("n/a"));
    }
}
