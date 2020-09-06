using System;
using System.IO.Ports;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// todo
    /// </summary>
    public class SerialPortConnection : IBinaryConnection
    {
        private readonly SerialPort _serialPort;

        /// <summary>
        /// todo
        /// </summary>
        /// <param name="serialPort">the thing</param>
        public SerialPortConnection(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            _serialPort.Write(buffer.ToArray(), 0, buffer.Length);
        }

        /// <inheritdoc/>
        public ReadOnlySpan<byte> Read(int count)
        {
            byte[] buffer = new byte[count];
            int c = 0;
            while (c < count)
            {
                c += _serialPort.Read(buffer, c, count - c);
            }

            return new ReadOnlySpan<byte>(buffer, 0, count);
        }
    }
}
