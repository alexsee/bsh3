// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace BSH.Main.Utils;

public static class UiFormatUtils
{
    public const string DATE_FORMAT_HOUR_MINUTE = "HH:mm";
    public const string DATE_FORMAT_LONG = "dd.MM.yyyy HH:mm";

    public static string HumanizeDate(this DateTime date)
    {
        string formattedDate;
        if (date.Date == DateTime.Today)
        {
            formattedDate = "Heute " + date.ToShortTimeString();
        }
        else if (date.AddDays(1d) == DateTime.Today)
        {
            formattedDate = "Gestern " + date.ToShortTimeString();
        }
        else
        {
            formattedDate = date.Date.ToString("dd. MMMM yyyy ") + date.ToShortTimeString();
        }

        return formattedDate;
    }
}
