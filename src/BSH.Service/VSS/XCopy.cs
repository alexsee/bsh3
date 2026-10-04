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


/// <summary>
/// PInvoke wrapper for CopyEx
/// http://msdn.microsoft.com/en-us/library/windows/desktop/aa363852.aspx
/// </summary>
public class XCopy
{
    public static void Copy(string source, string destination, bool overwrite, bool nobuffering)
    {
        new XCopy().CopyInternal(source, destination, overwrite, nobuffering);
    }

    private int IsCancelled;

    private XCopy()
    {
        IsCancelled = 0;
    }

    private void CopyInternal(string source, string destination, bool overwrite, bool nobuffering)
    {
        var copyFileFlags = CopyFileFlags.COPY_FILE_RESTARTABLE;
        if (!overwrite)
        {
            copyFileFlags |= CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS;
        }

        if (nobuffering)
        {
            copyFileFlags |= CopyFileFlags.COPY_FILE_NO_BUFFERING;
        }

        var result = CopyFileEx(source, destination, new CopyProgressRoutine(CopyProgressHandler), IntPtr.Zero, ref IsCancelled, copyFileFlags);
        if (!result)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    #region PInvoke

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

    private delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason,
                                                    IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

    private enum CopyProgressResult : uint
    {
        PROGRESS_CONTINUE = 0
    }

    private enum CopyProgressCallbackReason : uint
    {
        CALLBACK_CHUNK_FINISHED = 0x00000000
    }

    [Flags]
    private enum CopyFileFlags : uint
    {
        COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
        COPY_FILE_NO_BUFFERING = 0x00001000,
        COPY_FILE_RESTARTABLE = 0x00000002
    }

    private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long streamByteTrans, uint dwStreamNumber,
                                                   CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
    {
        return CopyProgressResult.PROGRESS_CONTINUE;
    }

    #endregion

}
