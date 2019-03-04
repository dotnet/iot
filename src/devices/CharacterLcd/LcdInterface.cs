// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Abstraction layer for accessing the lcd IC.
    /// </summary>
    public abstract partial class LcdInterface : IDisposable
    {
        private bool _disposed;

        public abstract void SendData(byte value);
        public abstract void SendCommand(byte command);
        public abstract void SendData(ReadOnlySpan<byte> values);
        public abstract void SendCommands(ReadOnlySpan<byte> values);

        public abstract bool EightBitMode { get; }

        /// <summary>
        /// Wait for the device to not be busy.
        /// </summary>
        /// <param name="microseconds">Time to wait if checking busy state isn't possible/practical.</param>
        public virtual void WaitForNotBusy(int microseconds)
        {
            DelayHelper.DelayMicroseconds(microseconds, allowThreadYield: true);

            // While we could check for the busy state it isn't currently practical. Most
            // commands need a maximum of 37μs to complete. Reading the busy flag alone takes
            // ~200μs (on the Pi) if going through the software driver. Prepping the pins and
            // reading once can take nearly a millisecond, which clearly is not going to be
            // performant.

            // Leaving the flag here to make sure calling code is describing when it cannot
            // actually check the busy state should we be able to get performance
            // to the point where checking would be a net benefit.

            // Note that on a Raspberry Pi 3B+ we average about 1.5μs when delaying for one μs.
        }

        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        public abstract bool BacklightOn { get; set; }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                _disposed = true;
            }
        }

        /// <summary>
        /// Creates a GPIO based interface for the LCD.
        /// </summary>
        /// <param name="registerSelectPin">The pin that controls the regsiter select.</param>
        /// <param name="enablePin">The pin that controls the enable switch.</param>
        /// <param name="dataPins">Collection of pins holding the data that will be printed on the screen.</param>
        /// <param name="backlightPin">The optional pin that controls the backlight of the display.</param>
        /// <param name="backlightBrightness">The brightness of the backlight. 0.0 for off, 1.0 for on.</param>
        /// <param name="readWritePin">The optional pin that controls the read and write switch.</param>
        /// <param name="controller">The controller to use with the LCD. If not specified, uses the platform default.</param>
        public static LcdInterface CreateGpio(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, float backlightBrightness = 1.0f, int readWritePin = -1, IGpioController controller = null)
        {
            return new Gpio(registerSelectPin, enablePin, dataPins, backlightPin, backlightBrightness, readWritePin, controller);
        }

        /// <summary>
        /// Create an integrated I2c based interface for the LCD.
        /// </summary>
        /// <remarks>
        /// This is for on-chip I2c support. For connecting via I2c GPIO expanders, use the GPIO interface <see cref="CreateGpio(int, int, int[], int, float, int, IGpioController)"/>.
        /// </remarks>
        /// <param name="device">The I2c device for the LCD.</param>
        public static LcdInterface CreateI2c(I2cDevice device)
        {
            return new I2c(device);
        }
    }
}
