// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Helpers;

public static class FrameExtensions
{
    public static object? GetPageViewModel(this Frame frame) => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
}
