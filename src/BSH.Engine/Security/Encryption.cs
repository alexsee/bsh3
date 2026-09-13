// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.Security.Cryptography;
using Serilog;

namespace Brightbits.BSH.Engine.Security;

public class Encryption
{
    private static readonly ILogger Logger = Log.ForContext<Encryption>();

    // TODO: remove the hardcoded salts
    private readonly byte[] mKeySalt = [0xA1, 0x41, 0xC4, 0xF5, 0x23, 0x70, 0xBF, 0x52];
    private readonly byte[] mIVSalt = [0x47, 0x80, 0x22, 0xFF, 0x12, 0xE7, 0xF1, 0x39];

    public bool Encode(string sourceFile, string targetFile, string password, int bufferSize = 4096)
    {
        return CryptFile(sourceFile, targetFile, password, encrypt: true, bufferSize);
    }

    public bool Decode(string sourceFile, string targetFile, string password, int bufferSize = 4096)
    {
        return CryptFile(sourceFile, targetFile, password, encrypt: false, bufferSize);
    }

    private bool CryptFile(string sourceFile, string targetFile, string password, bool encrypt, int bufferSize)
    {
        try
        {
            DeriveKeyMaterial(password, out var key, out var iv);

            using var aes = Aes.Create();
            using var transform = encrypt ? aes.CreateEncryptor(key, iv) : aes.CreateDecryptor(key, iv);
            using var inFileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var outFileStream = new FileStream(targetFile, FileMode.Create);
            using var cryptStream = new CryptoStream(outFileStream, transform, CryptoStreamMode.Write);
            inFileStream.CopyTo(cryptStream, bufferSize);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "File {SourceFile} could not be {Operation}.", sourceFile, encrypt ? "encrypted" : "decrypted");
            TryDeletePartialFile(targetFile);
            return false;
        }
    }

    private static void TryDeletePartialFile(string targetFile)
    {
        try
        {
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Partial output file {TargetFile} could not be deleted.", targetFile);
        }
    }

    private void DeriveKeyMaterial(string password, out byte[] key, out byte[] iv)
    {
        // Iteration count must stay at 1000 to remain compatible with existing encrypted backups.
        key = Rfc2898DeriveBytes.Pbkdf2(password, mKeySalt, 1000, HashAlgorithmName.SHA1, 32);
        iv = Rfc2898DeriveBytes.Pbkdf2(password, mIVSalt, 1000, HashAlgorithmName.SHA1, 16);
    }
}
