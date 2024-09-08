// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Brightbits.BSH.Engine.Utils;

public static class DateUtils
{
    public static DateTime GetDateToWeekDay(DayOfWeek weekDay, DateTime date)
    {
        if (date.DayOfWeek == weekDay)
        {
            return date;
        }
        else
        {
            return GetDateToWeekDay(weekDay, date.Subtract(new TimeSpan(1, 0, 0, 0)));
        }
    }

    public static DateTime GetDateToMonth(int day, DateTime date)
    {
        if (date.Day == day)
        {
            return date;
        }
        else
        {
            return GetDateToMonth(day, date.Subtract(new TimeSpan(1, 0, 0, 0)));
        }
    }

    public static DateTime ReformatVersionDate(string date)
    {
        ArgumentNullException.ThrowIfNull(date);

        date = date.Replace("-", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "", StringComparison.OrdinalIgnoreCase);
        date = date.Insert(2, ".");
        date = date.Insert(5, ".");
        date = date.Insert(10, " ");
        date = date.Insert(13, ":");
        date = date.Insert(16, ":");
        return Convert.ToDateTime(date, System.Globalization.CultureInfo.CreateSpecificCulture("de-DE"));
    }
}
