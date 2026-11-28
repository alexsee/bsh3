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
using Microsoft.Win32.SafeHandles;

namespace BSH.Service;

public class VSSService : IVSSRemoteObject
{
    private Exception? exception;

    public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination)
    {
        // The service folder is not an authorization input.
        _ = vssServiceFolder;
        this.exception = null;
        SafeAccessTokenHandle? caller = null;
        try
        {
            // A missing impersonation token is a denial, including when the process is LocalSystem.
            if (!VssCallerAuthorization.TryDuplicateCallerToken(out var duplicated) || duplicated == null)
            {
                this.exception = new UnauthorizedAccessException(VssCallerAuthorization.DeniedMessage);
                return false;
            }

            caller = duplicated;
            if (!VssCallerAuthorization.RevertToSelf())
            {
                this.exception = new UnauthorizedAccessException(VssCallerAuthorization.DeniedMessage);
                return false;
            }

            VssCallerAuthorization.CopyAuthorized(caller, source, destination);
            return true;
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }
        finally
        {
            VssCallerAuthorization.RevertToSelf();
            caller?.Dispose();
        }

        return false;
    }

    public Exception? GetException()
    {
        VssCallerAuthorization.RevertToSelf();
        return this.exception;
    }
}
