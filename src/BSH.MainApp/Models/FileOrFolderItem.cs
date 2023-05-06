// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BSH.MainApp.Models;

public class FileOrFolderItem : INotifyPropertyChanged
{
    public string Name
    {
        get; set;
    }

    public string FullPath
    {
        get; set;
    }

    public DateTime FileDateModified
    {
        get; set;
    }

    public DateTime FileDateCreated
    {
        get; set;
    }

    public double FileSize
    {
        get; set;
    }

    public bool IsFile
    {
        get; set;
    }

    private BitmapSource _icon;
    public BitmapSource Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            this.OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
