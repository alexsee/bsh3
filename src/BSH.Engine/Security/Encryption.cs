// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using System.Security.Cryptography;

namespace Brightbits.BSH.Engine.Security;

public class Encryption
{
    // TODO: remove the hardcoded salts and use cryptographically secure random generation
    private readonly byte[] mKeySalt = [0xA1, 0x41, 0xC4, 0xF5, 0x23, 0x70, 0xBF, 0x52];
    private readonly byte[] mIVSalt = [0x47, 0x80, 0x22, 0xFF, 0x12, 0xE7, 0xF1, 0x39];

    public bool Encode(string sourceFile, string targetFile, string password, int bufferSize = 4096)
    {
        try
        {
            using var InFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using FileStream OutFileStream = new(targetFile, FileMode.Create);

            var key = new Rfc2898DeriveBytes(password, mKeySalt, 100000, HashAlgorithmName.SHA256);
            var iv = new Rfc2898DeriveBytes(password, mIVSalt, 100000, HashAlgorithmName.SHA256);

            var aes = Aes.Create();

            using var CryptStream = new CryptoStream(OutFileStream, aes.CreateEncryptor(key.GetBytes(32), iv.GetBytes(16)), CryptoStreamMode.Write); // 16,24,32
            InFileStream.CopyTo(CryptStream, bufferSize);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Decode(string sourceFile, string targetFile, string password, int bufferSize = 4096)
    {
        try
        {
            using var InFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var OutFileStream = new FileStream(targetFile, FileMode.Create);

            var key = new Rfc2898DeriveBytes(password, mKeySalt, 100000, HashAlgorithmName.SHA256);
            var iv = new Rfc2898DeriveBytes(password, mIVSalt, 100000, HashAlgorithmName.SHA256);

            var aes = Aes.Create();
            using var CryptStream = new CryptoStream(OutFileStream, aes.CreateDecryptor(key.GetBytes(32), iv.GetBytes(16)), CryptoStreamMode.Write);

            InFileStream.CopyTo(CryptStream, bufferSize);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
