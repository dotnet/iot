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
        /// <summary>
        /// Gets a temperature coefficient from <see cref="Register.DIG_T1"/>.
        /// </summary>
        public ushort TCal1 { get; private set; }

        /// <summary>
        /// Gets a temperature coefficient from <see cref="Register.DIG_T2"/>.
        /// </summary>
        public short TCal2 { get; private set; }

        /// <summary>
        /// Gets a temperature coefficient from <see cref="Register.DIG_T3"/>
        /// </summary>
        public byte TCal3 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H1_MSB"/> and <see cref="Register.DIG_H1_LSB"/>.
        /// </summary>
        public ushort HCal1 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H2_MSB"/> and <see cref="Register.DIG_H2_LSB"/>.
        /// </summary>
        public ushort HCal2 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H3"/>.
        /// </summary>
        public sbyte HCal3 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H4"/>.
        /// </summary>
        public sbyte HCal4 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H5"/>.
        /// </summary>
        public sbyte HCal5 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H6"/>.
        /// </summary>
        public byte HCal6 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.DIG_H7"/>.
        /// </summary>
        public sbyte HCal7 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P1_LSB"/>.
        /// </summary>
        public ushort PCal1 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P2_LSB"/>.
        /// </summary>
        public short PCal2 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P3"/>.
        /// </summary>
        public byte PCal3 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P4_LSB"/>.
        /// </summary>
        public short PCal4 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P5_LSB"/>.
        /// </summary>
        public short PCal5 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P6"/>.
        /// </summary>
        public byte PCal6 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P7"/>.
        /// </summary>
        public byte PCal7 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_8"/>.
        /// </summary>
        public short PCal8 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_9"/>.
        /// </summary>
        public short PCal9 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.DIG_P10"/>.
        /// </summary>
        public byte PCal10 { get; private set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bme680">The <see cref="Bme680"/> to read coefficient data from.</param>
        internal void ReadFromDevice(Bme680 bme680)
        {
            // Temperature.
            TCal1 = (ushort)bme680.Read16Bits(Register.DIG_T1);
            TCal2 = bme680.Read16Bits(Register.DIG_T2);
            TCal3 = bme680.Read8Bits(Register.DIG_T3);

            // Humidity.
            HCal1 = (ushort)((bme680.Read8Bits(Register.DIG_H1_MSB) << 4) | (bme680.Read8Bits(Register.DIG_H1_LSB) & 0b_0000_1111));
            HCal2 = (ushort)((bme680.Read8Bits(Register.DIG_H2_MSB) << 4) | (bme680.Read8Bits(Register.DIG_H2_LSB) >> 4));
            HCal3 = (sbyte)bme680.Read8Bits(Register.DIG_H3);
            HCal4 = (sbyte)bme680.Read8Bits(Register.DIG_H4);
            HCal5 = (sbyte)bme680.Read8Bits(Register.DIG_H5);
            HCal6 = bme680.Read8Bits(Register.DIG_H6);
            HCal7 = (sbyte)(bme680.Read8Bits(Register.DIG_H7));

            // Pressure.
            PCal1 = (ushort)bme680.Read16Bits(Register.DIG_P1_LSB);
            PCal2 = bme680.Read16Bits(Register.DIG_P2_LSB);
            PCal3 = bme680.Read8Bits(Register.DIG_P3);
            PCal4 = bme680.Read16Bits(Register.DIG_P4_LSB);
            PCal5 = bme680.Read16Bits(Register.DIG_P5_LSB);
            PCal6 = bme680.Read8Bits(Register.DIG_P6);
            PCal7 = bme680.Read8Bits(Register.DIG_P7);
            PCal8 = bme680.Read16Bits(Register.DIG_P8_LSB);
            PCal9 = bme680.Read16Bits(Register.DIG_P9_LSB);
            PCal10 = bme680.Read8Bits(Register.DIG_P10);
        }
    }
}
