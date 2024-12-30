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
using System.IO;
using BSH.Service.Shared;
using Serilog;
using ServiceWire.NamedPipes;

namespace Brightbits.BSH.Engine.Services;

public static class VolumeShadowCopyService
{
    public static bool CopyFile(string fileName, string destFileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(destFileName);

        try
        {
            using var npClient = new NpClient<IVSSRemoteObject>(new NpEndPoint("backupservicehome"));

            var serviceFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var destination = destFileName.Replace("\\\\", "\\", StringComparison.OrdinalIgnoreCase);

            // copy file
            var result = npClient.Proxy.CopyFileWithVSS(serviceFilePath, fileName, destination);

            if (!result)
            {
                Log.Error(npClient.Proxy.GetException(), "Could not copy file via VSS.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not communicate with Backup Service Home Service");
            return false;
        }
    }
}