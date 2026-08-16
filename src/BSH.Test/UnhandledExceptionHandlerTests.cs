// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BSH.MainApp.Services;
using NUnit.Framework;

namespace BSH.Test;

public class UnhandledExceptionHandlerTests
{
    [Test]
    public async Task HandleAsyncLogsExceptionWithMessageAndSource()
    {
        var logged = new List<Exception>();
        var handler = CreateHandler(logged.Add, askContinue: true);
        var exception = CreateException("disk I/O failed", "BSH.MainApp.Services.JobService");

        await handler.HandleAsync(exception);

        Assert.That(logged, Has.Count.EqualTo(1));
        Assert.That(logged[0], Is.SameAs(exception));
        Assert.That(logged[0].Message, Is.EqualTo("disk I/O failed"));
        Assert.That(logged[0].Source, Is.EqualTo("BSH.MainApp.Services.JobService"));
    }

    [Test]
    public async Task HandleAsyncShowsNonCrypticContentWithMessageSourceAndStack()
    {
        string? shownContent = null;
        var handler = CreateHandler(
            _ => { },
            askContinue: true,
            onAsk: (_, content) => shownContent = content);
        var exception = CreateException("database is locked", "BSH.Engine.Database");

        await handler.HandleAsync(exception);

        Assert.That(shownContent, Is.Not.Null.And.Not.Empty);
        Assert.That(shownContent, Does.Contain("database is locked"));
        Assert.That(shownContent, Does.Contain("BSH.Engine.Database"));
        Assert.That(shownContent, Does.Contain("CreateException"));
    }

    [Test]
    public async Task HandleAsyncNullExceptionStillLogsAndShowsContent()
    {
        var logged = new List<Exception>();
        string? shownContent = null;
        var handler = CreateHandler(
            logged.Add,
            askContinue: true,
            onAsk: (_, content) => shownContent = content);

        await handler.HandleAsync(null);

        Assert.That(logged, Has.Count.EqualTo(1));
        Assert.That(shownContent, Is.Not.Empty);
    }

    [Test]
    public async Task HandleAsyncPrefersExplicitMessageWhenProvided()
    {
        string? shownContent = null;
        var handler = CreateHandler(
            _ => { },
            askContinue: true,
            onAsk: (_, content) => shownContent = content);

        await handler.HandleAsync(
            new InvalidOperationException("generic wrapper"),
            preferredMessage: "Original WinUI fault message");

        Assert.That(shownContent, Does.Contain("Original WinUI fault message"));
        Assert.That(shownContent, Does.Not.Contain("generic wrapper"));
    }

    [Test]
    public async Task HandleAsyncUnwrapsAggregateExceptionForUserContent()
    {
        string? shownContent = null;
        var handler = CreateHandler(
            _ => { },
            askContinue: true,
            onAsk: (_, content) => shownContent = content);

        await handler.HandleAsync(new AggregateException(
            new InvalidOperationException("inner disk failure"),
            new IOException("inner path missing")));

        Assert.That(shownContent, Does.Contain("inner disk failure"));
        Assert.That(shownContent, Does.Contain("inner path missing"));
        Assert.That(shownContent, Does.Not.Contain("One or more errors occurred"));
    }

    [Test]
    public async Task HandleAsyncExitsWhenUserChoosesExit()
    {
        var exited = false;
        var handler = CreateHandler(_ => { }, askContinue: false, onExit: () => exited = true);

        await handler.HandleAsync(new InvalidOperationException("boom"));

        Assert.That(exited, Is.True);
    }

    [Test]
    public async Task HandleAsyncDoesNotExitWhenUserContinues()
    {
        var exited = false;
        var handler = CreateHandler(_ => { }, askContinue: true, onExit: () => exited = true);

        await handler.HandleAsync(new InvalidOperationException("boom"));

        Assert.That(exited, Is.False);
    }

    [Test]
    public async Task HandleAsyncExitsWhenPresentationFails()
    {
        var exited = false;
        var handler = new UnhandledExceptionHandler(
            askContinueAsync: (_, _) => throw new InvalidOperationException("no UI"),
            exitApplication: () => exited = true,
            logError: _ => { });

        await handler.HandleAsync(new InvalidOperationException("boom"));

        Assert.That(exited, Is.True);
    }

    [Test]
    public void HandleTerminatingFlushesLogsWithoutRequiringExit()
    {
        var flushed = false;
        var exited = false;
        var handler = new UnhandledExceptionHandler(
            askContinueAsync: (_, _) => Task.FromResult(true),
            exitApplication: () => exited = true,
            logError: _ => { },
            flushLogs: () => flushed = true);

        handler.HandleTerminating(new InvalidOperationException("fatal"));

        Assert.That(flushed, Is.True);
        Assert.That(exited, Is.False);
    }

    private static UnhandledExceptionHandler CreateHandler(
        Action<Exception> logError,
        bool askContinue,
        Action? onExit = null,
        Action<string, string>? onAsk = null)
    {
        return new UnhandledExceptionHandler(
            askContinueAsync: (title, content) =>
            {
                onAsk?.Invoke(title, content);
                return Task.FromResult(askContinue);
            },
            exitApplication: () => onExit?.Invoke(),
            logError: logError);
    }

    private static Exception CreateException(string message, string source)
    {
        try
        {
            throw new InvalidOperationException(message) { Source = source };
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
