using System;

namespace Iot.Device.HuskyLens
{
    /// <summary>
    /// Represents the connection object used by the <see cref="HuskyLens"/> to communicate with the device
    /// </summary>
    public interface IBinaryConnection
    {
        /// <summary>
        /// Writes the buffer to the connected device
        /// </summary>
        /// <param name="buffer">data to write</param>
        void Write(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Reads any bytes that are available from the device
        /// </summary>
        /// <param name="count">number of bytes to read</param>
        /// <returns>whatever was read, obviously. Duh!</returns>
        ReadOnlySpan<byte> Read(int count);
    }
}
