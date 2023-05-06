// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Windows.ApplicationModel.Resources;

namespace BSH.MainApp.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}
