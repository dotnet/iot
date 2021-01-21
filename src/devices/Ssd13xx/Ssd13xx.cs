// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Represents base class for SSD13xx OLED displays
    /// </summary>
    public abstract class Ssd13xx : IDisposable
    {
        // Multiply of screen resolution plus single command byte.
        private const int DefaultBufferSize = 48 * 96 + 1;
        private byte[] _genericBuffer;

        /// <summary>
        /// Underlying I2C device
        /// </summary>
        protected I2cDevice _i2cDevice;

        /// <summary>
        /// Constructs instance of Ssd13xx
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="bufferSize">Command buffer size</param>
        public Ssd13xx(I2cDevice i2cDevice, int bufferSize = DefaultBufferSize)
        {
            _genericBuffer = new byte[bufferSize];
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
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
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Span<byte> writeBuffer = SliceGenericBuffer(data.Length + 1);

            writeBuffer[0] = 0x40; // Control byte.
            data.CopyTo(writeBuffer.Slice(1));
            _i2cDevice.Write(writeBuffer);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        /// <summary>
        /// Acquires span of specific length pointing to the command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
        protected Span<byte> SliceGenericBuffer(int length) => SliceGenericBuffer(0, length);

        /// <summary>
        /// Acquires span of specific length at specific position in command buffer.
        /// If length of the command buffer is too small it will be reallocated.
        /// </summary>
        /// <param name="start">Start index of the requested span</param>
        /// <param name="length">Requested length</param>
        /// <returns>Span of bytes pointing to the command buffer</returns>
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
