// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Text;

namespace Brightbits.BSH.Engine.Security;

public static class Hash
{
    public static string GetMD5Hash(string input)
    {
        // convert the input string to a byte array and compute the hash
#pragma warning disable CA5351
        var data = System.Security.Cryptography.MD5.HashData(Encoding.Default.GetBytes(input));
#pragma warning restore CA5351

        return Convert.ToHexStringLower(data);
    }
}
