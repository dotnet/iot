// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bmx280.Register;

namespace Iot.Device.Bmx280
{
    /// <summary>
    /// Calibration data for the <see cref="Bme680"/>.
    /// </summary>
    internal class Bme680CalibrationData : CalibrationData
    {
        public ushort DigH1 { get; set; }
        public ushort DigH2 { get; set; }
        public sbyte DigH3 { get; set; }
        public sbyte DigH4 { get; set; }
        public sbyte DigH5 { get; set; }
        public byte DigH6 { get; set; }
        public sbyte DigH7 { get; set; }

        public byte DigP10 { get; set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bme680">The <see cref="BmxBase"/> to read coefficient data from.</param>
        internal override void ReadFromDevice(BmxBase bmxBase)
        {
            // Read temperature calibration data.
            DigT1 = bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_T1);
            DigT2 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_T2);
            DigT3 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_T3);

            // Read humidity calibration data.
            DigH1 = (ushort)((bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H1_MSB) << 4) | (bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H1_LSB) & 0b0000_1111));
            DigH2 = (ushort)((bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H2_MSB) << 4) | (bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H2_LSB) >> 4));
            DigH3 = (sbyte)bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H3);
            DigH4 = (sbyte)bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H4);
            DigH5 = (sbyte)bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H5);
            DigH6 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H6);
            DigH7 = (sbyte)(bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_H7));

            // Read pressure calibration data.
            DigP1 = bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P1_LSB);
            DigP2 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P2_LSB);
            DigP3 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_P3);
            DigP4 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P4_LSB);
            DigP5 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P5_LSB);
            DigP6 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_P6);
            DigP7 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_P7);
            DigP8 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P8_LSB);
            DigP9 = (short)bmxBase.Read16BitsFromRegister((byte)Bme680Register.DIG_P9_LSB);
            DigP10 = bmxBase.Read8BitsFromRegister((byte)Bme680Register.DIG_P10);
        }
    }
}
