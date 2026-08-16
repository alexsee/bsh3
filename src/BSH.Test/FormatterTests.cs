// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Helpers;
using NUnit.Framework;

namespace BSH.Test;

public class FormatterTests
{
    [Test]
    public void ShortenPathMiddleReturnsEmptyForNull()
    {
        Assert.That(Formatter.ShortenPathMiddle(null, 20), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ShortenPathMiddleKeepsShortPathsIntact()
    {
        const string path = @"C:\docs\file.txt";
        Assert.That(Formatter.ShortenPathMiddle(path, 70), Is.EqualTo(path));
    }

    [Test]
    public void ShortenPathMiddleKeepsFileNameAndInsertsEllipsis()
    {
        var path = @"C:\Users\alex\Documents\Projects\Backup\very-long-folder\report.pdf";
        var shortened = Formatter.ShortenPathMiddle(path, 40);

        Assert.That(shortened, Does.StartWith(@"C:\"));
        Assert.That(shortened, Does.Contain("..."));
        Assert.That(shortened, Does.EndWith("report.pdf"));
        Assert.That(shortened.Length, Is.EqualTo(40));
    }
}
