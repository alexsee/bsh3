// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.Windows.ApplicationModel.Resources;

namespace BSH.MainApp.Helpers;

public static class ResourceExtensions
{
    private static readonly Lazy<ResourceLoader?> ResourceLoader = new(CreateResourceLoader);

    public static string GetLocalized(this string resourceKey)
    {
        try
        {
            var loader = ResourceLoader.Value;
            if (loader is null)
            {
                return resourceKey;
            }

            var value = loader.GetString(resourceKey);
            return string.IsNullOrEmpty(value) ? resourceKey : value;
        }
        catch
        {
            // Unit tests and other hosts may not have WinAppSDK PRI resources available.
            return resourceKey;
        }
    }

    private static ResourceLoader? CreateResourceLoader()
    {
        try
        {
            return new ResourceLoader();
        }
        catch
        {
            return null;
        }
    }
}
