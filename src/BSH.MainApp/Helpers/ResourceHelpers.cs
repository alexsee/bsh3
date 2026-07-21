// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Markup;

namespace BSH.MainApp.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed class ResourceString : MarkupExtension
{
    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue()
    {
        // CommunityToolkit returns null/empty when the key or PRI map is unavailable.
        return Name.GetLocalized() ?? Name;
    }
}
