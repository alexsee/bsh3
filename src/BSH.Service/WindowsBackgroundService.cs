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
            // Interactive callers and administrators may use the pipe. LocalSystem and the
            // service account keep FullControl so further instances can be created, including
            // under a custom account. The DACL is protected so the default ACL cannot keep
            // Authenticated Users.
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.InteractiveSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow));
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));
            using var identity = WindowsIdentity.GetCurrent();
            _pipeSecurity.AddAccessRule(new PipeAccessRule(
                identity.User ?? throw new InvalidOperationException("The service account has no Windows SID."),
                PipeAccessRights.FullControl,
                AccessControlType.Allow));
            _pipeSecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
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
            host = new CallerImpersonatingNpHost(pipeName, new ServiceWireLogger(logger),
                new CustomNamedPipeServerStreamFactory());
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

    // ServiceWire dispatches synchronously on the connection thread. Bind the pipe to
    // that dispatch so the RPC can impersonate its caller only for file I/O.
    internal sealed class CallerImpersonatingNpHost : NpHost
    {
        private const int MaxConnections = 254;

        private readonly INamedPipeServerStreamFactory streamFactory;

        private readonly CancellationTokenSource stopping = new();

        [ThreadStatic]
        private static VssPipeConnection? currentConnection;

        internal static VssPipeConnection? CurrentConnection => currentConnection;

        private Thread? listener;

        private int disposeOnce;

        public CallerImpersonatingNpHost(string pipeName, ILog log, INamedPipeServerStreamFactory streamFactory)
            : base(pipeName, log, streamFactory: streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        protected override void StartListener()
        {
            var cancellation = stopping.Token;
            listener = new Thread(() => Listen(cancellation)) { IsBackground = true };
            listener.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref disposeOnce, 1) == 0)
            {
                stopping.Cancel();
                listener?.Join();
                stopping.Dispose();
            }

            base.Dispose(disposing);
        }

        private void Listen(CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    AcceptNextClient(cancellation);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception error)
            {
                Log.Error("VSS pipe listener failed: {0}", error.Message);
                SetStatus(HostStatus.Faulted);
            }
        }

        private void AcceptNextClient(CancellationToken cancellation)
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = streamFactory.Create(PipeName, PipeDirection.InOut, MaxConnections, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous, 8192, 8192);
                pipe.WaitForConnectionAsync(cancellation).GetAwaiter().GetResult();
                cancellation.ThrowIfCancellationRequested();

                var connected = pipe;
                new Thread(() => Serve(connected)) { IsBackground = true }.Start();
                pipe = null;
            }
            catch (IOException exception) when ((exception.HResult & 0xFFFF) == 231)
            {
                // All pipe instances are in use. Wait for capacity or shutdown.
                cancellation.WaitHandle.WaitOne(50);
            }
            finally
            {
                pipe?.Dispose();
            }
        }

        private void Serve(NamedPipeServerStream pipe)
        {
            using (pipe)
            {
                currentConnection = new VssPipeConnection(pipe);
                try
                {
                    ProcessRequest(new BufferedStream(pipe), new BufferedStream(pipe));
                }
                catch (Exception error)
                {
                    Log.Debug("VSS pipe connection ended: {0}", error.GetType().Name);
                }
                finally
                {
                    currentConnection = null;
                }
            }
        }
    }
}
