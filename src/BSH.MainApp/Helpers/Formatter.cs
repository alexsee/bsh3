﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Helpers;
public static class Formatter
{
    public static string HumanizeDate(this DateTime date)
    {
        if (date == DateTime.MaxValue)
        {
            return "Nie";
        }

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
