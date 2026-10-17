// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using BSH.Service;
using BSH.Service.Shared;
using NUnit.Framework;
using ServiceWire.NamedPipes;

namespace BSH.Test;

[Platform("Win")]
public class ServiceWirePipeTests
{
    [Test]
    public void PipeGrantsUsersReadWriteAndServiceAccountFullControl()
    {
        var factory = new CustomNamedPipeServerStreamFactory();
        using var pipe = factory.Create("bsh-acl-" + Guid.NewGuid().ToString("N"),
            PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.None, 8192, 8192);
        var rules = pipe.GetAccessControl().GetAccessRules(true, false, typeof(SecurityIdentifier))
            .Cast<PipeAccessRule>().ToArray();

        Assert.Multiple(() =>
        {
            AssertAllowed(rules, new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite);
            AssertAllowed(rules, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl);
            AssertAllowed(rules, WindowsIdentity.GetCurrent().User!, PipeAccessRights.FullControl);
        });
    }

    [Test]
    public void VssContractRoundTripsErrorsAndReconnectsOverNamedPipe()
    {
        var pipeName = "bsh-rpc-" + Guid.NewGuid().ToString("N");
        using var host = new NpHost(pipeName, streamFactory: new CustomNamedPipeServerStreamFactory());
        host.AddService<IVSSRemoteObject>(new FailedVssService());
        host.Open();

        for (var connection = 0; connection < 2; connection++)
        {
            using var client = new NpClient<IVSSRemoteObject>(new NpEndPoint(pipeName));
            Assert.That(client.Proxy.CopyFileWithVSS("service", "source", "destination"), Is.False);
            var error = client.Proxy.GetException();
            Assert.That(error, Is.TypeOf<InvalidOperationException>());
            Assert.That(error!.Message, Is.EqualTo("Snapshot failed"));
            Assert.That(error.InnerException, Is.TypeOf<UnauthorizedAccessException>());
            Assert.That(error.InnerException!.Message, Is.EqualTo("Access denied"));
            Assert.That(client.Proxy.CopyFileWithVSS("service", "source", "destination"), Is.False);
        }
    }

    private static void AssertAllowed(PipeAccessRule[] rules, SecurityIdentifier sid, PipeAccessRights rights)
    {
        Assert.That(rules.Any(rule => rule.IdentityReference.Equals(sid)
            && rule.AccessControlType == AccessControlType.Allow
            && (rule.PipeAccessRights & rights) == rights), Is.True, $"Missing {rights} for {sid}");
    }

    private sealed class FailedVssService : IVSSRemoteObject
    {
        public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination) => false;

        public Exception GetException() => new InvalidOperationException("Snapshot failed",
            new UnauthorizedAccessException("Access denied"));
    }
}
