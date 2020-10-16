// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;
using Iot.Device.QwiicButton.RegisterMapping;

namespace Iot.Device.QwiicButton
{
    /// <summary>
    /// SparkFun Qwiic Button is an I2C based button with a built-in LED.
    /// Supported hardware version: 1.0.0
    /// </summary>
    public sealed partial class QwiicButton : IDisposable
    {
        private const int DefaultAddress = 0x6F; // Default I2C address of the button
        private I2cRegisterAccess<Register> _registerAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="QwiicButton"/> class.
        /// </summary>
        /// <param name="i2cBusId">I2C bus ID the button is connected to.</param>
        /// <param name="i2cAddress">I2C bus address of the button (default=0x6F).</param>
        public QwiicButton(int i2cBusId, byte i2cAddress = DefaultAddress)
        {
            var settings = new I2cConnectionSettings(i2cBusId, i2cAddress);
            var device = I2cDevice.Create(settings);
            _registerAccess = new I2cRegisterAccess<Register>(device, useLittleEndian: true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QwiicButton"/> class.
        /// </summary>
        /// <param name="i2cDevice">Qwiic Button communications channel.</param>
        public QwiicButton(I2cDevice i2cDevice)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _registerAccess = new I2cRegisterAccess<Register>(i2cDevice, useLittleEndian: true);
        }

        /// <summary>
        /// Returns the 8-bit device ID of the button.
        /// </summary>
        public byte GetDeviceId()
        {
            return _registerAccess.ReadRegister<byte>(Register.Id);
        }

        /// <summary>
        /// Returns the firmware version of the button.
        /// </summary>
        public Version GetFirmwareVersion()
        {
            var major = _registerAccess.ReadRegister<byte>(Register.FirmwareMajor);
            var minor = _registerAccess.ReadRegister<byte>(Register.FirmwareMinor);
            return new Version(major, minor);
        }

        /// <summary>
        /// Configures the button to attach to the I2C bus using the specified address.
        /// Since this operation does not update the configuration of the underlying <see cref="I2cDevice"/>,
        /// the <see cref="QwiicButton"/> instance is subsequently misconfigured and thus actively disposed.
        /// </summary>
        public void ChangeI2cAddressAndDispose(byte address)
        {
            if (address < 0x08 || address > 0x77)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "I2C input address must be between 0x08 and 0x77");
            }

            _registerAccess.WriteRegister(Register.I2cAddress, address);
            Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _registerAccess?.Dispose();
            _registerAccess = null;
        }
    }
}
