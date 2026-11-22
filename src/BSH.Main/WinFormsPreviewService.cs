// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Brightbits.BSH.Engine;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

internal sealed class WinFormsPreviewService
{
    private readonly Func<Task<bool>> checkMedia;
    private readonly Func<bool> requestPassword;
    private readonly Func<int, string, string, Task<(string Path, bool IsTemporary)>> retrieveFile;
    private readonly Func<string, Task> showPreview;
    private readonly Action showError;

    public WinFormsPreviewService() : this(
        () => BackupLogic.BackupController.CheckMediaAsync(ActionType.Restore),
        () => BackupLogic.BackupController.RequestPassword(),
        (version, file, folder) => BackupLogic.QueryManager.GetFileNameFromDriveAsync(version, file, folder, BackupLogic.BackupService.GetPassword()),
        path => LaunchAsync(path),
        () => MessageBox.Show(Resources.DLG_FEATURE_NOT_AVAILABLE_TEXT, Resources.DLG_FEATURE_NOT_AVAILABLE_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Exclamation))
    {
    }

    internal WinFormsPreviewService(
        Func<Task<bool>> checkMedia,
        Func<bool> requestPassword,
        Func<int, string, string, Task<(string Path, bool IsTemporary)>> retrieveFile,
        Func<string, Task> showPreview,
        Action showError)
    {
        this.checkMedia = checkMedia;
        this.requestPassword = requestPassword;
        this.retrieveFile = retrieveFile;
        this.showPreview = showPreview;
        this.showError = showError;
    }

    public async Task PreviewAsync(Func<(int Version, string File, string Folder)> getSelection)
    {
        if (!await checkMedia() || !requestPassword())
        {
            return;
        }

        (string Path, bool IsTemporary) file = default;
        try
        {
            var selection = getSelection();
            file = await retrieveFile(selection.Version, selection.File, selection.Folder);
            if (string.IsNullOrEmpty(file.Path))
            {
                throw new InvalidOperationException("No preview file was retrieved.");
            }

            await showPreview(file.Path);
        }
        catch
        {
            showError();
        }
        finally
        {
            // Only extracted files returned with ownership belong to this caller.
            if (file.IsTemporary && file.Path != null)
            {
                for (var attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        File.Delete(file.Path);
                        break;
                    }
                    catch
                    {
                        // Preserve the existing best-effort cleanup policy.
                    }
                }
            }
        }
    }

    internal static async Task LaunchAsync(string path, Func<ProcessStartInfo, Process> start = null)
    {
        var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, "SmartPreview.exe"))
        {
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Normal
        };
        info.ArgumentList.Add("-file:" + path);

        using var process = (start ?? Process.Start)(info)
            ?? throw new InvalidOperationException("SmartPreview did not start.");
        await process.WaitForExitAsync();
    }
}
