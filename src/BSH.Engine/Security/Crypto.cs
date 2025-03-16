// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

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
