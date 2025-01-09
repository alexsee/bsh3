// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;

namespace BSH.MainApp.Windows;

public sealed partial class NewBackupWindow : WinUIEx.WindowEx
{
    public NewBackupViewModel ViewModel { get; } = new NewBackupViewModel();

    public NewBackupWindow()
    {
        this.InitializeComponent();
    }
}
