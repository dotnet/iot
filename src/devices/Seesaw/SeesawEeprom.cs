// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.I2c;

namespace Iot.Device.Seesaw
{
    public partial class Seesaw : IDisposable
    {
        /// <summary>
        /// Write a byte to the EEProm area on the Seesaw module.
        /// </summary>
        /// <param name="eepromAddress">The point in the EEProm area to write the byte.</param>
        /// <param name="value">The value to write into the EEProm area.</param>
        public void WriteEEPromByte(byte eepromAddress, byte value)
        {
            WriteEEProm(eepromAddress, new byte[] { value });
        }

        /// <summary>
        /// Write a byte array to the EEProm area on the Seesaw module.
        /// </summary>
        /// <param name="eepromAddress">The point in the EEProm area to start writing the data.</param>
        /// <param name="data">The bytes to be written into the EEProm area.</param>
        public void WriteEEProm(byte eepromAddress, byte[] data)
        {
            if (!HasModule(SeesawModule.Eeprom))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw EEPROM functionality");
            }

            Write(SeesawModule.Eeprom, (SeesawFunction)eepromAddress, data);
        }

        /// <summary>
        /// Read a byte from the EEProm area on the Seesaw module.
        /// </summary>
        /// <param name="eepromAddress">The point in the EEProm area to start reading the data.</param>
        /// <returns>The data byte read from the EEProm area.</returns>
        public byte ReadEEPromByte(byte eepromAddress)
        {
            if (!HasModule(SeesawModule.Eeprom))
            {
                throw new InvalidOperationException($"The hardware on I2C Bus {I2cDevice.ConnectionSettings.BusId}, Address 0x{I2cDevice.ConnectionSettings.DeviceAddress:X2} does not support Adafruit SeeSaw EEPROM functionality");
            }

            return ReadByte(SeesawModule.Eeprom, (SeesawFunction)eepromAddress);
        }

        /// <summary>
        /// Change the I2C address that the Seesaw board listens on. Note that this will reset communications
        /// with the host device and dispose the current I2cDevice.
        /// </summary>
        /// <param name="i2cAddress">The new I2C address to be used.</param>
        public void SetI2CAddress(byte i2cAddress)
        {
            I2cConnectionSettings oldSsettings = I2cDevice.ConnectionSettings;

            if (i2cAddress != GetI2CAddress())
            {
                WriteEEPromByte((byte)SeesawFunction.EepromI2cAddr, i2cAddress);

                I2cDevice.Dispose();

                Initialize(I2cDevice.Create(new I2cConnectionSettings(oldSsettings.BusId, i2cAddress)));
            }
        }

        /// <summary>
        /// Read the address configured to be used as the I2C address.
        /// </summary>
        /// <returns>The data byte representing the I2C address.</returns>
        private byte GetI2CAddress() => ReadEEPromByte((byte)SeesawFunction.EepromI2cAddr);
    }
}
