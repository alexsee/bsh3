// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Contracts.Services;
using BSH.MainApp.Helpers;

namespace BSH.MainApp.Services;

public sealed class BackupTargetService : IBackupTargetService
{
    public Task<BackupTargetMoveResult> MoveExistingBackupDataAsync(string currentFolderPath, string newFolderPath)
    {
        return Task.Run(() => MoveExistingBackupData(currentFolderPath, newFolderPath));
    }

    private static BackupTargetMoveResult MoveExistingBackupData(string currentFolderPath, string newFolderPath)
    {
        try
        {
            if (PathRules.IsDriveRoot(newFolderPath))
            {
                return BackupTargetMoveResult.Failed("Selecting a drive root as backup target is not supported. Choose a specific folder instead.");
            }

            if (string.IsNullOrWhiteSpace(currentFolderPath) || string.Equals(currentFolderPath, newFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                return BackupTargetMoveResult.Succeeded;
            }

            if (Directory.Exists(newFolderPath) && Directory.EnumerateFileSystemEntries(newFolderPath).Any())
            {
                return BackupTargetMoveResult.Failed("The selected target folder already contains files.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(newFolderPath) ?? newFolderPath);
            if (Directory.Exists(newFolderPath))
            {
                Directory.Delete(newFolderPath, true);
            }

            Directory.Move(currentFolderPath, newFolderPath);
            return BackupTargetMoveResult.Succeeded;
        }
        catch (Exception ex)
        {
            return BackupTargetMoveResult.Failed(ex.Message);
        }
    }
}
