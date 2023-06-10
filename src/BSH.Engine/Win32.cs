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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Brightbits.BSH.Engine.Models;

namespace Brightbits.BSH.Engine;

public class Win32Stuff
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern long GetVolumeInformation(
        string PathName,
        StringBuilder VolumeNameBuffer,
        UInt32 VolumeNameSize,
        ref UInt32 VolumeSerialNumber,
        ref UInt32 MaximumComponentLength,
        ref UInt32 FileSystemFlags,
        StringBuilder FileSystemNameBuffer,
        UInt32 FileSystemNameSize);

    static System.Timers.Timer tmr;

    [FlagsAttribute]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
    public const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    public static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes,
        out SHFILEINFO psfi, uint cbFileInfo, uint flags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [Flags]
    public enum SHGFI : uint
    {
        SHGFI_ICON = 0x000000100,
        SHGFI_DISPLAYNAME = 0x000000200,
        SHGFI_TYPENAME = 0x000000400,
        SHGFI_ATTRIBUTES = 0x000000800,
        SHGFI_ICONLOCATION = 0x000001000,
        SHGFI_EXETYPE = 0x000002000,
        SHGFI_SYSICONINDEX = 0x000004000,
        SHGFI_LINKOVERLAY = 0x000008000,
        SHGFI_SELECTED = 0x000010000,
        SHGFI_ATTR_SPECIFIED = 0x000020000,
        SHGFI_LARGEICON = 0x000000000,
        SHGFI_SMALLICON = 0x000000001,
        SHGFI_OPENICON = 0x000000002,
        SHGFI_SHELLICONSIZE = 0x000000004,
        SHGFI_PIDL = 0x000000008,
        SHGFI_USEFILEATTRIBUTES = 0x000000010,
        SHGFI_ADDOVERLAYS = 0x000000020,
        SHGFI_OVERLAYINDEX = 0x000000040
    }

    [DllImport("mpr.dll")]
    public static extern int WNetAddConnection2(NetResource netResource, string password, string username, int flags);

    [DllImport("mpr.dll")]
    public static extern int WNetCancelConnection2(string name, int flags, bool force);

    [StructLayout(LayoutKind.Sequential)]
    public class NetResource
    {
        public ResourceScope Scope;
        public ResourceType ResourceType;
        public ResourceDisplaytype DisplayType;
        public int Usage;
        public string LocalName;
        public string RemoteName;
        public string Comment;
        public string Provider;
    }

    public enum ResourceScope : int
    {
        Connected = 1,
        GlobalNetwork,
        Remembered,
        Recent,
        Context
    };

    public enum ResourceType : int
    {
        Any = 0,
        Disk = 1,
        Print = 2,
        Reserved = 8,
    }

    public enum ResourceDisplaytype : int
    {
        Generic = 0x0,
        Domain = 0x01,
        Server = 0x02,
        Share = 0x03,
        File = 0x04,
        Group = 0x05,
        Network = 0x06,
        Root = 0x07,
        Shareadmin = 0x08,
        Directory = 0x09,
        Tree = 0x0a,
        Ndscontainer = 0x0b
    }

    public static string GetDisplayName(string path)
    {
        var shfi = new SHFILEINFO();
        if (0 != SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, out shfi,
            (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_DISPLAYNAME))
        {
            return shfi.szDisplayName;
        }
        return null;
    }

    public static string GetVolumeSerial(string pathName)
    {
        uint serial_number = 0;
        uint max_component_length = 0;
        var sb_volume_name = new StringBuilder(256);
        var file_system_flags = new UInt32();
        var sb_file_system_name = new StringBuilder(256);

        if (GetVolumeInformation(pathName, sb_volume_name,
            (UInt32)sb_volume_name.Capacity, ref serial_number,
            ref max_component_length, ref file_system_flags,
            sb_file_system_name,
            (UInt32)sb_file_system_name.Capacity) == 0)
        {
            return null;
        }
        else
        {
            return serial_number.ToString();
        }
    }

    public static void KeepSystemAwake()
    {
        tmr = new System.Timers.Timer(60000);
        tmr.Elapsed += new System.Timers.ElapsedEventHandler(tmr_Elapsed);

        SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
        tmr.Start();
    }

    static void tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
    }

    public static void AllowSystemSleep()
    {
        tmr.Stop();
        tmr = null;

        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }

    public static List<ExplorerWindow> GetWindowsExplorerPaths()
    {
        var result = new List<ExplorerWindow>();

        var t = Type.GetTypeFromProgID("Shell.Application");
        dynamic o = Activator.CreateInstance(t);

        try
        {
            var ws = o.Windows();
            for (var i = 0; i < ws.Count; i++)
            {
                var ie = ws.Item(i);

                if (ie == null)
                {
                    continue;
                }

                var path = System.IO.Path.GetFileName((string)ie.FullName);
                if (path.ToLower() == "explorer.exe" && !string.IsNullOrEmpty(ie.LocationURL))
                {
                    var location = new Uri(ie.LocationURL).LocalPath;
                    result.Add(new ExplorerWindow(location, ie.document.folder.title));
                }
            }
        }
        finally
        {
            Marshal.FinalReleaseComObject(o);
        }

        return result;
    }

    public static bool IsDiskFull(Exception ex)
    {
        const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
        const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

        return ex.HResult == HR_ERROR_HANDLE_DISK_FULL
            || ex.HResult == HR_ERROR_DISK_FULL;
    }
}