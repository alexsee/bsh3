// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Brightbits.BSH.Engine.Contracts.Database;
using Brightbits.BSH.Engine.Contracts.Repo;
using Brightbits.BSH.Engine.Database;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine.Repo;

public class ScheduleRepository : IScheduleRepository
{
    private readonly IDbClientFactory dbClientFactory;

    public ScheduleRepository(IDbClientFactory dbClientFactory)
    {
        this.dbClientFactory = dbClientFactory;
    }

    public async Task<IReadOnlyList<ScheduleEntry>> GetSchedulesAsync()
    {
        var result = new List<ScheduleEntry>();

        using var dbClient = dbClientFactory.CreateDbClient();
        using var reader = await dbClient.ExecuteDataReaderAsync(CommandType.Text, "SELECT * FROM schedule", null);

        while (await reader.ReadAsync())
        {
            result.Add(new ScheduleEntry()
            {
                Type = reader.GetInt32("timType"),
                Date = reader.GetDateTimeParsed("timDate")
            });
        }

        await reader.CloseAsync();
        return result;
    }

    public async Task ReplaceSchedulesAsync(IEnumerable<ScheduleEntry> schedules)
    {
        using var dbClient = dbClientFactory.CreateDbClient();
        dbClient.BeginTransaction();

        await dbClient.ExecuteNonQueryAsync("DELETE FROM schedule");

        foreach (var schedule in schedules)
        {
            var parameters = new (string, object)[]
            {
                ("timType", schedule.Type),
                ("timDate", schedule.Date.ToString("dd.MM.yyyy HH:mm:ss"))
            };

            await dbClient.ExecuteNonQueryAsync(
                CommandType.Text,
                "INSERT INTO schedule ( timType, timDate) VALUES ( @timType, @timDate )",
                parameters);
        }

        dbClient.CommitTransaction();
    }
}
