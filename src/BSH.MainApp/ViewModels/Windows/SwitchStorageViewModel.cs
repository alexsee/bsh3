// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace BSH.MainApp.ViewModels.Windows;

public partial class SwitchStorageViewModel : ObservableObject
{
    private readonly ISwitchStorageService switchStorageService;
    private string databaseFile = string.Empty;

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();

    public ObservableCollection<SetupDriveItem> AvailableDrives { get; } = new();

    public IReadOnlyList<string> MediumTypeOptions { get; } =
    [
        "SwitchStorage_LocalDrive".GetLocalized(),
        "SwitchStorage_FtpServer".GetLocalized()
    ];

    public IReadOnlyList<string> FtpEncodings { get; } = ["ISO-8859-1", "UTF8"];

    [ObservableProperty]
    private int selectedMediumIndex;

    [ObservableProperty]
    private Visibility localTargetVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility ftpTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private SetupDriveItem? selectedDrive;

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

    private bool IsLocalTarget => SelectedMediumIndex == 0;

    public void Initialize(string databaseFilePath)
    {
        databaseFile = databaseFilePath;
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
            AvailableDrives.Add(new SetupDriveItem
            {
                DisplayName = $"{drive.Name} ({SafeVolumeLabel(drive)})",
                RootPath = drive.RootDirectory.FullName,
                DriveType = drive.DriveType
            });
        }
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        ValidationErrorMessage = null;

        if (IsLocalTarget)
        {
            if (SelectedDrive == null)
            {
                ValidationErrorMessage = "SwitchStorage_SelectDriveRequired".GetLocalized();
                return;
            }

            if (switchStorageService.LocalTargetContainsBackupData(SelectedDrive.RootPath))
            {
                ValidationErrorMessage = "SwitchStorage_MediumNotEmpty".GetLocalized();
                return;
            }

            await switchStorageService.SwitchToLocalAsync(
                SelectedDrive.RootPath,
                ResolveVolumeSerial(SelectedDrive.RootPath),
                databaseFile);
            TaskCompletionSource.TrySetResult(true);
            return;
        }

        if (!TryOpenFtp(requireEmptyTarget: true, out var error))
        {
            ValidationErrorMessage = error;
            return;
        }

        await switchStorageService.SwitchToFtpAsync(CreateFtpTarget(), databaseFile);
        TaskCompletionSource.TrySetResult(true);
    }

    [RelayCommand]
    private void TestFtpConnection()
    {
        ValidationErrorMessage = null;
        ValidationErrorMessage = TryOpenFtp(requireEmptyTarget: false, out var error)
            ? "SwitchStorage_FtpConnectionSuccessful".GetLocalized()
            : error;
    }

    [RelayCommand]
    private void Cancel()
    {
        TaskCompletionSource.TrySetResult(false);
    }

    private bool TryOpenFtp(bool requireEmptyTarget, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(FtpHost) || string.IsNullOrWhiteSpace(FtpUser))
        {
            error = "SwitchStorage_FtpHostUserRequired".GetLocalized();
            return false;
        }

        if (!int.TryParse(FtpPort, out var port))
        {
            error = "SwitchStorage_FtpPortInvalid".GetLocalized();
            return false;
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

            if (requireEmptyTarget && storage.FileExists("backup.bshdb"))
            {
                error = "SwitchStorage_MediumNotEmpty".GetLocalized();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = string.Format("SwitchStorage_FtpConnectionFailed".GetLocalized(), ex.Message);
            return false;
        }
    }

    private SwitchStorageFtpTarget CreateFtpTarget()
    {
        return new SwitchStorageFtpTarget(
            FtpHost,
            string.IsNullOrWhiteSpace(FtpPort) ? "21" : FtpPort,
            FtpUser,
            FtpPassword,
            FtpFolder,
            string.IsNullOrWhiteSpace(FtpEncoding) ? "UTF8" : FtpEncoding,
            FtpEnforceUnencrypted);
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
