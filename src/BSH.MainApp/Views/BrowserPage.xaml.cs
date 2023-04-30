// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Views;

public sealed partial class BrowserPage : Page
{
    public BrowserViewModel ViewModel => (BrowserViewModel)DataContext;

    public BrowserPage()
    {
        DataContext = App.GetService<BrowserViewModel>();
        InitializeComponent();
    }
}
