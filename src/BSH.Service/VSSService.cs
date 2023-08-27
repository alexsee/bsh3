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

using BSH.Service.Shared;
using BSH.Service.VSS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BSH.Service
{
    public class VSSService : IVSSRemoteObject
    {
        private Exception exception = null;

        public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination)
        {
            var fileInfo = new FileInfo(source);

            try
            {
                using (var vss = new VssBackup())
                {
                    vss.Setup(fileInfo.Directory.Root.Name);

                    var snapshotPath = vss.GetSnapshotPath(source);
                    XCopy.Copy(snapshotPath, destination, true, false);
                }

                return true;
            }
            catch (Exception ex)
            {
                this.exception = ex;
            }

            return false;
        }

        public Exception GetException()
        {
            return this.exception;
        }
    }
}
