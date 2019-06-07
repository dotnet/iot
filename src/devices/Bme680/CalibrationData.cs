// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    internal class CalibrationData
    {
        // humidity calibration registers
        public ushort ParH1 { get; private set; }
        public ushort ParH2 { get; private set; }
        public sbyte ParH3 { get; private set; }
        public sbyte ParH4 { get; private set; }
        public sbyte ParH5 { get; private set; }
        public byte ParH6 { get; private set; }
        public sbyte ParH7 { get; private set; }

        // gas calibration registers
        public sbyte ParGh1 { get; private set; }
        public short ParGh2 { get; private set; }
        public sbyte ParGh3 { get; private set; }

        // temperature calibration registers
        public ushort ParT1 { get; private set; }
        public short ParT2 { get; private set; }
        public sbyte ParT3 { get; private set; }

        // pressure calibration registers
        public ushort ParP1 { get; private set; }
        public short ParP2 { get; private set; }
        public sbyte ParP3 { get; private set; }
        public short ParP4 { get; private set; }
        public short ParP5 { get; private set; }
        public sbyte ParP6 { get; private set; }
        public sbyte ParP7 { get; private set; }
        public short ParP8 { get; private set; }
        public short ParP9 { get; private set; }
        public byte ParP10 { get; private set; }

        // heater range
        public byte ResHeatRange { get; private set; }
        // heater resistance correction factor
        public sbyte ResHeatVal { get; private set; }
        // range switching error
        public sbyte RangeSwErr { get; private set; }

        internal void ReadFromDevice(Bme680 bme680)
        {
            // load humidity calibration data
            ParH1 = (ushort)((bme680.Read8BitsFromRegister((byte) Register.PAR_H1_MSB) << 4) |
                     (bme680.Read8BitsFromRegister((byte) Register.PAR_H1_LSB) & (byte)Mask.BIT_H1_DATA_MSK));
            ParH2 = (ushort)((bme680.Read8BitsFromRegister((byte)Register.PAR_H2_MSB) << 4) |
                             (bme680.Read8BitsFromRegister((byte)Register.PAR_H2_LSB) >> 4));
            ParH3 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_H3);
            ParH4 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_H4);
            ParH5 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_H5);
            ParH6 = bme680.Read8BitsFromRegister((byte)Register.PAR_H6);
            ParH7 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_H7);

            // load gas calibration data
            ParGh1 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_GH1);
            ParGh2 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_GH2);
            ParGh3 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_GH3);

            // load temperature calibration data
            ParT1 = bme680.Read16BitsFromRegister((byte)Register.PAR_T1);
            ParT2 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_T2);
            ParT3 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_T3);

            // load pressure calibration data
            ParP1 = bme680.Read16BitsFromRegister((byte)Register.PAR_P1);
            ParP2 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_P2);
            ParP3 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_P3);
            ParP4 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_P4);
            ParP5 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_P5);
            ParP6 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_P6);
            ParP7 = (sbyte)bme680.Read8BitsFromRegister((byte)Register.PAR_P7);
            ParP8 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_P8);
            ParP9 = (short)bme680.Read16BitsFromRegister((byte)Register.PAR_P9);
            ParP10 = bme680.Read8BitsFromRegister((byte)Register.PAR_P10);

            // load heater calibration data
            var rangeReg = bme680.Read8BitsFromRegister((byte)Register.RES_HEAT_RANGE);
            ResHeatRange = (byte)((rangeReg & (byte)Mask.RH_RANGE) >> 4);

            var rangeSwReg = bme680.Read8BitsFromRegister((byte)Register.RANGE_SW_ERR);
            RangeSwErr = (sbyte)((rangeSwReg & (byte)Mask.RS_ERROR) >> 4);

            ResHeatVal = (sbyte)bme680.Read8BitsFromRegister((byte)Register.RES_HEAT_VAL);
        }
    }
}
