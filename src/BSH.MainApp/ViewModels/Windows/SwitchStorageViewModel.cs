// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.ObjectModel;
using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Services;
using Brightbits.BSH.Engine.Storage;
using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Models;
using BSH.MainApp.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI;

namespace BSH.MainApp.ViewModels.Windows;

public partial class SwitchStorageViewModel : ObservableObject
{
    private readonly ISwitchStorageService switchStorageService;
    private string databaseFile = string.Empty;

    public TaskCompletionSource<bool> TaskCompletionSource { get; } = new();

    public ObservableCollection<SetupDriveItem> AvailableDrives { get; } = new();

    public IReadOnlyList<MediaTargetOption> MediumOptions { get; } =
    [
        new(MediaTargetKind.LocalDrive, "SwitchStorage_LocalDrive".GetLocalized()),
        new(MediaTargetKind.Ftp, "SwitchStorage_FtpServer".GetLocalized()),
        new(MediaTargetKind.Unc, "Setup_Target_Unc_Title".GetLocalized()),
    ];

    public IReadOnlyList<string> FtpEncodings { get; } = ["ISO-8859-1", "UTF8"];

    [ObservableProperty]
    private MediaTargetOption? selectedMedium;

    [ObservableProperty]
    private Visibility localTargetVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility ftpTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility uncTargetVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private SetupDriveItem? selectedDrive;

    [ObservableProperty]
    private string uncPath = string.Empty;

    [ObservableProperty]
    private string uncUsername = string.Empty;

    [ObservableProperty]
    private string uncPassword = string.Empty;

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

    public void Initialize(string databaseFilePath)
    {
        databaseFile = databaseFilePath;
        RefreshDrives();
        SelectedMedium = MediumOptions[0];
        UpdateTargetVisibility();
    }

    partial void OnSelectedMediumChanged(MediaTargetOption? value)
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

        switch (SelectedMedium?.Kind)
        {
            case MediaTargetKind.LocalDrive:
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
                    MediaTargetApplier.ResolveVolumeSerial(SelectedDrive.RootPath),
                    databaseFile);
                TaskCompletionSource.TrySetResult(true);
                return;

            case MediaTargetKind.Unc:
                if (!TryValidateUnc(requireEmptyTarget: true, out var uncError))
                {
                    ValidationErrorMessage = uncError;
                    return;
                }

                await switchStorageService.SwitchToUncAsync(CreateUncTarget(), databaseFile);
                TaskCompletionSource.TrySetResult(true);
                return;

            case MediaTargetKind.Ftp:
                if (!TryOpenFtp(requireEmptyTarget: true, out var error))
                {
                    ValidationErrorMessage = error;
                    return;
                }

                await switchStorageService.SwitchToFtpAsync(CreateFtpTarget(), databaseFile);
                TaskCompletionSource.TrySetResult(true);
                return;

            default:
                ValidationErrorMessage = "Setup_Validation_SelectBackupTarget".GetLocalized();
                return;
        }
    }

    [RelayCommand]
    private void TestUncConnection()
    {
        ValidationErrorMessage = null;
        ValidationErrorMessage = TryValidateUnc(requireEmptyTarget: false, out var error)
            ? "SwitchStorage_NetworkConnectionSuccessful".GetLocalized()
            : error;
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

    private bool TryValidateUnc(bool requireEmptyTarget, out string? error)
    {
        error = null;

        var probe = UncTargetProbe.Probe(UncPath, UncUsername, UncPassword, requireEmptyTarget);
        switch (probe.Status)
        {
            case UncProbeStatus.Ok:
                UncPath = probe.NormalizedPath;
                return true;

            case UncProbeStatus.InvalidPath:
                error = "Setup_Validation_EnterNetworkPath".GetLocalized();
                return false;

            case UncProbeStatus.ContainsBackupData:
                error = "SwitchStorage_MediumNotEmpty".GetLocalized();
                return false;

            default:
                error = string.IsNullOrEmpty(probe.Detail)
                    ? "Setup_Validation_NetworkUnreachable".GetLocalized()
                    : string.Format(
                        "Setup_Validation_NetworkUnreachableWithError".GetLocalized() ?? "Setup_Validation_NetworkUnreachableWithError",
                        probe.Detail);
                return false;
        }
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

            if (requireEmptyTarget && storage.FileExists(UncTargetProbe.BackupDatabaseFileName))
            {
                error = "SwitchStorage_MediumNotEmpty".GetLocalized();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = string.Format("SwitchStorage_FtpConnectionFailed".GetLocalized() ?? "SwitchStorage_FtpConnectionFailed", ex.Message);
            return false;
        }
    }

    private SwitchStorageUncTarget CreateUncTarget()
    {
        return new SwitchStorageUncTarget(
            UncPath.Trim().Replace('/', '\\'),
            UncUsername ?? "",
            UncPassword ?? "");
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
        var kind = SelectedMedium?.Kind;
        LocalTargetVisibility = kind == MediaTargetKind.LocalDrive ? Visibility.Visible : Visibility.Collapsed;
        FtpTargetVisibility = kind == MediaTargetKind.Ftp ? Visibility.Visible : Visibility.Collapsed;
        UncTargetVisibility = kind == MediaTargetKind.Unc ? Visibility.Visible : Visibility.Collapsed;
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

public sealed record MediaTargetOption(MediaTargetKind Kind, string DisplayName)
{
    public override string ToString() => DisplayName;
}
