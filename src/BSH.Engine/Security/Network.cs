// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.ComponentModel;
using static Brightbits.BSH.Engine.Win32Stuff;

namespace Brightbits.BSH.Engine.Security;

public class NetworkConnection : IDisposable
{
    public string RemoteShare { get; set; } = null;

    /// <summary>
    /// Opens a temporary SMB connection. By default <paramref name="remotePassword"/> is
    /// treated as DPAPI LocalMachine ciphertext (config <c>UNCPassword</c>). Pass
    /// <paramref name="passwordIsEncrypted"/> as false for UI plaintext (setup / media-switch probes).
    /// </summary>
    public NetworkConnection(string remoteHost, string remoteUser, string remotePassword, bool passwordIsEncrypted = true)
    {
        if (string.IsNullOrEmpty(remoteUser) || string.IsNullOrEmpty(remotePassword) || !Uri.TryCreate(remoteHost, UriKind.Absolute, out var loc) || !loc.IsUnc)
        {
            return;
        }

        var auth = loc.Host;
        var segments = loc.Segments;

        // Config-stored passwords are DPAPI ciphertext; UI probes pass plaintext.
        var pw = passwordIsEncrypted
            ? Crypto.DecryptString(remotePassword, System.Security.Cryptography.DataProtectionScope.LocalMachine)
            : remotePassword;

        if (pw.Length == 0)
        {
            return;
        }

        // expected format is '\\machine\share'
        this.RemoteShare = String.Format(@"\\{0}\{1}", auth, segments[1].Trim('\\', '/'));
        this.Connect(remoteUser, pw);
    }

    ~NetworkConnection()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        Disconnect();
    }

    void Connect(string remoteUser, string remotePassword)
    {
        var netResource = new NetResource()
        {
            Scope = ResourceScope.GlobalNetwork,
            ResourceType = ResourceType.Disk,
            DisplayType = ResourceDisplaytype.Share,
            RemoteName = this.RemoteShare
        };

        // user must be qualified 'authority\user'
        if (remoteUser.IndexOf('\\') < 0)
        {
            remoteUser = String.Format(@"{0}\{1}", new Uri(RemoteShare).Host, remoteUser);
        }

        var dwResult = WNetAddConnection2(netResource, remotePassword, remoteUser, 0);

        // are we already connected?
        if (1219 == dwResult)
        {
            RemoteShare = null;
            return;
        }

        if (0 != dwResult)
        {
            throw new Win32Exception(dwResult);
        }
    }

    void Disconnect()
    {
        if (RemoteShare != null)
        {
            WNetCancelConnection2(this.RemoteShare, 0, true);
        }
    }
}
