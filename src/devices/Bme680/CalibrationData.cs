// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bme680
{
    /// <summary>
    /// Calibration data for the <see cref="Bme680"/>.
    /// </summary>
    public class CalibrationData
    {
        public ushort DigT1 { get; private set; }

        public short DigT2 { get; private set; }

        public byte DigT3 { get; private set; }



        public ushort DigH1 { get; private set; }

        public ushort DigH2 { get; private set; }

        public sbyte DigH3 { get; private set; }

        public sbyte DigH4 { get; private set; }

        public sbyte DigH5 { get; private set; }

        public byte DigH6 { get; private set; }

        public sbyte DigH7 { get; private set; }



        public ushort DigP1 { get; private set; }

        public short DigP2 { get; private set; }

        public byte DigP3 { get; private set; }

        public short DigP4 { get; private set; }

        public short DigP5 { get; private set; }

        public byte DigP6 { get; private set; }

        public byte DigP7 { get; private set; }

        public short DigP8 { get; private set; }

        public short DigP9 { get; private set; }

        public byte DigP10 { get; private set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bme680">The <see cref="Bme680"/> to read coefficient data from.</param>
        internal void ReadFromDevice(Bme680 bme680)
        {
            // Temperature.
            DigT1 = (ushort)bme680.Read16Bits(Register.DIG_T1);
            DigT2 = bme680.Read16Bits(Register.DIG_T2);
            DigT3 = bme680.Read8Bits(Register.DIG_T3);

            // Humidity.
            DigH1 = (ushort)((bme680.Read8Bits(Register.DIG_H1_MSB) << 4) | (bme680.Read8Bits(Register.DIG_H1_LSB) & 0b_0000_1111));
            DigH2 = (ushort)((bme680.Read8Bits(Register.DIG_H2_MSB) << 4) | (bme680.Read8Bits(Register.DIG_H2_LSB) >> 4));
            DigH3 = (sbyte)bme680.Read8Bits(Register.DIG_H3);
            DigH4 = (sbyte)bme680.Read8Bits(Register.DIG_H4);
            DigH5 = (sbyte)bme680.Read8Bits(Register.DIG_H5);
            DigH6 = bme680.Read8Bits(Register.DIG_H6);
            DigH7 = (sbyte)(bme680.Read8Bits(Register.DIG_H7));

            // Pressure.
            DigP1 = (ushort)bme680.Read16Bits(Register.DIG_P1_LSB);
            DigP2 = bme680.Read16Bits(Register.DIG_P2_LSB);
            DigP3 = bme680.Read8Bits(Register.DIG_P3);
            DigP4 = bme680.Read16Bits(Register.DIG_P4_LSB);
            DigP5 = bme680.Read16Bits(Register.DIG_P5_LSB);
            DigP6 = bme680.Read8Bits(Register.DIG_P6);
            DigP7 = bme680.Read8Bits(Register.DIG_P7);
            DigP8 = bme680.Read16Bits(Register.DIG_P8_LSB);
            DigP9 = bme680.Read16Bits(Register.DIG_P9_LSB);
            DigP10 = bme680.Read8Bits(Register.DIG_P10);
        }
    }
}
