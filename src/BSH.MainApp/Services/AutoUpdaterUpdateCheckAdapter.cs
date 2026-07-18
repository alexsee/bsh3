// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using AutoUpdaterDotNET;
using BSH.MainApp.Contracts.Services;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public sealed class AutoUpdaterUpdateCheckAdapter : IUpdateCheckAdapter
{
    private readonly Func<IPresentationService> presentationServiceFactory;
    private bool eventsHooked;
    private bool notifyWhenUpToDate;

    public AutoUpdaterUpdateCheckAdapter(Func<IPresentationService>? presentationServiceFactory = null)
    {
        this.presentationServiceFactory = presentationServiceFactory ?? (() => App.GetService<IPresentationService>());
    }

    public void Start(string feedUrl, bool notifyWhenUpToDate = false)
    {
        EnsureEventsHooked();
        this.notifyWhenUpToDate = notifyWhenUpToDate;
        AutoUpdater.Start(feedUrl);
    }

    private void EnsureEventsHooked()
    {
        if (eventsHooked)
        {
            return;
        }

        eventsHooked = true;
        AutoUpdater.CheckForUpdateEvent += OnCheckForUpdate;
    }

    private void OnCheckForUpdate(UpdateInfoEventArgs args)
    {
        if (!args.IsUpdateAvailable)
        {
            if (notifyWhenUpToDate)
            {
                _ = presentationServiceFactory().ShowMessageBoxAsync(
                    "No updates found",
                    "You are already using the latest version.",
                    [new UICommand("OK")]);
            }

            return;
        }

        AutoUpdater.ShowUpdateForm(args);
    }
}
