// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.ViewModels.Windows;

public partial class SwitchStorageViewModel : ObservableObject
{
    private readonly ISwitchStorageService switchStorageService;

    public TaskCompletionSource<SwitchStorageSelection?> TaskCompletionSource { get; } = new();

    public ObservableCollection<SwitchStorageDriveItem> AvailableDrives { get; } = new();

    public IReadOnlyList<string> FtpEncodings { get; } = ["ISO-8859-1", "UTF8"];

    [ObservableProperty]
    private int selectedMediumIndex;

    [ObservableProperty]
    private Visibility localTargetVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility ftpTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private SwitchStorageDriveItem? selectedDrive;

    [ObservableProperty]
    private string ftpHost = string.Empty;

    [ObservableProperty]
    private string ftpPort = "21";

    [ObservableProperty]
    private string ftpUser = string.Empty;

    [ObservableProperty]
    private string ftpPassword = string.Empty;

    [ObservableProperty]
    private string ftpFolder = string.Empty;

    [ObservableProperty]
    private string ftpEncoding = "UTF8";

    [ObservableProperty]
    private bool ftpEnforceUnencrypted;

    [ObservableProperty]
    private string? validationErrorMessage;

    public SwitchStorageViewModel(ISwitchStorageService switchStorageService)
    {
        this.switchStorageService = switchStorageService;
    }

    public bool IsLocalTarget => SelectedMediumIndex == 0;

    public void Initialize()
    {
        RefreshDrives();
        SelectedMediumIndex = 0;
        UpdateTargetVisibility();
    }

    partial void OnSelectedMediumIndexChanged(int value)
    {
        UpdateTargetVisibility();
        ValidationErrorMessage = null;
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        AvailableDrives.Clear();
        foreach (var drive in DriveInfo.GetDrives().Where(drive => drive.IsReady))
        {
            AvailableDrives.Add(new SwitchStorageDriveItem
            {
                DisplayName = $"{drive.Name} ({SafeVolumeLabel(drive)})",
                RootPath = drive.RootDirectory.FullName,
                DriveType = drive.DriveType
            });
        }
    }

    [RelayCommand]
    private void Confirm()
    {
        ValidationErrorMessage = null;

        if (IsLocalTarget)
        {
            if (SelectedDrive == null)
            {
                ValidationErrorMessage = "Select a drive for the new backup medium.";
                return;
            }

            if (switchStorageService.LocalTargetContainsBackupData(SelectedDrive.RootPath))
            {
                ValidationErrorMessage = "The selected medium already contains other backups and cannot be used for switching.";
                return;
            }

            TaskCompletionSource.TrySetResult(new SwitchStorageSelection
            {
                IsLocal = true,
                DriveRoot = SelectedDrive.RootPath,
                MediaVolumeSerial = ResolveVolumeSerial(SelectedDrive.RootPath)
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(FtpHost) || string.IsNullOrWhiteSpace(FtpUser))
        {
            ValidationErrorMessage = "Enter FTP host and username.";
            return;
        }

        if (!int.TryParse(FtpPort, out var port))
        {
            ValidationErrorMessage = "FTP port is invalid.";
            return;
        }

        try
        {
            var folder = FtpStorage.GetFtpPath(FtpFolder);
            FtpFolder = folder;

            using var storage = new FtpStorage(
                FtpHost,
                port,
                FtpUser,
                FtpPassword,
                folder,
                string.IsNullOrWhiteSpace(FtpEncoding) ? "UTF8" : FtpEncoding,
                !FtpEnforceUnencrypted,
                0);
            storage.Open();

            if (storage.FileExists("backup.bshdb"))
            {
                ValidationErrorMessage = "The selected medium already contains other backups and cannot be used for switching.";
                return;
            }
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = "FTP connection failed: " + ex.Message;
            return;
        }

        TaskCompletionSource.TrySetResult(new SwitchStorageSelection
        {
            IsLocal = false,
            Ftp = new SwitchStorageFtpTarget(
                FtpHost,
                string.IsNullOrWhiteSpace(FtpPort) ? "21" : FtpPort,
                FtpUser,
                FtpPassword,
                FtpFolder,
                string.IsNullOrWhiteSpace(FtpEncoding) ? "UTF8" : FtpEncoding,
                FtpEnforceUnencrypted)
        });
    }

    [RelayCommand]
    private void TestFtpConnection()
    {
        ValidationErrorMessage = null;

        if (string.IsNullOrWhiteSpace(FtpHost) || string.IsNullOrWhiteSpace(FtpUser))
        {
            ValidationErrorMessage = "Enter FTP host and username.";
            return;
        }

        if (!int.TryParse(FtpPort, out var port))
        {
            ValidationErrorMessage = "FTP port is invalid.";
            return;
        }

        try
        {
            FtpFolder = FtpStorage.GetFtpPath(FtpFolder);

            using var storage = new FtpStorage(
                FtpHost,
                port,
                FtpUser,
                FtpPassword,
                FtpFolder,
                string.IsNullOrWhiteSpace(FtpEncoding) ? "UTF8" : FtpEncoding,
                !FtpEnforceUnencrypted,
                0);
            storage.Open();

            ValidationErrorMessage = "FTP connection successful.";
        }
        catch (Exception ex)
        {
            ValidationErrorMessage = "FTP connection failed: " + ex.Message;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(null);
    }

    private void UpdateTargetVisibility()
    {
        LocalTargetVisibility = SelectedMediumIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
        FtpTargetVisibility = SelectedMediumIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string ResolveVolumeSerial(string driveRoot)
    {
        if (string.IsNullOrWhiteSpace(driveRoot) || driveRoot.Length < 3)
        {
            return "";
        }

        var serial = Win32Stuff.GetVolumeSerial(driveRoot[..3]);
        return string.IsNullOrEmpty(serial) || serial == "0" ? "" : serial;
    }

    private static string SafeVolumeLabel(DriveInfo drive)
    {
        try
        {
            return drive.VolumeLabel;
        }
        catch
        {
            return drive.DriveType.ToString();
        }
    }
}
