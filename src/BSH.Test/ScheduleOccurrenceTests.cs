// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Models;
using Brightbits.BSH.Engine.Services;
using NUnit.Framework;

namespace BSH.Test;

[TestFixture]
public class ScheduleOccurrenceTests
{
    // Saturday
    private static readonly DateTime Now = new(2026, 10, 17, 12, 30, 0);

    private static ScheduleEntry Entry(ScheduleEntryKind kind, DateTime date) => new() { Type = (int)kind, Date = date };

    [Test]
    public void Once_InThePast_NeverRunsAgain()
    {
        Assert.That(ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Once, Now.AddMinutes(-1)), Now), Is.Null);
        Assert.That(ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Once, Now.AddDays(2)), Now), Is.EqualTo(Now.AddDays(2)));
    }

    [TestCase(45, "2026-10-17 12:45")]
    [TestCase(30, "2026-10-17 13:30")]
    [TestCase(10, "2026-10-17 13:10")]
    public void Hourly_RunsAtTheConfiguredMinute(int minute, string expected)
    {
        var next = ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Hourly, new DateTime(2020, 1, 1, 0, minute, 0)), Now);

        Assert.That(next, Is.EqualTo(DateTime.Parse(expected, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Test]
    public void Daily_UsesTomorrowOnceTodaysTimeHasPassed()
    {
        Assert.That(
            ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Daily, new DateTime(2020, 1, 1, 14, 0, 0)), Now),
            Is.EqualTo(new DateTime(2026, 10, 17, 14, 0, 0)));
        Assert.That(
            ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Daily, new DateTime(2020, 1, 1, 9, 0, 0)), Now),
            Is.EqualTo(new DateTime(2026, 10, 18, 9, 0, 0)));
    }

    [Test]
    public void Weekly_UsesTheWeekdayOfTheStoredDate()
    {
        // 2026-10-12 is a Monday.
        var next = ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Weekly, new DateTime(2026, 10, 12, 8, 0, 0)), Now);

        Assert.That(next, Is.EqualTo(new DateTime(2026, 10, 19, 8, 0, 0)));
    }

    [Test]
    public void Monthly_SkipsMonthsWithoutTheConfiguredDay()
    {
        var next = ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Monthly, new DateTime(2026, 1, 31, 10, 0, 0)), Now);

        Assert.That(next, Is.EqualTo(new DateTime(2026, 10, 31, 10, 0, 0)));
        Assert.That(
            ScheduleOccurrence.GetNext(Entry(ScheduleEntryKind.Monthly, new DateTime(2026, 1, 31, 10, 0, 0)), new DateTime(2026, 10, 31, 11, 0, 0)),
            Is.EqualTo(new DateTime(2026, 12, 31, 10, 0, 0)));
    }

    [Test]
    public void SeveralEntries_ReturnTheEarliest()
    {
        var next = ScheduleOccurrence.GetNext(
            [
                Entry(ScheduleEntryKind.Once, Now.AddMinutes(-5)),
                Entry(ScheduleEntryKind.Daily, new DateTime(2020, 1, 1, 20, 0, 0)),
                Entry(ScheduleEntryKind.Hourly, new DateTime(2020, 1, 1, 0, 50, 0)),
            ],
            Now);

        Assert.That(next, Is.EqualTo(new DateTime(2026, 10, 17, 12, 50, 0)));
    }
}
