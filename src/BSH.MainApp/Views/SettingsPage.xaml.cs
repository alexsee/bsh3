// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Views.SettingsPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BSH.MainApp.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void navSettings_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        var navOptions = new FrameNavigationOptions();
        navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

        Type pageType = typeof(SourcesSettingsPage);
        if (args.InvokedItemContainer == nviSources)
        {
            pageType = typeof(SourcesSettingsPage);
        }
        else if (args.InvokedItemContainer == nviTarget)
        {
            pageType = typeof(TargetSettingsPage);
        }
        else if (args.InvokedItemContainer == nviMode)
        {
            pageType = typeof(ModeSettingsPage);
        }
        else if (args.InvokedItemContainer == nviOptions)
        {
            pageType = typeof(OptionsSettingsPage);
        }
        else if (args.InvokedItemContainer == nviEnhanced)
        {
            pageType = typeof(EnhancedSettingsPage);
        }
        contentFrame.NavigateToType(pageType, null, navOptions);
    }
}
