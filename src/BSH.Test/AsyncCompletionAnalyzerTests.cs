// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brightbits.BSH.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace BSH.Test;

public class AsyncCompletionAnalyzerTests
{
    // #654 (f52d7d2): synchronous scheduler adapters dropped ScheduleJob's task.
    [TestCase("scheduler.ScheduleJob(GetJob(action), trigger);", "BSH0001")]
    // #472 (0fcab14): these exact expressions produced Task<Task>, which was
    // stored as Task and completed before the backup/restore operation did.
    [TestCase("currentTask = Task.Factory.StartNew(async () => await backupJob.BackupAsync(cancellationToken));", "BSH0002")]
    [TestCase("currentTask = Task.Factory.StartNew(() => restoreJob.RestoreAsync(cancellationToken));", "BSH0002")]
    [TestCase("new ValueTask(Task.CompletedTask);", "BSH0001")]
    [TestCase("scheduler?.ScheduleJob(GetJob(action), trigger);", "BSH0001")]
    [TestCase("new TaskFactory<Task>().StartNew(() => Task.CompletedTask);", "BSH0001,BSH0002")]
    // Ownership may be awaited, returned, synchronously observed, or retained.
    [TestCase("await scheduler.ScheduleJob(GetJob(action), trigger);", "")]
    [TestCase("return scheduler.ScheduleJob(GetJob(action), trigger);", "")]
    [TestCase("scheduler.ScheduleJob(GetJob(action), trigger).GetAwaiter().GetResult();", "")]
    [TestCase("currentTask = scheduler.ScheduleJob(GetJob(action), trigger);", "")]
    [TestCase("_ = scheduler.ScheduleJob(GetJob(action), trigger);", "")]
    [TestCase("currentTask = Task.Run(async () => await backupJob.BackupAsync(cancellationToken));", "")]
    [TestCase("currentTask = Task.Factory.StartNew(() => 42);", "")]
    public async Task DiagnosesTaskOwnership(string statement, string expectedIds)
    {
        var isAsync = statement.StartsWith("await ", StringComparison.Ordinal);
        var body = statement.StartsWith("return ", StringComparison.Ordinal)
            ? statement : statement + " return Task.CompletedTask;";
        var source = $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            class Example
            {
                Task currentTask;
                Example scheduler, backupJob, restoreJob;
                Action action;
                object trigger;
                CancellationToken cancellationToken;
                object GetJob(Action action) => null;
                Task ScheduleJob(object job, object trigger) => Task.CompletedTask;
                Task BackupAsync(CancellationToken token) => Task.CompletedTask;
                Task RestoreAsync(CancellationToken token) => Task.CompletedTask;
                {{(isAsync ? "async Task" : "Task")}} Run()
                {
                    {{(isAsync ? statement : body)}}
                }
            }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
            .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("Example",
            new[] { CSharpSyntaxTree.ParseText(source) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.That(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "Each fixture must compile, so a compiler error cannot masquerade as enforcement.");

        var diagnostics = await compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new AsyncCompletionAnalyzer())).GetAnalyzerDiagnosticsAsync();

        Assert.That(diagnostics.Select(d => d.Id).OrderBy(id => id),
            Is.EqualTo(expectedIds.Split(',', StringSplitOptions.RemoveEmptyEntries).OrderBy(id => id)));
        Assert.That(diagnostics.All(d => d.Severity == DiagnosticSeverity.Error), Is.True);
    }
}
