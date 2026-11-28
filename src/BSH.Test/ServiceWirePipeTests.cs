// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using Brightbits.BSH.Engine.Services;
using BSH.Service;
using BSH.Service.Shared;
using Microsoft.Extensions.Logging.Abstractions;
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
        var interactive = new SecurityIdentifier(WellKnownSidType.InteractiveSid, null);

        Assert.Multiple(() =>
        {
            AssertNotAllowed(rules, new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite);
            AssertAllowed(rules, interactive, PipeAccessRights.ReadWrite);
            AssertNotAllowed(rules, interactive, PipeAccessRights.CreateNewInstance);
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
    public void VssPipeCopiesReadableFilesAndHonorsFileAcls()
    {
        var pipeName = "bsh-rpc-" + Guid.NewGuid().ToString("N");
        var root = Directory.CreateTempSubdirectory("bsh-vss-acl-");
        var readable = Path.Combine(root.FullName, "readable.txt");
        var deniedRead = Path.Combine(root.FullName, "denied-read.txt");
        var deniedWrite = Path.Combine(root.FullName, "denied-write.txt");
        var output = Path.Combine(root.FullName, "output.txt");
        File.WriteAllText(readable, "readable");
        File.WriteAllText(deniedRead, "private");
        File.WriteAllText(deniedWrite, "original");

        try
        {
            SetFileDeny(deniedRead, FileSystemRights.ReadData, deny: true);
            SetFileDeny(deniedWrite, FileSystemRights.WriteData, deny: true);
            var logger = new ServiceWireLogger(NullLogger<WindowsBackgroundService>.Instance);
            using var host = new CallerImpersonatingNpHost(pipeName, logger, new CustomNamedPipeServerStreamFactory());
            host.AddService<IVSSRemoteObject>(new VSSService((source, copy) =>
            {
                using var identity = WindowsIdentity.GetCurrent(ifImpersonating: true);
                if (identity != null)
                {
                    throw new InvalidOperationException("The VSS requester ran as the pipe caller.");
                }

                copy(source);
            }));
            host.Open();

            var client = new VolumeShadowCopyClient(pipeName);
            Assert.That(client.CopyFile(readable, output), Is.True);
            Assert.That(File.ReadAllText(output), Is.EqualTo("readable"));
            Assert.That(client.CopyFile(deniedRead, output), Is.False);
            Assert.That(File.ReadAllText(output), Is.EqualTo("readable"));
            Assert.That(client.CopyFile(readable, deniedWrite), Is.False);
            Assert.That(File.ReadAllText(deniedWrite), Is.EqualTo("original"));
        }
        finally
        {
            SetFileDeny(deniedRead, FileSystemRights.ReadData, deny: false);
            SetFileDeny(deniedWrite, FileSystemRights.WriteData, deny: false);
            root.Delete(recursive: true);
        }
    }

    [Test]
    [Explicit("Requires the updated VSS service installed as LocalSystem and a standard-user test runner.")]
    public void StandardUserCanBackUpALockedFileThroughTheInstalledVssService()
    {
        using var identity = WindowsIdentity.GetCurrent();
        Assert.That(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator), Is.False);

        var root = Directory.CreateTempSubdirectory("bsh-vss-locked-");
        var source = Path.Combine(root.FullName, "locked.txt");
        var destination = Path.Combine(root.FullName, "backup.txt");
        try
        {
            File.WriteAllText(source, "locked file contents");
            using var locked = File.Open(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            Assert.That(new VolumeShadowCopyClient().CopyFile(source, destination), Is.True);
            Assert.That(File.ReadAllText(destination), Is.EqualTo("locked file contents"));
        }
        finally
        {
            root.Delete(recursive: true);
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

    private static void SetFileDeny(string path, FileSystemRights rights, bool deny)
    {
        using var identity = WindowsIdentity.GetCurrent();
        var rule = new FileSystemAccessRule(identity.User!, rights, AccessControlType.Deny);
        var file = new FileInfo(path);
        var security = file.GetAccessControl();
        if (deny)
        {
            security.AddAccessRule(rule);
        }
        else
        {
            security.RemoveAccessRule(rule);
        }

        file.SetAccessControl(security);
    }

    private sealed class FailedVssService : IVSSRemoteObject
    {
        public bool CopyFileWithVSS(string vssServiceFolder, string source, string destination) => false;

        public Exception GetException() => new InvalidOperationException("Snapshot failed",
            new UnauthorizedAccessException("Access denied"));
    }
}
