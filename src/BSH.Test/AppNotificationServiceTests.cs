// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BSH.MainApp;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Notifications;
using BSH.MainApp.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using NUnit.Framework;

namespace BSH.Test;

public class AppNotificationServiceTests
{
    [Test]
    public void ShowDoesNothingBeforeRegistration()
    {
        var service = new AppNotificationService(new UnusedNavigation());

        Assert.That(service.Show("<toast/>"), Is.False);
        service.Dispose();
    }

    [Test]
    public void InitializeDoesNotThrowWhenThePlatformRejectsRegistration()
    {
        var service = new AppNotificationService(new UnusedNavigation());

        Assert.DoesNotThrow(() => service.Initialize());
        service.Show("<toast/>");

        try
        {
            service.Dispose();
        }
        catch (Exception)
        {
            // The test host has no packaged toast registration.
        }
    }

    [Test]
    public void RegisteredNotificationsCanBeShownAndCleared()
    {
        var shown = 0;
        var unregistered = 0;
        var service = new AppNotificationService(
            new UnusedNavigation(),
            register: () => { },
            detach: () => { },
            showPayload: _ =>
            {
                shown++;
                return true;
            },
            unregister: () => unregistered++);

        service.Initialize();
        Assert.That(service.Show("<toast/>"), Is.True);
        Assert.That(shown, Is.EqualTo(1));

        service.Dispose();
        service.Dispose();
        Assert.That(unregistered, Is.EqualTo(1));
    }

    [Test]
    public void RegistrationFailureDetachesAndStaysSilent()
    {
        var detached = 0;
        var service = new AppNotificationService(
            new UnusedNavigation(),
            register: () => throw new InvalidOperationException("register"),
            detach: () =>
            {
                detached++;
                throw new InvalidOperationException("detach");
            },
            showPayload: _ => true);

        Assert.DoesNotThrow(() => service.Initialize());
        Assert.That(detached, Is.EqualTo(1));
        Assert.That(service.Show("<toast/>"), Is.False);
        service.Dispose();
    }

    [Test]
    public void AFailedToastDoesNotEscape()
    {
        var service = new AppNotificationService(
            new UnusedNavigation(),
            register: () => { },
            detach: () => { },
            showPayload: _ => throw new InvalidOperationException("show"),
            unregister: () => { });

        service.Initialize();
        Assert.That(service.Show("<toast/>"), Is.False);
        service.Dispose();
    }

    [Test]
    public void ActivationPostsBeforeItTouchesTheWindow()
    {
        var events = new List<string>();
        Action posted = null;
        var service = new AppNotificationService(
            new UnusedNavigation(),
            register: () => { },
            detach: () => { },
            showPayload: _ => true,
            postToUi: action =>
            {
                events.Add("post");
                posted = action;
            },
            presentOnUi: key => events.Add("present:" + key));

        service.Activate("action=settings");

        Assert.That(events, Is.EqualTo(new[] { "post" }));
        posted();
        Assert.That(events, Is.EqualTo(new[] { "post", "present:" + ToastNotificationActivation.PageKeySettings }));
        service.Dispose();
    }

    [Test]
    public async Task CapturedQueueRunsActivationOffTheCallerWithoutAWindow()
    {
        var controller = DispatcherQueueController.CreateOnDedicatedThread();
        var previous = UiThread.Queue;
        try
        {
            UiThread.Queue = controller.DispatcherQueue;
            string presented = null;
            var done = new TaskCompletionSource();
            var service = new AppNotificationService(
                new UnusedNavigation(),
                register: () => { },
                detach: () => { },
                showPayload: _ => true,
                presentOnUi: key =>
                {
                    presented = key;
                    done.TrySetResult();
                });

            service.Activate("action=overview");
            await done.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.That(presented, Is.EqualTo(ToastNotificationActivation.PageKeyOverview));
            service.Dispose();
        }
        finally
        {
            UiThread.Queue = previous;
            await controller.ShutdownQueueAsync();
        }
    }

    private sealed class UnusedNavigation : INavigationService
    {
        public Frame Frame
        {
            get;
            set;
        }

        public bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false)
        {
            return false;
        }
    }
}
