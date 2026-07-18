// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Security;

namespace BSH.MainApp.Services;

internal static class ToastNotificationPayload
{
    public static string Create(string title, string text)
    {
        return $"""
            <toast>
                <visual>
                    <binding template="ToastGeneric">
                        <text>{SecurityElement.Escape(title)}</text>
                        <text>{SecurityElement.Escape(text)}</text>
                    </binding>
                </visual>
            </toast>
            """;
    }
}
