﻿// Copyright 2022 Alexander Seeliger
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
using System.Security;
using System.Security.Cryptography;

namespace Brightbits.BSH.Engine.Security;

public class Encryption
{
    private readonly byte[] mKeySalt = [0xA1, 0x41, 0xC4, 0xF5, 0x23, 0x70, 0xBF, 0x52];
    private readonly byte[] mIVSalt = [0x47, 0x80, 0x22, 0xFF, 0x12, 0xE7, 0xF1, 0x39];
    private int mBitLen;

    public int BitLen
    {
        get => mBitLen * 8;
        set
        {
            if (value % 8 == 1)
            {
                throw new Exception("Es wurde keine gültige Bitstärke für die Verschlüsselung angegeben!");
            }

            mBitLen = Convert.ToInt32(value / (double)8);
        }
    }

    private void CheckBitLen()
    {
        if (mBitLen == 0 || mBitLen != 16 || mBitLen != 24 || mBitLen != 32)
        {
            mBitLen = 32;
        }
    }

    public bool Encode(string SourceFile, string TargetFile, SecureString Password, int BufferSize = 4096)
    {
        try
        {
            using var InFileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using FileStream OutFileStream = new(TargetFile, FileMode.Create);

            var key = new Rfc2898DeriveBytes(Crypto.ToInsecureString(Password), mKeySalt, 1000, HashAlgorithmName.SHA1);
            var iv = new Rfc2898DeriveBytes(Crypto.ToInsecureString(Password), mIVSalt, 1000, HashAlgorithmName.SHA1);

            CheckBitLen();

            var aes = Aes.Create();

            using var CryptStream = new CryptoStream(OutFileStream, aes.CreateEncryptor(key.GetBytes(mBitLen), iv.GetBytes(16)), CryptoStreamMode.Write); // 16,24,32
            InFileStream.CopyTo(CryptStream, BufferSize);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Decode(string SourceFile, string TargetFile, SecureString Password, int BufferSize = 4096)
    {
        try
        {
            using var InFileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var OutFileStream = new FileStream(TargetFile, FileMode.Create);

            var key = new Rfc2898DeriveBytes(Crypto.ToInsecureString(Password), mKeySalt, 1000, HashAlgorithmName.SHA1);
            var iv = new Rfc2898DeriveBytes(Crypto.ToInsecureString(Password), mIVSalt, 1000, HashAlgorithmName.SHA1);

            CheckBitLen();

            var aes = Aes.Create();
            using var CryptStream = new CryptoStream(OutFileStream, aes.CreateDecryptor(key.GetBytes(mBitLen), iv.GetBytes(16)), CryptoStreamMode.Write);

            InFileStream.CopyTo(CryptStream, BufferSize);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
