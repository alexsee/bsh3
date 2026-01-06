// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views;

public sealed partial class AddSchedulePage : Page
{
    public AddScheduleViewModel ViewModel => (AddScheduleViewModel)DataContext;

    public AddSchedulePage()
    {
        InitializeComponent();
    }
}
