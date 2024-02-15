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
using System.Text;

namespace Brightbits.BSH.Engine.Security;

public static class Crypto
{
    static readonly byte[] entropy = Encoding.Unicode.GetBytes("vUNHSdlkflk+#sdFwe48p");

    public static string EncryptString(string input)
    {
        return EncryptString(input, System.Security.Cryptography.DataProtectionScope.CurrentUser);
    }

    public static string EncryptString(string input, System.Security.Cryptography.DataProtectionScope scope)
    {
        var encryptedData = System.Security.Cryptography.ProtectedData.Protect(
            Encoding.Unicode.GetBytes(input),
            entropy,
            scope);
        return Convert.ToBase64String(encryptedData);
    }

    public static string DecryptString(string encryptedData)
    {
        return DecryptString(encryptedData, System.Security.Cryptography.DataProtectionScope.CurrentUser);
    }

    public static string DecryptString(string encryptedData, System.Security.Cryptography.DataProtectionScope scope)
    {
        try
        {
            var decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                Convert.FromBase64String(encryptedData),
                entropy,
                scope);
            return Encoding.Unicode.GetString(decryptedData);
        }
        catch
        {
            return "";
        }
    }
}
