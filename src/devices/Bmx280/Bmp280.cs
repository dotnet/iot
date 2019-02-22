// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmx280
{
    public class Bmp280 : BmxBase
    {
        private const byte Signature = 0x58;
        public const byte Address = 0x77;

        public Bmp280(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _signature = Signature;
            _calibrationData = new CalibrationData();
            _communicationProtocol = CommunicationProtocol.I2c;            
        }                   
    }
}
