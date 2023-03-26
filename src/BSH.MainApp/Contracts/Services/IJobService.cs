using Brightbits.BSH.Engine;

namespace BSH.MainApp.Contracts.Services;
public interface IJobService
{
    bool IsCancellationRequested
    {
        get;
    }

    void Cancel();
    Task<bool> CheckMediaAsync(ActionType action, bool silent = false);
    Task CreateBackupAsync(string title, string description, bool statusDialog = true, bool fullBackup = false, bool shutdownPC = false, bool shutdownApp = false, string sourceFolders = "");
    Task DeleteBackupAsync(string version, bool statusDialog = true);
    Task DeleteBackupsAsync(List<string> versions, bool statusDialog = true);
    Task DeleteSingleFileAsync(string fileFilter, string folderFilter, bool statusDialog = true);
    CancellationToken GetNewCancellationToken();
    Task<bool> RequestPassword();
    Task RestoreBackupAsync(string version, List<string> files, string destination, bool statusDialog = true);
    Task RestoreBackupAsync(string version, string file, string destination, bool statusDialog = true);
}