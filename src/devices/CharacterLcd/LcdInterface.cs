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

        /// <summary>
        /// Sends byte to LCD device
        /// </summary>
        /// <param name="value">Byte value to be sed</param>
        public abstract void SendData(byte value);

        /// <summary>
        /// Sends command to the LCD device
        /// </summary>
        /// <param name="command">Byte representing the command</param>
        public abstract void SendCommand(byte command);

        /// <summary>
        /// Sends data to the LCD device
        /// </summary>
        /// <param name="values">Bytes to be send to the device</param>
        public abstract void SendData(ReadOnlySpan<byte> values);

        /// <summary>
        /// Send commands to the LCD device
        /// </summary>
        /// <param name="values">Each byte represents command to be send</param>
        public abstract void SendCommands(ReadOnlySpan<byte> values);

        /// <summary>
        /// True if device uses 8-bits for communication, false if device uses 4-bits
        /// </summary>
        public abstract bool EightBitMode { get; }

        /// <summary>
        /// The command wait time multiplier for the LCD.
        /// </summary>
        /// <remarks>
        /// In order to handle controllers that might be running at a much slower clock
        /// we're exposing a multiplier for any "hard coded" waits. This can also be
        /// used to reduce the wait time when the clock runs faster or other overhead
        /// (time spent in other code) allows for more aggressive timing.
        ///
        /// There is a busy signal that can be checked that could make this moot, but
        /// currently we are unable to check the signal fast enough to make gains (or
        /// even equal) going off hard timings. The busy signal also requires having a
        /// r/w pin attached.
        /// </remarks>
        public double WaitMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Wait for the device to not be busy.
        /// </summary>
        /// <param name="microseconds">Time to wait if checking busy state isn't possible/practical.</param>
        public virtual void WaitForNotBusy(int microseconds)
        {
            DelayHelper.DelayMicroseconds((int)(microseconds * WaitMultiplier), allowThreadYield: true);

            // While we could check for the busy state it isn't currently practical. Most
            // commands need a maximum of 37μs to complete. Reading the busy flag alone takes
            // ~200μs (on the Pi) if going through the software driver. Prepping the pins and
            // reading once can take nearly a millisecond, which clearly is not going to be
            // performant.
            //
            // We might be able to dynamically introduce waits on the busy flag by measuring
            // the time to take a reading and utilizing the flag if we can check fast enough
            // relative to the requested wait time. If it takes 20μs to check and the wait time
            // is over 1000μs we may very well save significant time as the "slow" commands
            // (home and clear) can finish much faster than the time we've allocated.

            // Timings in the original HD44780 specification are based on a "typical" 270kHz
            // clock. (See page 25.) Most instructions take 3 clocks to complete. The internal
            // clock (fOSC) is documented as varying from 190-350KHz on the HD44780U and 140-450KHz
            // on the PCF2119x.
        }

        /// <summary>
        /// Enable/disable the backlight. (Will always return false if no backlight pin was provided.)
        /// </summary>
        public abstract bool BacklightOn { get; set; }

        /// <summary>
        /// Releases unmanaged resources used by LcdInterface
        /// and optionally release managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc/>
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
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public static LcdInterface CreateGpio(int registerSelectPin, int enablePin, int[] dataPins, int backlightPin = -1, float backlightBrightness = 1.0f, int readWritePin = -1, GpioController controller = null, bool shouldDispose = true)
        {
            return new Gpio(registerSelectPin, enablePin, dataPins, backlightPin, backlightBrightness, readWritePin, controller, shouldDispose);
        }

        /// <summary>
        /// Create an integrated I2c based interface for the LCD.
        /// </summary>
        /// <remarks>
        /// This is for on-chip I2c support. For connecting via I2c GPIO expanders, use the GPIO interface <see cref="CreateGpio(int, int, int[], int, float, int, GpioController, bool)"/>.
        /// </remarks>
        /// <param name="device">The I2c device for the LCD.</param>
        /// <param name="uses8Bit">True if the device uses 8 Bit commands, false if it handles only 4 bit commands.</param>
        public static LcdInterface CreateI2c(I2cDevice device, bool uses8Bit = true)
        {
            if (uses8Bit)
            {
                return new I2c(device);
            }
            else
            {
                return new I2c4Bit(device);
            }
        }
    }
}
