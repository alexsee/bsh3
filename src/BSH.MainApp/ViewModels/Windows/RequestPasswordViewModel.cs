// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using CommunityToolkit.Mvvm.ComponentModel;

namespace BSH.MainApp.ViewModels.Windows;
public partial class RequestPasswordViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string? password;

    [ObservableProperty]
    private bool? persist = false;
}
