// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Ssd1306.Command;
using System;
using System.Device.I2c;

namespace Iot.Device.Ssd1306
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system. 
    /// </summary>
    public class Ssd1306 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes new instance of Ssd1306 device that will communicate using I2C bus.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system. 
        /// </summary>
        /// <param name="i2cDevice">>The I2C device used for communication.</param>
        public Ssd1306(I2cDevice i2cDevice)
        {
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
        public void SendCommand(ICommand command)
        {
            const int stackThreshold = 32;
            byte[] commandBytes = command.GetBytes();

            if (commandBytes == null || commandBytes.Length == 0)
            {
                return;
            }

            Span<byte> writeBuffer = commandBytes.Length < stackThreshold ?
               stackalloc byte[commandBytes.Length + 1] :
               new byte[commandBytes.Length + 1];

            commandBytes.CopyTo(writeBuffer.Slice(1));

            // Be aware there is a Continuation Bit in the Control byte and can be used
            // to state (logic LOW) if there is only data bytes to follow.
            // This binding separates commands and data by using SendCommand and SendData.

            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Send data to the display controller.
        /// </summary>
        /// <param name="data">The data to send to the display controller.</param>
        public void SendData(byte[] data)
        {
            const int stackThreshold = 512;

            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Span<byte> writeBuffer = data.Length < stackThreshold ?
               stackalloc byte[data.Length + 1] :
               new byte[data.Length + 1];

            writeBuffer[0] = 0x40; // Control byte.
            data.CopyTo(writeBuffer.Slice(1));
            _i2cDevice.Write(writeBuffer);
        }

        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
