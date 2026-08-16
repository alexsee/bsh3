// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Windows.Graphics;
using WinUIEx;

namespace BSH.MainApp.Helpers;

public static class WindowPlacementExtensions
{
    public static void CenterOnMainWindow(this WindowEx window)
    {
        var mainWindow = App.MainWindow.AppWindow;
        var mainPosition = mainWindow.Position;
        var mainSize = mainWindow.Size;
        var windowSize = window.AppWindow.Size;

        window.AppWindow.Move(new PointInt32(
            mainPosition.X + ((mainSize.Width - windowSize.Width) / 2),
            mainPosition.Y + ((mainSize.Height - windowSize.Height) / 2)));
    }
}
