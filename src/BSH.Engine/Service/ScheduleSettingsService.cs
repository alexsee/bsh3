// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Config;
using Brightbits.BSH.Engine.Repo.Contracts;
using Brightbits.BSH.Engine.Types;

namespace Brightbits.BSH.Engine.Service;

public sealed class ScheduleSettingsService
{
    private readonly IConfigurationManager configurationManager;
    private readonly IScheduleRepository scheduleRepository;

    public ScheduleSettingsService(
        IConfigurationManager configurationManager,
        IScheduleRepository scheduleRepository)
    {
        this.configurationManager = configurationManager;
        this.scheduleRepository = scheduleRepository;
    }

    public async Task<ScheduleSettings> LoadAsync()
    {
        var settings = LoadPolicySettings();
        settings.Entries.AddRange(await scheduleRepository.GetSchedulesAsync());
        settings.PerformMissedBackupsLater = configurationManager.DoPastBackups == "1";

        return settings;
    }

    public SchedulePolicy LoadPolicy()
    {
        return new SchedulePolicy(LoadPolicySettings());
    }

    public async Task SaveAsync(ScheduleSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Persist retention/full-backup/missed-backup config before replacing schedule
        // rows so a later schedule-table failure cannot leave new entries paired with
        // stale destructive policy settings (#535).
        configurationManager.IntervallDelete = BuildRetentionConfig(settings);
        configurationManager.IntervallAutoHourBackups = Math.Max(1, settings.AutomaticHourlyBackupThreshold).ToString(CultureInfo.InvariantCulture);
        configurationManager.ScheduleFullBackup = settings.EnableScheduledFullBackups
            ? "day|" + Math.Max(1, settings.ScheduledFullBackupDays).ToString(CultureInfo.InvariantCulture)
            : string.Empty;
        configurationManager.DoPastBackups = settings.PerformMissedBackupsLater ? "1" : "0";

        await scheduleRepository.ReplaceSchedulesAsync(settings.Entries);
    }

    private void LoadRetention(ScheduleSettings settings)
    {
        var interval = configurationManager.IntervallDelete ?? string.Empty;
        if (string.IsNullOrEmpty(interval))
        {
            settings.RetentionMode = ScheduleRetentionMode.None;
            return;
        }

        if (interval == "auto")
        {
            settings.RetentionMode = ScheduleRetentionMode.Automatic;
            return;
        }

        var parts = interval.Split('|');
        if (parts.Length != 2
            || !TryParseRetentionUnit(parts[0], out var unit)
            || !TryParsePositiveInt(parts[1], out var retentionInterval))
        {
            settings.RetentionMode = ScheduleRetentionMode.None;
            return;
        }

        settings.RetentionMode = ScheduleRetentionMode.Interval;
        settings.RetentionInterval = retentionInterval;
        settings.RetentionIntervalUnit = unit;
    }

    private ScheduleSettings LoadPolicySettings()
    {
        var settings = new ScheduleSettings();
        LoadRetention(settings);
        settings.AutomaticHourlyBackupThreshold = ParsePositiveInt(configurationManager.IntervallAutoHourBackups, 24);
        LoadScheduledFullBackup(settings);
        return settings;
    }

    private void LoadScheduledFullBackup(ScheduleSettings settings)
    {
        var fullBackup = configurationManager.ScheduleFullBackup ?? string.Empty;
        if (string.IsNullOrEmpty(fullBackup))
        {
            settings.EnableScheduledFullBackups = false;
            return;
        }

        var parts = fullBackup.Split('|');
        if (parts.Length != 2 || parts[0] != "day" || !TryParsePositiveInt(parts[1], out var intervalDays))
        {
            settings.EnableScheduledFullBackups = false;
            return;
        }

        settings.EnableScheduledFullBackups = true;
        settings.ScheduledFullBackupDays = intervalDays;
    }

    private static string BuildRetentionConfig(ScheduleSettings settings)
    {
        return settings.RetentionMode switch
        {
            ScheduleRetentionMode.Automatic => "auto",
            ScheduleRetentionMode.Interval => $"{FormatRetentionUnit(settings.RetentionIntervalUnit)}|{Math.Max(1, settings.RetentionInterval).ToString(CultureInfo.InvariantCulture)}",
            _ => string.Empty,
        };
    }

    private static bool TryParseRetentionUnit(string value, out ScheduleRetentionIntervalUnit unit)
    {
        unit = value switch
        {
            "hour" => ScheduleRetentionIntervalUnit.Hour,
            "day" => ScheduleRetentionIntervalUnit.Day,
            "week" => ScheduleRetentionIntervalUnit.Week,
            _ => ScheduleRetentionIntervalUnit.Day,
        };

        return value is "hour" or "day" or "week";
    }

    private static string FormatRetentionUnit(ScheduleRetentionIntervalUnit unit)
    {
        return unit switch
        {
            ScheduleRetentionIntervalUnit.Hour => "hour",
            ScheduleRetentionIntervalUnit.Week => "week",
            _ => "day",
        };
    }

    private static int ParsePositiveInt(string value, int fallback)
    {
        return TryParsePositiveInt(value, out var parsed)
            ? parsed
            : fallback;
    }

    private static bool TryParsePositiveInt(string value, out int parsed)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed > 0;
    }
}
