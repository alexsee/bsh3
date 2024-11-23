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