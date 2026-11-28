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

using System.ComponentModel;
using System.Runtime.InteropServices;
using BSH.Service.VSS;
using Microsoft.Win32.SafeHandles;

namespace BSH.Service;

internal readonly record struct CallerFileRights(bool CanReadSource, bool CanWriteDestination);

internal static class VssCopyPolicy
{
    internal static bool IsAuthorized(CallerFileRights rights) => rights.CanReadSource && rights.CanWriteDestination;
}

internal static class VssCallerAuthorization
{
    internal const string DeniedMessage = "The caller is not authorized to copy this file.";

    private const uint TokenQuery = 0x0008;
    private const uint TokenDuplicate = 0x0002;
    private const uint TokenImpersonate = 0x0004;
    private const uint TokenAdjustPrivileges = 0x0020;
    private const uint SePrivilegeEnabled = 0x00000002;

    private const int SecurityImpersonation = 2;
    private const int TokenImpersonation = 2;

    private const uint FileReadAttributes = 0x00000080;
    private const uint ReadControl = 0x00020000;
    private const uint FileGenericRead = 0x00120089;
    private const uint FileGenericWrite = 0x00120116;
    private const uint FileGenericExecute = 0x001200A0;
    private const uint FileAllAccess = 0x001F01FF;
    private const uint FileAddFile = 0x00000002;

    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint FileShareDelete = 0x00000004;
    private const uint OpenExisting = 3;
    private const uint CreateAlways = 2;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileAttributeNormal = 0x00000080;

    private const uint SeFileObject = 1;
    private const uint OwnerSecurityInformation = 0x1;
    private const uint GroupSecurityInformation = 0x2;
    private const uint DaclSecurityInformation = 0x4;

    private const uint VolumeNameDos = 0;
    private const uint InvalidFileAttributes = 0xFFFFFFFF;
    private const uint DriveRemovable = 2;
    private const uint DriveFixed = 3;
    private const uint DriveRamDisk = 6;

    private const int ErrorFileNotFound = 2;
    private const int ErrorPathNotFound = 3;
    private const int ErrorAccessDenied = 5;
    private const int ErrorInsufficientBuffer = 122;

    internal static bool TryDuplicateCallerToken(out SafeAccessTokenHandle? caller)
    {
        caller = null;
        if (!OpenThreadToken(GetCurrentThread(), TokenQuery | TokenDuplicate | TokenImpersonate, true, out var opened))
        {
            return false;
        }

        using var threadToken = new SafeAccessTokenHandle(opened);
        caller = DuplicateImpersonation(threadToken);
        return caller != null;
    }

    internal static bool IsThreadImpersonating()
    {
        if (!OpenThreadToken(GetCurrentThread(), TokenQuery, true, out var opened))
        {
            return false;
        }

        new SafeAccessTokenHandle(opened).Dispose();
        return true;
    }

    internal static bool RevertToSelf() => RevertToSelfNative();

    internal static CallerFileRights Check(SafeAccessTokenHandle caller, string source, string destination)
    {
        ArgumentNullException.ThrowIfNull(caller);
        return new CallerFileRights(CanRead(caller, source), CanWriteDestination(caller, destination));
    }

    internal static void CopyAuthorized(SafeAccessTokenHandle caller, string source, string destination)
    {
        ArgumentNullException.ThrowIfNull(caller);
        if (!IsLocalDrivePath(source) || !IsLocalDrivePath(destination))
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        TryEnableBackupPrivilege();
        string snapshotSource;
        using (var sourceHandle = OpenForSecurity(source))
        {
            if (sourceHandle == null)
            {
                throw new UnauthorizedAccessException(DeniedMessage);
            }

            var rights = new CallerFileRights(
                HasRights(caller, sourceHandle, FileGenericRead),
                CanWriteDestination(caller, destination));
            if (!VssCopyPolicy.IsAuthorized(rights))
            {
                throw new UnauthorizedAccessException(DeniedMessage);
            }

            snapshotSource = GetFinalDosPath(sourceHandle);
        }

        if (!IsLocalDrivePath(snapshotSource))
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        CopyFromVolumeSnapshot(caller, snapshotSource, destination);
    }

    private static bool CanRead(SafeAccessTokenHandle caller, string path)
    {
        if (!IsLocalDrivePath(path))
        {
            return false;
        }

        TryEnableBackupPrivilege();
        using var handle = OpenForSecurity(path);
        if (handle == null)
        {
            return false;
        }

        return HasRights(caller, handle, FileGenericRead);
    }

    private static bool CanWriteDestination(SafeAccessTokenHandle caller, string path)
    {
        if (!IsLocalDrivePath(path))
        {
            return false;
        }

        TryEnableBackupPrivilege();
        var attributes = GetFileAttributesW(path);
        if (attributes == InvalidFileAttributes)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error is ErrorFileNotFound or ErrorPathNotFound)
            {
                return CanAddFileInParent(caller, path);
            }

            return false;
        }

        using var handle = OpenForSecurity(path);
        return handle != null && HasRights(caller, handle, FileGenericWrite);
    }

    private static bool CanAddFileInParent(SafeAccessTokenHandle caller, string path)
    {
        var parent = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(parent))
        {
            return false;
        }

        if (parent.Length == 2 && parent[1] == ':')
        {
            parent += "\\";
        }

        if (!IsLocalDrivePath(parent))
        {
            return false;
        }

        using var handle = OpenForSecurity(parent);
        return handle != null && HasRights(caller, handle, FileAddFile);
    }

    private static bool IsLocalDrivePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Contains('\0') || !Path.IsPathFullyQualified(path))
        {
            return false;
        }

        var dosPath = ToDosPath(path);
        if (dosPath.Length < 3 || !char.IsAsciiLetter(dosPath[0]) || dosPath[1] != ':' || dosPath[2] != '\\')
        {
            return false;
        }

        var driveType = GetDriveTypeW(dosPath[..3]);
        return driveType is DriveRemovable or DriveFixed or DriveRamDisk;
    }

    private static string ToDosPath(string path)
    {
        const string uncPrefix = @"\\?\UNC\";
        const string devicePrefix = @"\\?\";
        if (path.StartsWith(uncPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return @"\\" + path[uncPrefix.Length..];
        }

        if (path.StartsWith(devicePrefix, StringComparison.Ordinal))
        {
            return path[devicePrefix.Length..];
        }

        return path;
    }

    private static void CopyFromVolumeSnapshot(SafeAccessTokenHandle caller, string sourcePath, string destination)
    {
        var root = Path.GetPathRoot(sourcePath);
        if (string.IsNullOrEmpty(root))
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        using var backup = new VssBackup();
        backup.Setup(root);
        using var snapshot = new FileStream(
            backup.GetSnapshotPath(sourcePath),
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var created = CreateDestinationAsCaller(caller, destination);
        using var output = new FileStream(created, FileAccess.Write);
        snapshot.CopyTo(output);
    }

    private static SafeFileHandle CreateDestinationAsCaller(SafeAccessTokenHandle caller, string destination)
    {
        if (!ImpersonateLoggedOnUser(caller))
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        try
        {
            var handle = CreateFileW(
                destination,
                FileGenericWrite,
                FileShareRead,
                IntPtr.Zero,
                CreateAlways,
                FileAttributeNormal,
                IntPtr.Zero);
            if (!handle.IsInvalid)
            {
                return handle;
            }

            var error = Marshal.GetLastPInvokeError();
            handle.Dispose();
            if (error == ErrorAccessDenied)
            {
                throw new UnauthorizedAccessException(DeniedMessage);
            }

            throw new Win32Exception(error);
        }
        finally
        {
            RevertToSelfNative();
        }
    }

    private static SafeFileHandle? OpenForSecurity(string path)
    {
        var handle = CreateFileW(
            path,
            FileReadAttributes | ReadControl,
            FileShareRead | FileShareWrite | FileShareDelete,
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics,
            IntPtr.Zero);
        if (!handle.IsInvalid)
        {
            return handle;
        }

        handle.Dispose();
        return null;
    }

    private static string GetFinalDosPath(SafeFileHandle handle)
    {
        var buffer = new char[32768];
        var length = GetFinalPathNameByHandleW(handle, buffer, (uint)buffer.Length, VolumeNameDos);
        if (length == 0 || length >= buffer.Length)
        {
            throw new UnauthorizedAccessException(DeniedMessage);
        }

        return ToDosPath(new string(buffer, 0, (int)length));
    }

    private static bool HasRights(SafeAccessTokenHandle caller, SafeFileHandle handle, uint desiredAccess)
    {
        var descriptor = IntPtr.Zero;
        var privileges = IntPtr.Zero;
        try
        {
            var error = GetSecurityInfo(
                handle,
                SeFileObject,
                OwnerSecurityInformation | GroupSecurityInformation | DaclSecurityInformation,
                out _,
                out _,
                out _,
                out _,
                out descriptor);
            if (error != 0)
            {
                return false;
            }

            using var impersonation = DuplicateImpersonation(caller);
            if (impersonation == null)
            {
                return false;
            }

            var mapping = new GenericMapping
            {
                GenericRead = FileGenericRead,
                GenericWrite = FileGenericWrite,
                GenericExecute = FileGenericExecute,
                GenericAll = FileAllAccess
            };
            MapGenericMask(ref desiredAccess, ref mapping);

            var length = 256u;
            privileges = Marshal.AllocHGlobal((int)length);
            if (!AccessCheck(descriptor, impersonation, desiredAccess, ref mapping, privileges, ref length, out _, out var accessStatus))
            {
                if (Marshal.GetLastPInvokeError() != ErrorInsufficientBuffer || length == 0)
                {
                    return false;
                }

                Marshal.FreeHGlobal(privileges);
                privileges = IntPtr.Zero;
                privileges = Marshal.AllocHGlobal((int)length);
                if (!AccessCheck(descriptor, impersonation, desiredAccess, ref mapping, privileges, ref length, out _, out accessStatus))
                {
                    return false;
                }
            }

            return accessStatus != 0;
        }
        finally
        {
            if (descriptor != IntPtr.Zero)
            {
                LocalFree(descriptor);
            }

            if (privileges != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(privileges);
            }
        }
    }

    private static SafeAccessTokenHandle? DuplicateImpersonation(SafeAccessTokenHandle token)
    {
        if (!DuplicateTokenEx(
            token,
            TokenQuery | TokenImpersonate,
            IntPtr.Zero,
            SecurityImpersonation,
            TokenImpersonation,
            out var duplicate))
        {
            if (duplicate != IntPtr.Zero)
            {
                CloseHandle(duplicate);
            }

            return null;
        }

        return new SafeAccessTokenHandle(duplicate);
    }

    private static void TryEnableBackupPrivilege()
    {
        if (!OpenProcessToken(GetCurrentProcess(), TokenAdjustPrivileges | TokenQuery, out var opened))
        {
            return;
        }

        using var processToken = new SafeAccessTokenHandle(opened);
        if (!LookupPrivilegeValue(null, "SeBackupPrivilege", out var luid))
        {
            return;
        }

        var privileges = new TokenPrivileges
        {
            PrivilegeCount = 1,
            Privileges = new LuidAndAttributes
            {
                Luid = luid,
                Attributes = SePrivilegeEnabled
            }
        };
        _ = AdjustTokenPrivileges(processToken, false, ref privileges, 0, IntPtr.Zero, IntPtr.Zero);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Luid
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LuidAndAttributes
    {
        public Luid Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TokenPrivileges
    {
        public uint PrivilegeCount;
        public LuidAndAttributes Privileges;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GenericMapping
    {
        public uint GenericRead;
        public uint GenericWrite;
        public uint GenericExecute;
        public uint GenericAll;
    }

    [DllImport("kernel32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr GetCurrentThread();

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenThreadToken(IntPtr thread, uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool openAsSelf, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr process, uint desiredAccess, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateTokenEx(
        SafeAccessTokenHandle existingToken,
        uint desiredAccess,
        IntPtr tokenAttributes,
        int impersonationLevel,
        int tokenType,
        out IntPtr newToken);

    [DllImport("advapi32.dll", EntryPoint = "RevertToSelf", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RevertToSelfNative();

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ImpersonateLoggedOnUser(SafeAccessTokenHandle token);

    [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeValue(string? system, string name, out Luid luid);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(
        SafeAccessTokenHandle token,
        [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges,
        ref TokenPrivileges newState,
        uint bufferLength,
        IntPtr previousState,
        IntPtr returnLength);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetSecurityInfo(
        SafeFileHandle handle,
        uint objectType,
        uint securityInfo,
        out IntPtr owner,
        out IntPtr group,
        out IntPtr dacl,
        out IntPtr sacl,
        out IntPtr securityDescriptor);

    [DllImport("advapi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AccessCheck(
        IntPtr securityDescriptor,
        SafeAccessTokenHandle clientToken,
        uint desiredAccess,
        ref GenericMapping genericMapping,
        IntPtr privilegeSet,
        ref uint privilegeSetLength,
        out uint grantedAccess,
        out int accessStatus);

    [DllImport("advapi32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void MapGenericMask(ref uint accessMask, ref GenericMapping genericMapping);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetFinalPathNameByHandleW(SafeFileHandle file, [Out] char[] path, uint pathLength, uint flags);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetFileAttributesW(string fileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetDriveTypeW(string rootPath);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr LocalFree(IntPtr memory);
}
