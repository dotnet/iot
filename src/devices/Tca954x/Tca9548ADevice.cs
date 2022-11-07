// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Tca954x
{
    internal class Tca9548AI2cDevice : I2cDevice
    {
        private readonly I2cDevice _channelDevice;
        private readonly Tca9548A _tca9548A;
        private readonly MultiplexerChannel _tcaChannel;

        /// <summary>
        /// The connection settings of a device on an I2C bus.
        /// </summary>
        public override I2cConnectionSettings ConnectionSettings => _channelDevice.ConnectionSettings;

        /// <summary>
        ///  Initializes a new instance of the <see cref="Tca9548AI2cDevice"/> class on the <see cref="MultiplexerChannel"/> of TCA mux that will use the specified settings to communicate with the I2C device.
        /// </summary>
        /// <param name="tca9548A">Instance on TCA9548A device</param>
        /// <param name="tcaChannel">Channel on which device is</param>
        /// <param name="device">I2C device (from the parent bus)</param>
        internal Tca9548AI2cDevice(Tca9548A tca9548A, MultiplexerChannel tcaChannel, I2cDevice device)
        {
            _tca9548A = tca9548A;
            _tcaChannel = tcaChannel;
            _channelDevice = device;
        }

        private void SelectDeviceChannel() => _tca9548A.SelectChannel(_tcaChannel);

        /// <summary>
        /// Reads data from the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override void Read(Span<byte> buffer)
        {
            SelectDeviceChannel();
            _channelDevice.Read(buffer);
        }

        /// <summary>
        /// Reads a byte from the I2C device.
        /// </summary>
        /// <returns>A byte read from the I2C device.</returns>
        public override byte ReadByte()
        {
            SelectDeviceChannel();
            return _channelDevice.ReadByte();
        }

        /// <summary>
        /// Writes data to the I2C device.
        /// </summary>
        /// <param name="buffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.
        /// </param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            SelectDeviceChannel();
            _channelDevice.Write(buffer);
        }

        /// <summary>
        /// Writes a byte to the I2C device.
        /// </summary>
        /// <param name="value">The byte to be written to the I2C device.</param>
        public override void WriteByte(byte value)
        {
            SelectDeviceChannel();
            _channelDevice.WriteByte(value);
        }

        /// <summary>
        /// Performs an atomic operation to write data to and then read data from the I2C bus on which the device is connected,
        /// and sends a restart condition between the write and read operations.
        /// </summary>
        /// <param name="writeBuffer">
        /// The buffer that contains the data to be written to the I2C device.
        /// The data should not include the I2C device address.</param>
        /// <param name="readBuffer">
        /// The buffer to read the data from the I2C device.
        /// The length of the buffer determines how much data to read from the I2C device.
        /// </param>
        public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
        {
            SelectDeviceChannel();
            _channelDevice.WriteRead(writeBuffer, readBuffer);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        protected override void Dispose(bool disposing)
        {
            _channelDevice.Dispose();
            base.Dispose(disposing);
        }
    }
}
