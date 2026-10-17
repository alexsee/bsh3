// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Globalization;
using BSH.MainApp.Converters;
using NUnit.Framework;

namespace BSH.Test;

public class BrowserDateTimeConverterTests
{
    [TestCase(0, "Today, 15:00")]
    [TestCase(-1, "Yesterday, 15:00")]
    public void RecentVersionsUseRelativeDayAndMinutePrecision(int days, string expected)
    {
        var converter = new BrowserDateTimeConverter();
        var date = DateTime.Today.AddDays(days).AddHours(15).AddSeconds(42);

        Assert.That(converter.Convert(date, typeof(string), "Version", "en-US"), Is.EqualTo(expected));
    }

    [Test]
    public void OlderVersionsUseWrittenMonthAndFilesOmitSeconds()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            var converter = new BrowserDateTimeConverter();
            var date = new DateTime(2020, 5, 30, 16, 0, 42);

            Assert.That(converter.Convert(date, typeof(string), "Version", "en-GB"), Is.EqualTo("30. May 2020 16:00"));
            Assert.That(converter.Convert(date, typeof(string), null!, "en-GB"), Is.EqualTo("30/05/2020 16:00"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
