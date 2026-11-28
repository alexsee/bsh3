// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
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
    public void PipeDeniesAuthenticatedUsersAndGrantsInteractiveClients()
    {
        var factory = new CustomNamedPipeServerStreamFactory();
        using var pipe = factory.Create("bsh-acl-" + Guid.NewGuid().ToString("N"),
            PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.None, 8192, 8192);
        var rules = pipe.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier))
            .Cast<PipeAccessRule>().ToArray();

        Assert.Multiple(() =>
        {
            AssertNotAllowed(rules, new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite);
            AssertAllowed(rules, new SecurityIdentifier(WellKnownSidType.InteractiveSid, null), PipeAccessRights.ReadWrite);
            AssertAllowed(rules, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), PipeAccessRights.ReadWrite);
            AssertAllowed(rules, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl);
            AssertAllowed(rules, WindowsIdentity.GetCurrent().User!, PipeAccessRights.FullControl);
        });
    }

    [Test]
    public void CopyWithoutAnImpersonationTokenIsDenied()
    {
        var service = new VSSService();

        Assert.That(service.CopyFileWithVSS("service", @"C:\bsh-vss-no-token-source", @"C:\bsh-vss-no-token-destination"), Is.False);
        Assert.That(service.GetException(), Is.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public void CallerTokenAccessCheckFollowsReadAndWriteAces()
    {
        var root = Path.Combine(Path.GetTempPath(), "bsh-vss-acl-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var readable = Path.Combine(root, "readable.txt");
        var deniedRead = Path.Combine(root, "denied-read.txt");
        var deniedWrite = Path.Combine(root, "denied-write");
        File.WriteAllText(readable, "readable");
        File.WriteAllText(deniedRead, "secret");
        Directory.CreateDirectory(deniedWrite);

        try
        {
            SetDeny(deniedRead, FileSystemRights.Read, deny: true);
            SetDeny(deniedWrite, FileSystemRights.Write, deny: true);

            using var identity = WindowsIdentity.GetCurrent();
            var readableRights = VssCallerAuthorization.Check(identity.AccessToken, readable, readable);
            var unreadable = VssCallerAuthorization.Check(identity.AccessToken, deniedRead, readable);
            var unwritable = VssCallerAuthorization.Check(identity.AccessToken, readable, deniedWrite);

            Assert.Multiple(() =>
            {
                Assert.That(readableRights.CanReadSource, Is.True);
                Assert.That(unreadable.CanReadSource, Is.False);
                Assert.That(unwritable.CanWriteDestination, Is.False);
                Assert.That(VssCopyPolicy.IsAuthorized(new CallerFileRights(true, true)), Is.True);
                Assert.That(VssCopyPolicy.IsAuthorized(new CallerFileRights(true, false)), Is.False);
                Assert.That(VssCopyPolicy.IsAuthorized(new CallerFileRights(false, true)), Is.False);
                Assert.That(VssCopyPolicy.IsAuthorized(unreadable), Is.False);
                Assert.That(VssCopyPolicy.IsAuthorized(unwritable), Is.False);
            });
        }
        finally
        {
            TryClearDeny(deniedRead, FileSystemRights.Read);
            TryClearDeny(deniedWrite, FileSystemRights.Write);
            try
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
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

    private static void AssertNotAllowed(PipeAccessRule[] rules, SecurityIdentifier sid, PipeAccessRights rights)
    {
        Assert.That(rules.Any(rule => rule.IdentityReference.Equals(sid)
            && rule.AccessControlType == AccessControlType.Allow
            && (rule.PipeAccessRights & rights) == rights), Is.False, $"Unexpected {rights} for {sid}");
    }

    private static void SetDeny(string path, FileSystemRights rights, bool deny)
    {
        var rule = new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, rights, AccessControlType.Deny);
        if (Directory.Exists(path))
        {
            var directory = new DirectoryInfo(path);
            var security = directory.GetAccessControl();
            if (deny)
            {
                security.AddAccessRule(rule);
            }
            else
            {
                security.RemoveAccessRule(rule);
            }

            directory.SetAccessControl(security);
            return;
        }

        var file = new FileInfo(path);
        var fileSecurity = file.GetAccessControl();
        if (deny)
        {
            fileSecurity.AddAccessRule(rule);
        }
        else
        {
            fileSecurity.RemoveAccessRule(rule);
        }

        file.SetAccessControl(fileSecurity);
    }

    private static void TryClearDeny(string path, FileSystemRights rights)
    {
        try
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                SetDeny(path, rights, deny: false);
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
    }

    private sealed class FailedVssService : IVSSRemoteObject
    {
        public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination) => false;

        public Exception GetException() => new InvalidOperationException("Snapshot failed",
            new UnauthorizedAccessException("Access denied"));
    }
}
