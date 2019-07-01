// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c.Devices;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    public abstract class Ssd13xx : IDisposable
    {
        // Multiply of screen resolution plus single command byte.
        private const int DefaultBufferSize = 48 * 96 + 1;
        private byte[] _genericBuffer;
        protected I2cDevice _i2cDevice;

        public Ssd13xx(I2cDevice i2cDevice, int bufferSize = DefaultBufferSize)
        {
            _genericBuffer = new byte[bufferSize];
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Verifies value is within a specific range.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="start">Starting value of range.</param>
        /// <param name="end">Ending value of range.</param>
        /// <returns>Determines if value is within range.</returns>
        internal static bool InRange(uint value, uint start, uint end)
        {
            return (value - start) <= (end - start);
        }

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        public abstract void SendCommand(ISharedCommand command);

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public virtual void SendData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Span<byte> writeBuffer = SliceGenericBuffer(data.Length + 1);

            writeBuffer[0] = 0x40; // Control byte.
            data.CopyTo(writeBuffer.Slice(1));
            _i2cDevice.Write(writeBuffer);
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        protected Span<byte> SliceGenericBuffer(int length)
        {
            return SliceGenericBuffer(0, length);
        }

        protected Span<byte> SliceGenericBuffer(int start, int length)
        {
            if (_genericBuffer.Length < length)
            {
                var newBuffer = new byte[_genericBuffer.Length * 2];
                _genericBuffer.CopyTo(newBuffer, 0);
                _genericBuffer = newBuffer;
            }
            return _genericBuffer.AsSpan(start, length);
        }
    }
}
