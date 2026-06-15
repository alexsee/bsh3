// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Services;

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

        await scheduleRepository.ReplaceSchedulesAsync(settings.Entries);
        configurationManager.IntervallDelete = BuildRetentionConfig(settings);
        configurationManager.IntervallAutoHourBackups = Math.Max(1, settings.AutomaticHourlyBackupThreshold).ToString(CultureInfo.InvariantCulture);
        configurationManager.ScheduleFullBackup = settings.EnableScheduledFullBackups
            ? "day|" + Math.Max(1, settings.ScheduledFullBackupDays).ToString(CultureInfo.InvariantCulture)
            : string.Empty;
        configurationManager.DoPastBackups = settings.PerformMissedBackupsLater ? "1" : "0";
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
        settings.RetentionMode = ScheduleRetentionMode.Interval;
        settings.RetentionInterval = parts.Length > 1 ? ParsePositiveInt(parts[1], 1) : 1;
        settings.RetentionIntervalUnit = parts.Length > 0
            ? ParseRetentionUnit(parts[0])
            : ScheduleRetentionIntervalUnit.Day;
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
        settings.EnableScheduledFullBackups = parts.Length > 0 && parts[0] == "day";
        settings.ScheduledFullBackupDays = parts.Length > 1 ? ParsePositiveInt(parts[1], 1) : 1;
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

    private static ScheduleRetentionIntervalUnit ParseRetentionUnit(string unit)
    {
        return unit switch
        {
            "hour" => ScheduleRetentionIntervalUnit.Hour,
            "week" => ScheduleRetentionIntervalUnit.Week,
            _ => ScheduleRetentionIntervalUnit.Day,
        };
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
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : fallback;
    }
}
