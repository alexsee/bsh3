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
using System.Security;
using System.Security.Cryptography;

namespace Brightbits.BSH.Engine.Security;

/// <summary>
/// ''' Das ist eine Kryptographieklasse die die Verschlüsselungsalgorithmen aus den .NET Framework
/// ''' zusammenfasst und in einfach anzuwendenen Funktionen kapselt.
/// ''' </summary>
/// ''' <remarks>Tim Hartwig</remarks>
public class Encryption
{
    private readonly byte[] mKeySalt = new byte[] { 0xA1, 0x41, 0xC4, 0xF5, 0x23, 0x70, 0xBF, 0x52 };
    private readonly byte[] mIVSalt = new byte[] { 0x47, 0x80, 0x22, 0xFF, 0x12, 0xE7, 0xF1, 0x39 };
    private int mBitLen;

    public event ProgressErrorEventHandler ProgressError;

    public delegate void ProgressErrorEventHandler(object sender, Exception ex);

    /// <summary>
    ///     ''' Gibt die Verschlüsselungsstärke in Bits an:
    ///     ''' DES = 64;  TDES = 128,192;  RIJNDAEL = 128,192,256;  RC2 = 40 bis 128 (8er Schritte)
    ///     ''' </summary>
    public int BitLen
    {
        get
        {
            return mBitLen * 8;
        }
        set
        {
            if (value % 8 == 1)
            {
                throw new Exception("Es wurde keine gültige Bitstärke für die Verschlüsselung angegeben!");
            }

            mBitLen = Convert.ToInt32(value / (double)8);
        }
    }

    public enum ALGO
    {
        /// <summary>
        ///         ''' 128 Bit, 192 Bit, 256 Bit
        ///         ''' </summary>
        RIJNDAEL = 2
    }

    private void CheckBitLen()
    {
        if (mBitLen == 0 || mBitLen != 16 || mBitLen != 24 || mBitLen != 32)
        {
            mBitLen = 32;
        }
    }

    /// <summary>
    ///      Diese Funktion verschlüsselt eine Datei und speichert es in eine andere Datei (HDD -> HDD)
    ///     </summary>
    public bool Encode(string SourceFile, string TargetFile, SecureString Password, int BufferSize = 4096)
    {
        try
        {
            using (var InFileStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (FileStream OutFileStream = new(TargetFile, FileMode.Create))
                {
                    CryptoStream CryptStream = null;

                    var Data = new byte[BufferSize - 1 + 1];
                    long Buffer;
                    long BufferCount = 0;
                    var FileLen = InFileStream.Length;

                    Rfc2898DeriveBytes MakeKey = new(Crypto.ToInsecureString(Password), mKeySalt);
                    Rfc2898DeriveBytes MakeIV = new(Crypto.ToInsecureString(Password), mIVSalt);

                    // Prüfen ob die Bitstärke mit dem gewählten Algorithmus übereinstimmt und evtl. anpassen
                    CheckBitLen();

                    RijndaelManaged RIJNDAEL = new();
                    CryptStream = new CryptoStream(OutFileStream, RIJNDAEL.CreateEncryptor(MakeKey.GetBytes(mBitLen), MakeIV.GetBytes(16)), CryptoStreamMode.Write); // 16,24,32
                    do
                    {
                        if (BufferCount >= FileLen - BufferSize)
                        {
                            Data = new byte[Convert.ToInt32(FileLen - BufferCount) + 1];
                            Buffer = InFileStream.Read(Data, 0, Convert.ToInt32(FileLen - BufferCount));
                            CryptStream.Write(Data, 0, Convert.ToInt32(Buffer));
                            break;
                        }

                        Buffer = InFileStream.Read(Data, 0, BufferSize);
                        CryptStream.Write(Data, 0, BufferSize);
                        BufferCount += Buffer;
                    }
                    while (true);

                    CryptStream.Close();
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            ProgressError?.Invoke(this, ex);
            return false;
        }
    }

    /// <summary>
    ///     ''' Diese Funktion entschlüsselt eine Datei und speichert es in eine andere Datei (HDD -> HDD)
    ///     ''' </summary>
    public bool Decode(string SourceFile, string TargetFile, SecureString Password, int BufferSize = 4096)
    {
        try
        {
            FileStream InFileStream = new(SourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            FileStream OutFileStream = new(TargetFile, FileMode.Create);
            CryptoStream CryptStream = null;

            var Data = new byte[BufferSize - 1 + 1];
            long Buffer;
            long BufferCount = 0;
            var FileLen = InFileStream.Length;

            Rfc2898DeriveBytes MakeKey = new(Crypto.ToInsecureString(Password), mKeySalt);
            Rfc2898DeriveBytes MakeIV = new(Crypto.ToInsecureString(Password), mIVSalt);

            // Prüfen ob die Bitstärke mit dem gewählten Algorithmus übereinstimmt und evtl. anpassen
            CheckBitLen();

            RijndaelManaged RIJNDAEL = new();
            CryptStream = new CryptoStream(InFileStream, RIJNDAEL.CreateDecryptor(MakeKey.GetBytes(mBitLen), MakeIV.GetBytes(16)), CryptoStreamMode.Read);

            do
            {
                if (BufferCount >= FileLen - BufferSize)
                {
                    Data = new byte[Convert.ToInt32(FileLen - BufferCount) + 1];
                    Buffer = CryptStream.Read(Data, 0, Convert.ToInt32(FileLen - BufferCount));
                    OutFileStream.Write(Data, 0, Convert.ToInt32(Buffer));
                    break;
                }

                Buffer = CryptStream.Read(Data, 0, BufferSize);
                OutFileStream.Write(Data, 0, BufferSize);
                BufferCount += Buffer;
            }
            while (true);

            OutFileStream.Close();
            CryptStream.Close();
            InFileStream.Close();
            return true;
        }
        catch (Exception ex)
        {
            ProgressError?.Invoke(this, ex);
            return false;
        }
    }
}
