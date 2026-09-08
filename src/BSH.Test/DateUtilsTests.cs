// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Utils;
using NUnit.Framework;

namespace BSH.Test;

public class DateUtilsTests
{
    [Test]
    public void GetDateToWeekDayReturnsSameDateWhenAlreadyOnRequestedDay()
    {
        var wednesday = new DateTime(2026, 6, 17, 23, 15, 30);

        var result = DateUtils.GetDateToWeekDay(DayOfWeek.Wednesday, wednesday);

        Assert.That(result, Is.EqualTo(wednesday));
    }

    [Test]
    public void GetDateToWeekDayWalksBackToMostRecentMatchingWeekday()
    {
        var wednesday = new DateTime(2026, 6, 17, 23, 15, 30);

        var result = DateUtils.GetDateToWeekDay(DayOfWeek.Monday, wednesday);

        Assert.That(result, Is.EqualTo(new DateTime(2026, 6, 15, 23, 15, 30)));
    }

    [Test]
    public void GetDateToMonthReturnsSameDateWhenDayAlreadyMatches()
    {
        var date = new DateTime(2026, 3, 15, 8, 0, 0);

        var result = DateUtils.GetDateToMonth(15, date);

        Assert.That(result, Is.EqualTo(date));
    }

    [Test]
    public void GetDateToMonthUsesPreviousMonthWhenRequestedDayIsLater()
    {
        var date = new DateTime(2026, 3, 15, 8, 0, 0);

        var result = DateUtils.GetDateToMonth(20, date);

        Assert.That(result, Is.EqualTo(new DateTime(2026, 2, 20, 8, 0, 0)));
    }

    [Test]
    public void GetDateToMonthSkipsMonthsThatDoNotContainRequestedDay()
    {
        var date = new DateTime(2026, 3, 15, 8, 0, 0);

        var result = DateUtils.GetDateToMonth(31, date);

        Assert.That(result, Is.EqualTo(new DateTime(2026, 1, 31, 8, 0, 0)));
    }

    [Test]
    public void ReformatVersionDateParsesBackupVersionStamp()
    {
        var result = DateUtils.ReformatVersionDate("01-01-2021 00-00-00");

        Assert.That(result, Is.EqualTo(new DateTime(2021, 1, 1, 0, 0, 0)));
    }
}
