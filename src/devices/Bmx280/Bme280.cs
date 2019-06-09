// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmx280
{
    public class Bme280 : BmxBase
    {
        private const byte DeviceId = 0x60;
        public const byte DefaultI2cAddress = 0x76;

        public Bme280(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _deviceId = DeviceId;
            _calibrationData = new CalibrationData();
            _communicationProtocol = CommunicationProtocol.I2c;
        }
    }
}
