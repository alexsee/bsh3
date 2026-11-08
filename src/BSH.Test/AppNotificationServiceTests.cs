// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Notifications;
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
