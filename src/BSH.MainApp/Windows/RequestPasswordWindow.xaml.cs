// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.ViewModels.Windows;
using Microsoft.UI.Xaml.Controls;

namespace BSH.MainApp.Windows;

public sealed partial class RequestPasswordWindow : ContentDialog
{
    public RequestPasswordViewModel ViewModel { get; } = new RequestPasswordViewModel();

    public RequestPasswordWindow()
    {
        this.InitializeComponent();
    }
}
