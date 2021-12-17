using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot.Device.Arduino
{
    internal class ReconnectingNetworkStream : Stream
    {
        private readonly Func<Stream> _reconnect;
        private Stream? _streamImplementation;
        private object _lock = new object();

        public ReconnectingNetworkStream(Stream? underlyingStream, Func<Stream> reconnect)
        {
            _streamImplementation = underlyingStream;
            _reconnect = reconnect;
            Connect();
        }

        public ReconnectingNetworkStream(Func<Stream> connect)
        : this(null, connect)
        {
        }

        public override bool CanRead
        {
            get
            {
                return SafeExecute(x => x.CanRead, false);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return SafeExecute(x => x.CanSeek, false);
            }
        }

        public override bool CanWrite
        {
            get
            {
                return SafeExecute(x => x.CanWrite, false);
            }
        }

        public override long Length
        {
            get
            {
                return SafeExecute(x => x.Length, false);
            }
        }

        public override long Position
        {
            get
            {
                return SafeExecute(x => x.Position, false);
            }
            set
            {
                SafeExecute(x => x.Position = value, false);
            }
        }

        public override void Flush()
        {
            SafeExecute(x => x.Flush(), true);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return SafeExecute(x => x.Read(buffer, offset, count), true);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return SafeExecute(x => x.Seek(offset, origin), true);
        }

        public override void SetLength(long value)
        {
            SafeExecute(x => x.SetLength(value), false);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            SafeExecute(x => x.Write(buffer, offset, count), false);
        }

        private void Connect()
        {
            if (_streamImplementation == null)
            {
                lock (_lock)
                {
                    try
                    {
                        _streamImplementation = _reconnect();
                    }
                    catch (IOException)
                    {
                        // Ignore
                    }
                }
            }
        }

        private T SafeExecute<T>(Func<Stream, T> operation, bool doThrow)
            where T : struct
        {
            try
            {
                Connect();

                if (_streamImplementation == null)
                {
                    if (doThrow)
                    {
                        throw new TimeoutException("Stream is disconnected. Retrying next time.");
                    }

                    return default(T);
                }

                return operation(_streamImplementation);
            }
            catch (IOException x)
            {
                _streamImplementation?.Dispose();
                _streamImplementation = null;
                if (doThrow)
                {
                    throw new TimeoutException("Error executing operation. Retrying next time", x);
                }

                return default(T);
            }
        }

        private void SafeExecute(Action<Stream> operation, bool doThrow)
        {
            try
            {
                Connect();

                if (_streamImplementation == null)
                {
                    if (doThrow)
                    {
                        throw new TimeoutException("Stream is disconnected. Retrying next time.");
                    }

                    return;
                }

                operation(_streamImplementation);
            }
            catch (IOException x)
            {
                _streamImplementation?.Dispose();
                _streamImplementation = null;
                if (doThrow)
                {
                    throw new TimeoutException("Error executing operation. Retrying next time", x);
                }

                return;
            }
        }

        public override void Close()
        {
            if (_streamImplementation != null)
            {
                _streamImplementation.Close();
                _streamImplementation = null;
            }

            base.Close();
        }
    }
}
