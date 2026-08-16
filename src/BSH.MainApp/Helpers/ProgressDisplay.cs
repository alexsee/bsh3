// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Helpers;

/// <summary>
/// Keeps WinUI <c>RangeBase</c> progress values legal.
/// Setting <c>Maximum</c> to 0 or letting <c>Value</c> exceed <c>Maximum</c> (including
/// transiently between two <c>PropertyChanged</c> notifications) throws a COMException
/// that terminates the process with 0xC000027B and bypasses managed exception handlers.
/// </summary>
public static class ProgressDisplay
{
    public static (int Maximum, int Value) Normalize(int total, int current)
    {
        var maximum = Math.Max(1, total);
        var value = Math.Clamp(current, 0, maximum);
        return (maximum, value);
    }

    /// <summary>
    /// Assigns the next maximum/value in an order that never leaves Value &gt; Maximum
    /// between the two setter calls.
    /// </summary>
    public static void Assign(
        int currentMaximum,
        int currentValue,
        int total,
        int current,
        Action<int> setMaximum,
        Action<int> setValue)
    {
        ArgumentNullException.ThrowIfNull(setMaximum);
        ArgumentNullException.ThrowIfNull(setValue);

        var (maximum, value) = Normalize(total, current);

        if (maximum < currentValue)
        {
            setValue(value);
            setMaximum(maximum);
            return;
        }

        if (maximum != currentMaximum)
        {
            setMaximum(maximum);
        }

        setValue(value);
    }
}
