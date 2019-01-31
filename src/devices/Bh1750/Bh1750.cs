// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2c.Drivers;
using System.Device.Spi;
using System.Device.Spi.Drivers;
using System.Runtime.InteropServices;

namespace Iot.Device.Bh1750
{
    public class Bh1750 : IDisposable
    {
        public const int DeviceI2cAddress = 0x5C;
        public const byte OneTimeHighResolutionMode = 0b_0010_0000;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a Bh1750 device connected to an I2c bus.
        /// </summary>
        /// <param name="busId">The bus id in which the device is connected to. Defaults to 1.</param>
        public Bh1750(int busId = 1)
        {
            I2cConnectionSettings connectionSettings = new I2cConnectionSettings(busId, DeviceI2cAddress);
            _i2cDevice = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? (I2cDevice)new UnixI2cDevice(connectionSettings) : new Windows10I2cDevice(connectionSettings);
        }

        /// <summary>
        /// Reads the light detected by the sensor using high resolution mode.
        /// </summary>
        /// <returns>The light detected in lux.</returns>
        public double ReadLight()
        {
            Span<byte> readBuffer = stackalloc byte[2];

            _i2cDevice.WriteByte(OneTimeHighResolutionMode);
            _i2cDevice.Read(readBuffer);

            //Convert bytes read to Lux.
            // For more information on the formula, refer to datasheet: http://www.elechouse.com/elechouse/images/product/Digital%20light%20Sensor/bh1750fvi-e.pdf
            return ((readBuffer[1] + (256 * readBuffer[0])) / 1.2);
        }

        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
