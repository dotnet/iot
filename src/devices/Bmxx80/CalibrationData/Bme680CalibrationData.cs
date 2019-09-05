// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bmxx80.Register;

namespace Iot.Device.Bmxx80.CalibrationData
{
    /// <summary>
    /// Calibration data for the <see cref="Bme680"/>.
    /// </summary>
    internal class Bme680CalibrationData : Bmxx80CalibrationData
    {
        public byte DigP10 { get; set; }

        public ushort DigH1 { get; set; }
        public ushort DigH2 { get; set; }
        public sbyte DigH3 { get; set; }
        public sbyte DigH4 { get; set; }
        public sbyte DigH5 { get; set; }
        public byte DigH6 { get; set; }
        public sbyte DigH7 { get; set; }

        public sbyte DigGh1 { get; set; }
        public short DigGh2 { get; set; }
        public sbyte DigGh3 { get; set; }

        public byte ResHeatRange { get; set; }
        public sbyte ResHeatVal { get; set; }
        public sbyte RangeSwErr { get; set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bmxx80Base">The <see cref="Bmxx80Base"/> to read coefficient data from.</param>
        protected internal override void ReadFromDevice(Bmxx80Base bmxx80Base)
        {
            // Read temperature calibration data.
            DigT1 = bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_T1);
            DigT2 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_T2);
            DigT3 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_T3);

            // Read humidity calibration data.
            DigH1 = (ushort)((bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H1_MSB) << 4) | (bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H1_LSB) & (byte)Bme680Mask.BIT_H1_DATA_MSK));
            DigH2 = (ushort)((bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H2_MSB) << 4) | (bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H2_LSB) >> 4));
            DigH3 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H3);
            DigH4 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H4);
            DigH5 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H5);
            DigH6 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H6);
            DigH7 = (sbyte)(bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_H7));

            // Read pressure calibration data.
            DigP1 = bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P1_LSB);
            DigP2 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P2_LSB);
            DigP3 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_P3);
            DigP4 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P4_LSB);
            DigP5 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P5_LSB);
            DigP6 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_P6);
            DigP7 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_P7);
            DigP8 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P8_LSB);
            DigP9 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_P9_LSB);
            DigP10 = bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_P10);

            // read gas calibration data.
            DigGh1 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_GH1);
            DigGh2 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme680Register.DIG_GH2);
            DigGh3 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.DIG_GH3);

            // read heater calibration data
            ResHeatRange = (byte)((bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.RES_HEAT_RANGE) & (byte)Bme680Mask.RH_RANGE) >> 4);
            RangeSwErr = (sbyte)((bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.RANGE_SW_ERR) & (byte)Bme680Mask.RS_ERROR) >> 4);
            ResHeatVal = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme680Register.RES_HEAT_VAL);
        }
    }
}
