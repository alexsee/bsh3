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

namespace BSH.Service.VSS;

public static class XCopy
{
    private const uint CopyFileRestartable = 0x00000002;

    public static void Copy(string source, string destination)
    {
        if (!CopyFileExW(source, destination, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CopyFileRestartable))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CopyFileExW(string source, string destination, IntPtr progressRoutine, IntPtr data, IntPtr cancel, uint copyFlags);
}
