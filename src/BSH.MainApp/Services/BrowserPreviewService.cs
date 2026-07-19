// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Contracts.Services;
using BSH.MainApp.Contracts.Services;
using Windows.UI.Popups;

namespace BSH.MainApp.Services;

public class BrowserPreviewService : IBrowserPreviewService
{
    private readonly IJobService jobService;
    private readonly IBackupService backupService;
    private readonly IQueryManager queryManager;
    private readonly IPresentationService presentationService;
    private readonly ISmartPreviewHost smartPreviewHost;

    public BrowserPreviewService(
        IJobService jobService,
        IBackupService backupService,
        IQueryManager queryManager,
        IPresentationService presentationService,
        ISmartPreviewHost smartPreviewHost)
    {
        this.jobService = jobService;
        this.backupService = backupService;
        this.queryManager = queryManager;
        this.presentationService = presentationService;
        this.smartPreviewHost = smartPreviewHost;
    }

    public async Task PreviewFileAsync(string versionId, string fileName, string filePath)
    {
        if (!await jobService.CheckMediaAsync(ActionType.Restore))
        {
            return;
        }

        if (!await jobService.RequestPassword())
        {
            return;
        }

        string? localFilePath = null;
        var isTemporary = false;

        try
        {
            var password = backupService.GetPassword();
            (localFilePath, isTemporary) = await queryManager.GetFileNameFromDriveAsync(
                int.Parse(versionId),
                fileName,
                filePath,
                password);

            if (string.IsNullOrEmpty(localFilePath))
            {
                return;
            }

            await smartPreviewHost.ShowAsync(localFilePath, isTemporary);
        }
        catch
        {
            await presentationService.ShowMessageBoxAsync(
                "Feature not available",
                "Feature is currently not available.\n\nThis feature is currently not available because the quick preview could not be found. Reinstall Backup Service Home to resolve the problem.",
                [new UICommand("OK")]);
        }
        finally
        {
            if (isTemporary && !string.IsNullOrEmpty(localFilePath))
            {
                TryDeleteTemporaryFile(localFilePath);
            }
        }
    }

    private static void TryDeleteTemporaryFile(string filePath)
    {
        for (var i = 0; i < 5; i++)
        {
            try
            {
                File.Delete(filePath);
                break;
            }
            catch
            {
                // Preview process may still be releasing the file; retry.
            }
        }
    }
}
