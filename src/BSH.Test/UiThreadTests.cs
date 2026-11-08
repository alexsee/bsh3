// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BSH.MainApp;
using Microsoft.UI.Dispatching;
using NUnit.Framework;

namespace BSH.Test;

public class UiThreadTests
{
    [Test]
    public async Task UiWorkIsPreparedOnTheQueueAndNotOnTheCaller()
    {
        var controller = DispatcherQueueController.CreateOnDedicatedThread();
        var order = new List<string>();
        var done = new TaskCompletionSource();
        var caller = Environment.CurrentManagedThreadId;

        try
        {
            await UiThread.RunAsync(
                controller.DispatcherQueue,
                () => order.Add("prepare:" + Environment.CurrentManagedThreadId),
                () =>
                {
                    order.Add("work:" + Environment.CurrentManagedThreadId);
                    done.TrySetResult();
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.That(order, Has.Count.EqualTo(2));
            Assert.That(order[0], Does.StartWith("prepare:"));
            Assert.That(order[1], Does.StartWith("work:"));
            Assert.That(order[0], Is.EqualTo("prepare:" + order[1].Substring("work:".Length)));
            Assert.That(order[0], Is.Not.EqualTo("prepare:" + caller));
        }
        finally
        {
            await controller.ShutdownQueueAsync();
        }
    }

    [Test]
    public void AMissingQueueIsRejectedBeforeAnyWork()
    {
        var ran = false;
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await UiThread.RunAsync(null, () => ran = true, () => ran = true));
        Assert.That(ran, Is.False);
    }
}
