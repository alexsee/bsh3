// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Serilog;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.Services;

/// <summary>
/// Logs unhandled exceptions, shows a Continue/Exit prompt, and exits when needed.
/// </summary>
public sealed class UnhandledExceptionHandler
{
    private readonly Func<string, string, Task<bool>> askContinueAsync;
    private readonly Action exitApplication;
    private readonly Action<Exception> logError;
    private readonly Action flushLogs;
    private int isPresenting;

    public UnhandledExceptionHandler(
        Func<string, string, Task<bool>> askContinueAsync,
        Action exitApplication,
        Action<Exception>? logError = null,
        Action? flushLogs = null)
    {
        this.askContinueAsync = askContinueAsync;
        this.exitApplication = exitApplication;
        this.logError = logError ?? DefaultLog;
        this.flushLogs = flushLogs ?? Log.CloseAndFlush;
    }

    public Task HandleAsync(Exception? exception, string? preferredMessage = null) =>
        PresentAsync(exception, preferredMessage, exitIfShowFails: true);

    public void HandleTerminating(Exception? exception)
    {
        try
        {
            PresentAsync(exception, preferredMessage: null, exitIfShowFails: false)
                .GetAwaiter()
                .GetResult();
        }
        catch
        {
            // Best-effort surface during process teardown.
        }
        finally
        {
            flushLogs();
        }
    }

    private async Task PresentAsync(Exception? exception, string? preferredMessage, bool exitIfShowFails)
    {
        var resolved = exception ?? new InvalidOperationException(
            string.IsNullOrWhiteSpace(preferredMessage)
                ? "Unhandled_NoExceptionDetails".GetLocalized()
                : preferredMessage);

        logError(resolved);

        if (Interlocked.Exchange(ref isPresenting, 1) == 1)
        {
            return;
        }

        try
        {
            var continueRunning = await askContinueAsync(
                "Unhandled_Title".GetLocalized(),
                BuildContent(resolved, preferredMessage));
            if (!continueRunning)
            {
                exitApplication();
            }
        }
        catch
        {
            if (exitIfShowFails)
            {
                exitApplication();
            }
        }
        finally
        {
            Interlocked.Exchange(ref isPresenting, 0);
        }
    }

    private static string BuildContent(Exception exception, string? preferredMessage)
    {
        var message = !string.IsNullOrWhiteSpace(preferredMessage)
            ? preferredMessage
            : GetMessage(exception);
        var source = string.IsNullOrWhiteSpace(exception.Source)
            ? "Unhandled_Unknown".GetLocalized()
            : exception.Source;
        var stackTrace = GetStackTrace(exception);

        return
            "Unhandled_BodyIntro".GetLocalized() +
            Environment.NewLine +
            Environment.NewLine +
            "Unhandled_MessageLabel".GetLocalized() + " " + message +
            Environment.NewLine +
            "Unhandled_SourceLabel".GetLocalized() + " " + source +
            Environment.NewLine +
            Environment.NewLine +
            stackTrace;
    }

    private static string GetMessage(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var innerMessages = aggregate.Flatten().InnerExceptions
                .Select(inner => inner.Message)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToArray();

            if (innerMessages.Length > 0)
            {
                return string.Join(Environment.NewLine, innerMessages);
            }
        }

        return string.IsNullOrWhiteSpace(exception.Message)
            ? "Unhandled_NoMessage".GetLocalized()
            : exception.Message;
    }

    private static string GetStackTrace(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var flattened = aggregate.Flatten();
            if (flattened.InnerExceptions.Count == 1)
            {
                return flattened.InnerExceptions[0].StackTrace
                    ?? exception.StackTrace
                    ?? "Unhandled_NoStackTrace".GetLocalized();
            }
        }

        return exception.StackTrace ?? "Unhandled_NoStackTrace".GetLocalized();
    }

    private static void DefaultLog(Exception exception)
    {
        Log.Error(
            exception,
            "An unexpected error occurred. Source: {Source}. Message: {Message}",
            string.IsNullOrWhiteSpace(exception.Source) ? "(unknown)" : exception.Source,
            exception.Message);
    }
}
