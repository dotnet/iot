// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;

namespace Iot.Device.Hmc5883
{
    public struct RawValues
    {
        public int X;
        public int Y;
        public int Z;
    };

    /// <summary>
    /// 3-Axis Digital Compass IC HMC5883
    /// </summary>
    public class Hmc5883 : IDisposable
    {
        private I2cDevice _i2cDevice;

        /// <summary>
        /// 3-Axis Digital Compass IC HMC5883
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Hmc5883(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Sets the data output rate and measurement configuration for the device. 
        /// </summary>
        /// <param name="rate">Data output rates.</param>
        /// <param name="mode">Measurement configuration.</param>
        public void setOutputRateAndMeasurementMode(OutputRates rate, MeasurementModes mode)
        {
            int config = ((byte)rate | (byte)mode);
            write(Registers.ConfigurationRegisterA, (byte)config);
        }

        /// <summary>
        /// Sets the gain configuration for the device. 
        /// </summary>
        /// <param name="gain">Gain configuration.</param>
        public void setGain(GainConfiguration gain)
        {
            write(Registers.ConfigurationRegisterB, (byte)gain);
        }

        /// <summary>
        /// Sets the operating mode of the device. 
        /// </summary>
        /// <param name="mode">Operating mode.</param>
        public void setOperatingMode(OperatingModes mode)
        {
            write(Registers.ModeRegister, (byte)mode);
        }

        /// <summary>
        /// Reads values from the data output registers of device. 
        /// </summary>
        /// <returns>RawValues</returns>
        public RawValues getRawValues()
        {
            byte[] buffer = read(Registers.DataRegisterBegin, 6);
            RawValues raw = new RawValues();
            raw.X = (buffer[0] << 8) | buffer[1];
            raw.Z = (buffer[2] << 8) | buffer[3];
            raw.Y = (buffer[4] << 8) | buffer[5];
            return raw;
        }

        /// <summary>
        /// Writes the byte value to register. 
        /// </summary>
        /// <param name="register">The register to write data.</param>
        /// <param name="data">Data.</param>
        public void write(Registers register, byte data)
        {
            byte[] writeBuffer = new byte[] { (byte)register, data };
            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Reads the data from register.
        /// </summary>
        /// <param name="register">The register to read data.</param>
        /// <param name="length">The length of readed data (bytes).</param>
        /// <returns>byte[]</returns>
        public byte[] read(Registers register, int length)
        {
            _i2cDevice.WriteByte((byte)register);
            byte[] readBuffer = new byte[length];
            _i2cDevice.Read(readBuffer);

            return readBuffer;
        }

        /// <summary>
        /// Reads the byte from register.
        /// </summary>
        /// <param name="register">The register to read data.</param>
        /// <returns>byte</returns>
        public byte readByte(Registers register)
        {
            _i2cDevice.WriteByte((byte)register);
            return  _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Reads statuses of the device.
        /// </summary>
        /// <returns>IList<StatusRegisterValues></returns>
        public IList<StatusRegisterValues> getStatus()
        {
            var result = new List<StatusRegisterValues>();
            byte status = readByte(Registers.StatusRegister);

            byte mask = 0b00000001;
            if ((status & mask) != 0) {
                result.Add(StatusRegisterValues.RDY);
            }

            mask = 0b00000010;
            if ((status & mask) != 0) {
                result.Add(StatusRegisterValues.LOCK);
            }

            mask = 0b00000100;
            if ((status & mask) != 0) {
                result.Add(StatusRegisterValues.Ren);
            }

            return result;
        }
    }
}