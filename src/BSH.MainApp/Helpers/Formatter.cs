// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.WinUI;
using Serilog;

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

    /// <summary>
    /// Shortens a file path so it fits within <paramref name="maxLength"/> characters, keeping the
    /// file name intact and inserting an ellipsis between the path and the file name when possible.
    /// </summary>
    public static string ShortenPathMiddle(string? path, int maxLength)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
        {
            return path ?? string.Empty;
        }

        const string ellipsis = "...";

        var fileName = Path.GetFileName(path);
        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            return MiddleTruncate(path, maxLength, ellipsis);
        }

        // Reserve room for the file name, the ellipsis and the separator before it.
        var reserved = fileName.Length + ellipsis.Length + 1;
        if (reserved >= maxLength)
        {
            // The file name alone does not fit; truncate the whole path in the middle.
            return MiddleTruncate(path, maxLength, ellipsis);
        }

        var headLength = maxLength - reserved;
        var head = directory.Length > headLength ? directory.Substring(0, headLength) : directory;

        return head + ellipsis + Path.DirectorySeparatorChar + fileName;
    }

    /// <summary>
    /// Shortens a file path like <see cref="ShortenPathMiddle"/> but never throws; on failure it
    /// logs a warning (when a logger is supplied) and falls back to the original path.
    /// </summary>
    public static string ShortenPathMiddleSafe(string? path, int maxLength, ILogger? logger = null)
    {
        try
        {
            return ShortenPathMiddle(path, maxLength);
        }
        catch (Exception ex)
        {
            logger?.Warning(ex, "Could not shorten file path.");
            return path ?? string.Empty;
        }
    }

    private static string MiddleTruncate(string value, int maxLength, string ellipsis)
    {
        if (maxLength <= ellipsis.Length)
        {
            return value.Substring(0, Math.Max(0, maxLength));
        }

        var keep = maxLength - ellipsis.Length;
        var front = (int)Math.Ceiling(keep / 2.0);
        var back = keep - front;

        return value.Substring(0, front) + ellipsis + value.Substring(value.Length - back);
    }
}
