// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IBackupTargetService
{
    Task<BackupTargetMoveResult> MoveExistingBackupDataAsync(string currentFolderPath, string newFolderPath);
}

public sealed record BackupTargetMoveResult(bool Success, string? ErrorMessage = null)
{
    public static BackupTargetMoveResult Succeeded { get; } = new(true);

    public static BackupTargetMoveResult Failed(string errorMessage) => new(false, errorMessage);
}
