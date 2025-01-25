// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BSH.MainApp.Models;

public class FileOrFolderItem : INotifyPropertyChanged
{
    public string FileNameOnDrive
    {
        get; set;
    }

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

    public BitmapSource? Icon16
    {
        get;
        set;
    }

    public BitmapSource? Icon64
    {
        get;
        set;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
