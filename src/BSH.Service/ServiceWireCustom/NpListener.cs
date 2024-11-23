using System.IO.Pipes;
using Serilog;

namespace ServiceWire.NamedPipes
{
    public class NpListener
    {
        private bool running;
        private readonly EventWaitHandle terminateHandle = new(false, EventResetMode.AutoReset);
        private readonly int _maxConnections;
        private readonly INamedPipeServerStreamFactory _streamFactory;

        public string PipeName
        {
            get; set;
        }
        public event EventHandler<PipeClientConnectionEventArgs> RequestReieved;

        public NpListener(string pipeName, int maxConnections = 254, INamedPipeServerStreamFactory streamFactory = null)
        {
            if (maxConnections > 254) maxConnections = 254;
            _maxConnections = maxConnections;
            this.PipeName = pipeName;
            _streamFactory = streamFactory ?? new DefaultNamedPipeServerStreamFactory();
        }

        public void Start()
        {
            running = true;
            Task.Factory.StartNew(() => ServerLoop(), TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            if (running)
            {
                running = false;
                //make fake connection to terminate the waiting stream
                try
                {
                    using var client = new NamedPipeClientStream(PipeName);
                    client.Connect(50);
                }
                catch (Exception e)
                {
                    Log.Error("Stop error: {0}", e.ToString().Flatten());
                }
                terminateHandle.WaitOne();
            }
        }

        private void ServerLoop()
        {
            try
            {
                while (running)
                {
                    ProcessNextClient();
                }
            }
            catch (Exception e)
            {
                Log.Fatal("ServerLoop fatal error: {0}", e.ToString().Flatten());
            }
            finally
            {
                terminateHandle.Set();
            }
        }

        private void ProcessClientThread(NamedPipeServerStream pipeStream)
        {
            try
            {
                if (this.RequestReieved != null) //has event subscribers
                {
                    var args = new PipeClientConnectionEventArgs(pipeStream);
                    RequestReieved(this, args);
                }
            }
            catch (Exception e)
            {
                Log.Error("ProcessClientThread error: {0}", e.ToString().Flatten());
            }
            finally
            {
                if (pipeStream.IsConnected) pipeStream.Close();
                pipeStream.Dispose();
            }
        }

        public void ProcessNextClient()
        {
            try
            {
                var pipeStream = _streamFactory.Create(PipeName, PipeDirection.InOut, _maxConnections, PipeTransmissionMode.Byte, PipeOptions.None, 512, 512);
                pipeStream.WaitForConnection();
                Task.Factory.StartNew(() => ProcessClientThread(pipeStream));
            }
            catch (Exception e)
            {
                //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                Log.Error("ProcessNextClient error: {0}", e.ToString().Flatten());
            }
        }
    }

    // Defines the data protocol for reading and writing strings on our stream

    // Contains the method executed in the context of the impersonated user
}