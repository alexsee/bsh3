// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;

namespace BSH.MainApp.Helpers;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed class ResourceString : MarkupExtension
{
    private static readonly Lazy<ResourceLoader?> ResourceLoader = new(CreateResourceLoader);

    public string Name { get; set; } = string.Empty;

    protected override object ProvideValue()
    {
        try
        {
            var loader = ResourceLoader.Value;
            if (loader is null)
            {
                return Name;
            }

            var value = loader.GetString(Name);
            return string.IsNullOrEmpty(value) ? Name : value;
        }
        catch
        {
            return Name;
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
