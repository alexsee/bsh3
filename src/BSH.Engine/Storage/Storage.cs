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

namespace Brightbits.BSH.Engine.Storage;

public abstract class Storage
{
    protected static string CleanRemoteFileName(string remoteFile)
    {
        ArgumentNullException.ThrowIfNull(remoteFile);

        var result = remoteFile;

        if (remoteFile.StartsWith('\\'))
        {
            if (remoteFile.Length > 1)
            {
                result = remoteFile.Substring(1);
            }
            else
            {
                return "";
            }
        }

        return result;
    }

    protected static string GetLocalFileName(string localFile)
    {
        ArgumentNullException.ThrowIfNull(localFile);

        if (localFile.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
        {
            return localFile;
        }

        return "\\\\?\\" + localFile;
    }
}
