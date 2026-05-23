// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Security;
using BSH.Main.Properties;
using System.Threading.Tasks;

namespace Brightbits.BSH.Main;

/// <summary>
/// WinForms adapter for durable backup password storage.
/// </summary>
public sealed class WinFormsStoredPasswordAdapter : IStoredPasswordAdapter
{
    public Task<string> GetPasswordAsync()
    {
        if (string.IsNullOrEmpty(Settings.Default.BackupPwd))
        {
            return Task.FromResult(string.Empty);
        }

        return Task.FromResult(Crypto.DecryptString(Settings.Default.BackupPwd));
    }

    public Task StorePasswordAsync(string password)
    {
        Settings.Default.BackupPwd = Crypto.EncryptString(password);
        Settings.Default.Save();
        return Task.CompletedTask;
    }
}
