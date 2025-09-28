// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Text;

namespace Brightbits.BSH.Engine.Security;

public static class Hash
{
    public static string GetMD5Hash(string input)
    {
        // convert the input string to a byte array and compute the hash
        // Note: MD5 is cryptographically weak and should be replaced with SHA-256 or better
        // This is kept for backward compatibility with existing encrypted backups
        var data = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes and create a string.
        var sBuilder = new StringBuilder();

        // loop through each byte of the hashed data and format each one as a hexadecimal string
        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string
        return sBuilder.ToString();
    }
}
