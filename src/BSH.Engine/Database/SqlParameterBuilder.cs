// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Brightbits.BSH.Engine.Database;

/// <summary>
/// Builds parameterized SQL IN clauses for SQLite commands.
/// </summary>
internal static class SqlParameterBuilder
{
    public static (string Clause, List<(string, object)> Parameters) BuildInClause(string prefix, IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            throw new ArgumentException("At least one value is required for an IN clause.", nameof(values));
        }

        var parameters = new List<(string, object)>(values.Count);
        var names = new string[values.Count];
        for (var i = 0; i < values.Count; i++)
        {
            var name = prefix + i;
            names[i] = "@" + name;
            parameters.Add((name, values[i]));
        }

        return (string.Join(", ", names), parameters);
    }

    public static (string, object)[] Combine(IEnumerable<(string, object)> first, IEnumerable<(string, object)> second)
    {
        return first.Concat(second).ToArray();
    }
}
