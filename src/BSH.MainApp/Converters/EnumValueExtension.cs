// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Markup;

namespace BSH.MainApp.Converters;

/// <summary>
/// Provides an enum value that keeps its CLR type. WinUI boxes inline/resource enum
/// CommandParameter values as Int32, which breaks RelayCommand{TEnum}.
/// </summary>
[MarkupExtensionReturnType(ReturnType = typeof(object))]
public sealed class EnumValueExtension : MarkupExtension
{
    public Type? Type
    {
        get; set;
    }

    public string? Member
    {
        get; set;
    }

    protected override object ProvideValue()
    {
        if (Type == null || string.IsNullOrWhiteSpace(Member))
        {
            throw new InvalidOperationException("EnumValue requires both Type and Member.");
        }

        return Enum.Parse(Type, Member);
    }
}
