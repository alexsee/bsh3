// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using BSH.Service.Shared;
using ServiceWire.NamedPipes;

namespace BSH.Service
{
    public class CustomNamedPipeServerStreamFactory : INamedPipeServerStreamFactory
    {
        private readonly PipeSecurity _pipeSecurity;

        public CustomNamedPipeServerStreamFactory()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

            _pipeSecurity = new PipeSecurity();
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                sid,
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow
            ));
        }

        public NamedPipeServerStream Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances,
          PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
        {
            return NamedPipeServerStreamAcl.Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options,
              inBufferSize, outBufferSize, _pipeSecurity);
        }
    }

    public sealed class WindowsBackgroundService : IHostedService
    {
        private readonly Task _completedTask = Task.CompletedTask;

        private readonly NpHost host;

        private readonly IVSSRemoteObject remoteObject;

        public WindowsBackgroundService()
        {
            // init remote object
            remoteObject = new VSSService();

            // create remote rpc server
            host = new NpHost("backupservicehome", streamFactory: new CustomNamedPipeServerStreamFactory());
            host.AddService(remoteObject);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            host.Open();
            return _completedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            host.Close();
            return _completedTask;
        }
    }
}