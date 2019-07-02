// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.I2c;

namespace Iot.Device.Bmx280
{
    public class Bmp280 : BmxBase
    {
        private const byte DeviceId = 0x58;
        public const byte DefaultI2cAddress = 0x77;

        public Bmp280(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
            _deviceId = DeviceId;
            _communicationProtocol = CommunicationProtocol.I2c;
        }

        internal override CalibrationData ReadCalibrationData()
        {
            return new CalibrationData
            {
                // Read temperature calibration data
                DigT1 = Read16BitsFromRegister((byte)Register.DIG_T1),
                DigT2 = (short)Read16BitsFromRegister((byte)Register.DIG_T2),
                DigT3 = (short)Read16BitsFromRegister((byte)Register.DIG_T3),

                // Read pressure calibration data
                DigP1 = Read16BitsFromRegister((byte)Register.DIG_P1),
                DigP2 = (short)Read16BitsFromRegister((byte)Register.DIG_P2),
                DigP3 = (short)Read16BitsFromRegister((byte)Register.DIG_P3),
                DigP4 = (short)Read16BitsFromRegister((byte)Register.DIG_P4),
                DigP5 = (short)Read16BitsFromRegister((byte)Register.DIG_P5),
                DigP6 = (short)Read16BitsFromRegister((byte)Register.DIG_P6),
                DigP7 = (short)Read16BitsFromRegister((byte)Register.DIG_P7),
                DigP8 = (short)Read16BitsFromRegister((byte)Register.DIG_P8),
                DigP9 = (short)Read16BitsFromRegister((byte)Register.DIG_P9)
            };
        }
    }
}
