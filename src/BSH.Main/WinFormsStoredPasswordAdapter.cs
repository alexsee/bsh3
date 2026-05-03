// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Brightbits.BSH.Engine.Runtime.Ports;
using Brightbits.BSH.Engine.Security;
using BSH.Main.Properties;

namespace Brightbits.BSH.Main;

/// <summary>
/// WinForms adapter for durable backup password storage.
/// </summary>
public sealed class WinFormsStoredPasswordAdapter : IStoredPasswordAdapter
{
    public string GetPassword()
    {
        if (string.IsNullOrEmpty(Settings.Default.BackupPwd))
        {
            return string.Empty;
        }

        return Crypto.DecryptString(Settings.Default.BackupPwd);
    }

    public void StorePassword(string password)
    {
        Settings.Default.BackupPwd = Crypto.EncryptString(password);
        Settings.Default.Save();
    }
}
