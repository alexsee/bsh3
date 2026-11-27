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

using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using BSH.Service.Shared;
using ServiceWire;
using ServiceWire.NamedPipes;

namespace BSH.Service
{
    public class CustomNamedPipeServerStreamFactory : INamedPipeServerStreamFactory
    {
        private readonly PipeSecurity _pipeSecurity;

        public CustomNamedPipeServerStreamFactory()
        {
            _pipeSecurity = new PipeSecurity();
            // This endpoint performs file I/O as the service account. Only callers
            // that already have local administrative privileges may use it.
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Deny));
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                // ServiceWire opens the client with GENERIC_READ | GENERIC_WRITE.
                PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance | PipeAccessRights.Synchronize,
                AccessControlType.Allow));

            // Allow the service to create further pipe instances, including
            // when installed under a custom account.
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));
            using var identity = WindowsIdentity.GetCurrent();
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                identity.User ?? throw new InvalidOperationException("The service account has no Windows SID."),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));
        }

        public NamedPipeServerStream Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances,
          PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
        {
            return NamedPipeServerStreamAcl.Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options,
              inBufferSize, outBufferSize, _pipeSecurity);
        }
    }

    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly NpHost host;

        private readonly IVSSRemoteObject remoteObject;

        public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger)
        {
            // init remote object
            remoteObject = new VSSService();

            // Preview installs a vss.pipe beside this exe so it does not bind the stable pipe.
            var pipeName = VssPipeNames.ReadInstalledPipeName(AppContext.BaseDirectory);
            host = new NpHost(pipeName, log: new ServiceWireLogger(logger),
                streamFactory: new CustomNamedPipeServerStreamFactory());
            host.AddService(remoteObject);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                host.Open();
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                do
                {
                    // A listener failure happens on ServiceWire's own thread. Surface
                    // it to the host so the Windows service cannot stay silently broken.
                    if (host.Status == HostStatus.Faulted)
                    {
                        throw new InvalidOperationException($"The VSS pipe listener '{host.PipeName}' failed. See the ServiceWire log for details.");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            finally
            {
                host.Close();
            }
        }
    }
}
