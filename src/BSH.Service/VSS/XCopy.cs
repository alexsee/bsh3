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
        new XCopy().CopyInternal(source, destination, overwrite, nobuffering, null);
    }

    public static void Copy(string source, string destination, bool overwrite, bool nobuffering, EventHandler<ProgressChangedEventArgs> handler)
    {
        new XCopy().CopyInternal(source, destination, overwrite, nobuffering, handler);
    }

    private event EventHandler Completed;
    private event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    private int IsCancelled;
    private int FilePercentCompleted;

    private XCopy()
    {
        IsCancelled = 0;
    }

    private void CopyInternal(string source, string destination, bool overwrite, bool nobuffering, EventHandler<ProgressChangedEventArgs> handler)
    {
        try
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

            if (handler != null)
            {
                ProgressChanged += handler;
            }

            var result = CopyFileEx(source, destination, new CopyProgressRoutine(CopyProgressHandler), IntPtr.Zero, ref IsCancelled, copyFileFlags);
            if (!result)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        catch (Exception)
        {
            if (handler != null)
            {
                ProgressChanged -= handler;
            }

            throw;
        }
    }

    private void OnProgressChanged(double percent)
    {
        // only raise an event when progress has changed
        if ((int)percent > FilePercentCompleted)
        {
            FilePercentCompleted = (int)percent;

            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(FilePercentCompleted, null));
        }
    }

    private void OnCompleted()
    {
        Completed?.Invoke(this, EventArgs.Empty);
    }

    #region PInvoke

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref Int32 pbCancel, CopyFileFlags dwCopyFlags);

    private delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason,
                                                    IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

    private enum CopyProgressResult : uint
    {
        PROGRESS_CONTINUE = 0,
        PROGRESS_CANCEL = 1,
        PROGRESS_STOP = 2,
        PROGRESS_QUIET = 3
    }

    private enum CopyProgressCallbackReason : uint
    {
        CALLBACK_CHUNK_FINISHED = 0x00000000,
        CALLBACK_STREAM_SWITCH = 0x00000001
    }

    [Flags]
    private enum CopyFileFlags : uint
    {
        COPY_FILE_FAIL_IF_EXISTS = 0x00000001,
        COPY_FILE_NO_BUFFERING = 0x00001000,
        COPY_FILE_RESTARTABLE = 0x00000002,
        COPY_FILE_OPEN_SOURCE_FOR_WRITE = 0x00000004,
        COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 0x00000008
    }

    private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long streamByteTrans, uint dwStreamNumber,
                                                   CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
    {
        if (reason == CopyProgressCallbackReason.CALLBACK_CHUNK_FINISHED)
        {
            OnProgressChanged((transferred / (double)total) * 100.0);
        }

        if (transferred >= total)
        {
            OnCompleted();
        }

        return CopyProgressResult.PROGRESS_CONTINUE;
    }

    #endregion

}
