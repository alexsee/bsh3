// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Helpers;

public static class Formatter
{
    public static string HumanizeDate(this DateTime date)
    {
        if (date == DateTime.MaxValue)
        {
            return "Formatter_Never".GetLocalized();
        }

        string formattedDate;
        if (date.Date == DateTime.Today)
        {
            formattedDate = string.Format("Formatter_Today".GetLocalized(), date.ToShortTimeString());
        }
        else if (date.AddDays(1d) == DateTime.Today)
        {
            formattedDate = string.Format("Formatter_Yesterday".GetLocalized(), date.ToShortTimeString());
        }
        else
        {
            formattedDate = date.Date.ToString("Formatter_DatePrefix".GetLocalized()) + date.ToShortTimeString();
        }

        return formattedDate;
    }
}
