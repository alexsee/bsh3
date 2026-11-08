// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Contracts.Services;

public interface INavigationService
{
    Frame? Frame
    {
        get; set;
    }

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);
}
