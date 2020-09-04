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
        public ReadOnlySpan<byte> Read()
        {
            byte[] buffer = new byte[1024];
            int count = _serialPort.Read(buffer, 0, buffer.Length);
            return new ReadOnlySpan<byte>(buffer, 0, count);
        }
    }

    /// <summary>
    /// todo
    /// </summary>
    public interface IBinaryConnection
    {
        /// <summary>
        /// Writes the buffer to the connected device
        /// </summary>
        /// <param name="buffer">todo</param>
        void Write(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Reads any bytes that are available from the device
        /// </summary>
        /// <returns>whatever was read, obviously. Duh!</returns>
        ReadOnlySpan<byte> Read();
    }
}