// Copyright 2022 Alexander Seeliger
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Security.Principal;
using BSH.Service.Shared;
using BSH.Service.VSS;

namespace BSH.Service;

public class VSSService : IVSSRemoteObject
{
    private const string DeniedMessage = "The caller is not authorized to copy this file.";
    private readonly Action<string, Action<string>> withSnapshot;

    public VSSService() : this(WithSnapshot)
    {
    }

    internal VSSService(Action<string, Action<string>> withSnapshot)
    {
        this.withSnapshot = withSnapshot;
    }

    public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination)
    {
        var connection = CallerImpersonatingNpHost.CurrentConnection;
        if (connection == null)
        {
            return false;
        }

        connection.Error = null;
        WindowsIdentity? caller = null;
        try
        {
            // Capture the connection's token before VSS runs, then use it only for file access.
            connection.Pipe.RunAsClient(() => caller = WindowsIdentity.GetCurrent(ifImpersonating: true));
            if (caller?.ImpersonationLevel is not (TokenImpersonationLevel.Impersonation or TokenImpersonationLevel.Delegation))
            {
                throw new UnauthorizedAccessException(DeniedMessage);
            }

            var sourcePath = GetLocalPath(source);
            var destinationPath = GetLocalPath(destination);
            withSnapshot(sourcePath, snapshotPath =>
                WindowsIdentity.RunImpersonated(caller.AccessToken, () => CopySnapshot(snapshotPath, destinationPath)));
            return true;
        }
        catch (Exception error)
        {
            connection.Error = error;
            return false;
        }
        finally
        {
            caller?.Dispose();
        }
    }

    public Exception? GetException() =>
        CallerImpersonatingNpHost.CurrentConnection is { } connection
            ? connection.Error
            : new UnauthorizedAccessException(DeniedMessage);

    private static string GetLocalPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        const string extendedPrefix = @"\\?\";
        if (path.StartsWith(extendedPrefix, StringComparison.Ordinal))
        {
            path = path[extendedPrefix.Length..];
        }

        if (path.Length < 3 || !char.IsAsciiLetter(path[0]) || path[1] != ':' || path[2] != '\\')
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        var fullPath = Path.GetFullPath(path);
        var drive = new DriveInfo(Path.GetPathRoot(fullPath)!);
        if (drive.DriveType is not (DriveType.Fixed or DriveType.Removable or DriveType.Ram))
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        return fullPath;
    }

    private static void WithSnapshot(string source, Action<string> copy)
    {
        using var snapshot = new VssBackup();
        snapshot.Setup(Path.GetPathRoot(source)!);
        copy(snapshot.GetSnapshotPath(source));
    }

    private static void CopySnapshot(string source, string destination)
    {
        if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0)
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.Read);
        input.CopyTo(output);
    }
}
