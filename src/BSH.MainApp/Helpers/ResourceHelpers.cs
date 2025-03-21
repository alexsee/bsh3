﻿// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Markup;
using Windows.ApplicationModel.Resources;

namespace BSH.MainApp.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed class ResourceString : MarkupExtension
{
    private static readonly ResourceLoader resourceLoader = new();

    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue() => resourceLoader.GetString(Name);
}
