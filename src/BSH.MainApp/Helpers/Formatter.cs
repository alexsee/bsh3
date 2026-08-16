// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.WinUI;

namespace BSH.MainApp.Helpers;

public static class Formatter
{
    public static string HumanizeDate(this DateTime date)
    {
        if (date == DateTime.MaxValue)
        {
            return "Formatter_Never".GetLocalized() ?? "Formatter_Never";
        }

        string formattedDate;
        if (date.Date == DateTime.Today)
        {
            formattedDate = string.Format("Formatter_Today".GetLocalized() ?? "Formatter_Today", date.ToShortTimeString());
        }
        else if (date.AddDays(1d) == DateTime.Today)
        {
            formattedDate = string.Format("Formatter_Yesterday".GetLocalized() ?? "Formatter_Yesterday", date.ToShortTimeString());
        }
        else
        {
            formattedDate = date.Date.ToString("Formatter_DatePrefix".GetLocalized() ?? "Formatter_DatePrefix") + date.ToShortTimeString();
        }

        return formattedDate;
    }
}
