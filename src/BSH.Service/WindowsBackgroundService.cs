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
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using BSH.Service.Shared;
using Microsoft.Win32.SafeHandles;
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
            // NamedPipeServerStream is sealed. CallerImpersonatingNpHost impersonates on a wrapping
            // read stream. The DACL is applied as the pipe is created.
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

    /// <summary>
    /// NpHost reads the pipe and then invokes the service on that same thread, but its
    /// connection handler calls the base ProcessRequest and NamedPipeServerStream is sealed.
    /// This host accepts the pipe and impersonates on the stream ServiceWire reads.
    /// </summary>
    internal sealed class CallerImpersonatingNpHost : NpHost
    {
        private const int MaxConnections = 254;

        private readonly INamedPipeServerStreamFactory streamFactory;

        private readonly ManualResetEventSlim stopped = new(false);

        private volatile bool running;

        private int disposeOnce;

        public CallerImpersonatingNpHost(string pipeName, ILog log, INamedPipeServerStreamFactory streamFactory)
            : base(pipeName, log, streamFactory: streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        protected override void StartListener()
        {
            stopped.Reset();
            running = true;
            Task.Factory.StartNew(Listen, TaskCreationOptions.LongRunning);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref disposeOnce, 1) == 0)
            {
                StopListening();
            }

            base.Dispose(disposing);
        }

        private void Listen()
        {
            try
            {
                while (running)
                {
                    AcceptNextClient();
                }
            }
            catch (Exception)
            {
                SetStatus(HostStatus.Faulted);
            }
            finally
            {
                running = false;
                stopped.Set();
            }
        }

        private void AcceptNextClient()
        {
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = streamFactory.Create(PipeName, PipeDirection.InOut, MaxConnections, PipeTransmissionMode.Byte,
                    PipeOptions.None, 8192, 8192);
                pipe.WaitForConnection();
                if (!running)
                {
                    return;
                }

                var connected = pipe;
                pipe = null;
                Task.Factory.StartNew(() => Serve(connected));
            }
            catch (IOException exception) when (IsPipeBusy(exception) && running)
            {
                Thread.Sleep(50);
            }
            finally
            {
                pipe?.Dispose();
            }
        }

        private void Serve(NamedPipeServerStream pipe)
        {
            try
            {
                var reading = new CallerImpersonatingReadStream(new BufferedStream(pipe), pipe);
                ProcessRequest(reading, new BufferedStream(pipe));
            }
            catch (Exception)
            {
                // ProcessRequest already records client failures. One client must not stop the listener.
            }
            finally
            {
                VssCallerAuthorization.RevertToSelf();
                try
                {
                    if (pipe.IsConnected)
                    {
                        pipe.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                }

                pipe.Dispose();
            }
        }

        private void StopListening()
        {
            if (!running)
            {
                return;
            }

            running = false;
            for (var attempt = 0; attempt < 20 && !stopped.IsSet; attempt++)
            {
                try
                {
                    using var client = new NamedPipeClientStream(PipeName);
                    client.Connect(50);
                }
                catch (Exception)
                {
                    // The listener may not be inside WaitForConnection yet.
                }

                if (stopped.Wait(50))
                {
                    return;
                }
            }
        }

        private static bool IsPipeBusy(IOException exception) => (exception.HResult & 0xFFFF) == 231;
    }

    file sealed class CallerImpersonatingReadStream : Stream
    {
        private readonly Stream inner;

        private readonly NamedPipeServerStream pipe;

        public CallerImpersonatingReadStream(Stream inner, NamedPipeServerStream pipe)
        {
            this.inner = inner;
            this.pipe = pipe;
        }

        public override bool CanRead => inner.CanRead;

        public override bool CanSeek => inner.CanSeek;

        public override bool CanWrite => inner.CanWrite;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = inner.Read(buffer, offset, count);
            ImpersonateClient(read);
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var read = inner.Read(buffer);
            ImpersonateClient(read);
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);

        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);

        public override void Flush() => inner.Flush();

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => inner.SetLength(value);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        private void ImpersonateClient(int bytesRead)
        {
            if (bytesRead <= 0)
            {
                return;
            }

            // A pool thread can still be impersonating the previous client. Drop that token
            // and bind this read to the client on this pipe.
            VssCallerAuthorization.RevertToSelf();
            if (!ImpersonateNamedPipeClient(pipe.SafePipeHandle))
            {
                throw new IOException("The named-pipe caller could not be impersonated.");
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ImpersonateNamedPipeClient(SafePipeHandle pipe);
    }
}
