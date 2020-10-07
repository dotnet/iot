//// Licensed to the .NET Foundation under one or more agreements.
//// The .NET Foundation licenses this file to you under the MIT license.
//// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// SparkFun Qwiic Button is an I2C based button with a built-in LED.
    /// Supported hardware version: 1.0.0
    /// </summary>
    public partial class QwiicButton : IDisposable
    {
        private const int DefaultAddress = 0x6F; // Default I2C address of the button
        private I2cBusAccess _i2cBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="QwiicButton"/> class.
        /// </summary>
        /// <param name="i2cBusId">I2C bus ID the button is connected to.</param>
        /// <param name="i2cAddress">I2C bus address of the button (default=0x6F).</param>
        public QwiicButton(int i2cBusId, byte i2cAddress = DefaultAddress)
        {
            I2cBusId = i2cBusId;
            I2cAddress = i2cAddress;
            var settings = new I2cConnectionSettings(i2cBusId, i2cAddress);
            var device = I2cDevice.Create(settings);
            _i2cBus = new I2cBusAccess(device);
        }

        /// <summary>
        /// I2C bus ID the button is connected to.
        /// </summary>
        public int I2cBusId { get; set; }

        /// <summary>
        /// I2C bus address of the button.
        /// </summary>
        public byte I2cAddress { get; set; }

        /// <summary>
        /// Returns the 8-bit device ID of the button.
        /// </summary>
        public byte GetDeviceId()
        {
            return _i2cBus.ReadSingleRegister(Register.Id);
        }

        /// <summary>
        /// Returns the firmware version of the button as a 16-bit integer.
        /// The leftmost (high) byte is the major revision number, and the rightmost (low) byte is
        /// the minor revision number.
        /// </summary>
        public ushort GetFirmwareVersionAsInteger()
        {
            ushort version = (ushort)(_i2cBus.ReadSingleRegister(Register.FirmwareMajor) << 8);
            version |= _i2cBus.ReadSingleRegister(Register.FirmwareMinor);
            return version;
        }

        /// <summary>
        /// Returns the firmware version of the button as a [major revision].[minor revision] string.
        /// </summary>
        public string GetFirmwareVersionAsString()
        {
            var major = _i2cBus.ReadSingleRegister(Register.FirmwareMajor);
            var minor = _i2cBus.ReadSingleRegister(Register.FirmwareMinor);
            return major + "." + minor;
        }

        /// <summary>
        /// Configures the button to attach to the I2C bus using the specified address.
        /// </summary>
        public void SetI2cAddress(byte address)
        {
            if (address < 0x08 || address > 0x77)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "I2C input address must be between 0x08 and 0x77");
            }

            _i2cBus.WriteSingleRegister(Register.I2cAddress, address);
            I2cAddress = address;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _i2cBus?.Dispose();
            _i2cBus = null;
        }
    }
}
