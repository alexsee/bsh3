// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Serilog;

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
                ? "An unexpected error occurred without exception details."
                : preferredMessage);

        logError(resolved);

        if (Interlocked.Exchange(ref isPresenting, 1) == 1)
        {
            return;
        }

        try
        {
            var continueRunning = await askContinueAsync("Unexpected error", BuildContent(resolved, preferredMessage));
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
        var source = string.IsNullOrWhiteSpace(exception.Source) ? "(unknown)" : exception.Source;
        var stackTrace = GetStackTrace(exception);

        return
            "An unexpected problem occurred. The error has been logged." +
            Environment.NewLine +
            Environment.NewLine +
            $"Message: {message}" +
            Environment.NewLine +
            $"Source: {source}" +
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

        return string.IsNullOrWhiteSpace(exception.Message) ? "(no message)" : exception.Message;
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
                    ?? "(no stack trace)";
            }
        }

        return exception.StackTrace ?? "(no stack trace)";
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
