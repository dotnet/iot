using System;
using System.Net.Sockets;

namespace Iot.Device.Pigpio
{
    internal class TcpConnection
    {
        #region # event

        public event EventHandler StreamChanged;

        #endregion


        #region # private field

        private TcpClient tcp = null;
        private string ipOrHost;
        private int port;

        #endregion


        #region # public property

        public bool IsOpened
        {
            get
            {
                return tcp != null;
            }
        }

        private NetworkStream _stream = null;
        public NetworkStream Stream
        {
            get
            {
                return _stream;
            }
            set
            {
                _stream = value;
                if (StreamChanged != null)
                {
                    StreamChanged.Invoke(this, new EventArgs());
                }
            }
        }

        #endregion


        #region # Implementation of IDisposable

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Release managed objects
                Close();
            }

            // Release unmanaged objects

            disposed = true;
        }
        ~TcpConnection()
        {
            Dispose(false);
        }

        #endregion


        #region # public method

        public bool Open(string ipOrHost, int port)
        {
            if (tcp == null)
            {
                try
                {
                    this.ipOrHost = ipOrHost;
                    this.port = port;

                    tcp = new TcpClient();
                    tcp.BeginConnect(ipOrHost, port, new AsyncCallback(NetConnectCallback), null);

                    Console.WriteLine("Connecting to {0}:{1}...", ipOrHost, port);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection failed({0}).", ex.Message);
                    Close();
                    return false;
                }
            }
            return true;
        }

        public void Close()
        {
            if (Stream != null)
            {
                // Execute handlers of StreamChanged event, and call Dispose()
                var stream = Stream;
                Stream = null;
                stream.Dispose();
            }
            if (tcp != null)
            {
                tcp.Close();
                tcp = null;

                Console.WriteLine("{0}:{1} was disconnected.", ipOrHost, port);
            }
            ipOrHost = string.Empty;
            port = 0;
        }

        #endregion


        #region # private method

        private void NetConnectCallback(IAsyncResult result)
        {
            if (tcp == null)
                return;

            if (tcp.Connected)
            {
                Console.WriteLine("Connected to LAN {0}:{1}.",
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address,
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port);

                var stream = tcp.GetStream();
                stream.ReadTimeout = 10000;
                stream.WriteTimeout = 10000;
                Stream = stream;
            }
        }

        #endregion
    }
}
