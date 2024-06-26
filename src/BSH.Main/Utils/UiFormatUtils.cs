﻿// Copyright 2022 Alexander Seeliger
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
