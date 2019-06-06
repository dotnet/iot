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
        /// Gets a temperature coefficient from <see cref="Register.temp_cal_1"/>.
        /// </summary>
        public ushort TCal1 { get; private set; }

        /// <summary>
        /// Gets a temperature coefficient from <see cref="Register.temp_cal_2"/>.
        /// </summary>
        public short TCal2 { get; private set; }

        /// <summary>
        /// Gets a temperature coefficient from <see cref="Register.temp_cal_3"/>
        /// </summary>
        public byte TCal3 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_1_msb"/> and <see cref="Register.hum_cal_1_lsb"/>.
        /// </summary>
        public ushort HCal1 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_2_msb"/> and <see cref="Register.hum_cal_2_lsb"/>.
        /// </summary>
        public ushort HCal2 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_3"/>.
        /// </summary>
        public sbyte HCal3 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_4"/>.
        /// </summary>
        public sbyte HCal4 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_5"/>.
        /// </summary>
        public sbyte HCal5 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_6"/>.
        /// </summary>
        public byte HCal6 { get; private set; }

        /// <summary>
        /// Gets a humidity coefficient from <see cref="Register.hum_cal_7"/>.
        /// </summary>
        public sbyte HCal7 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_1_lsb"/>.
        /// </summary>
        public ushort PCal1 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_2_lsb"/>.
        /// </summary>
        public short PCal2 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_3"/>.
        /// </summary>
        public byte PCal3 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_4_lsb"/>.
        /// </summary>
        public short PCal4 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_5_lsb"/>.
        /// </summary>
        public short PCal5 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_6"/>.
        /// </summary>
        public byte PCal6 { get; private set; }

        /// <summary>
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_7"/>.
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
        /// Gets a pressure coefficient from <see cref="Register.pres_cal_10"/>.
        /// </summary>
        public byte PCal10 { get; private set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bme680">The <see cref="Bme680"/> to read coefficient data from.</param>
        internal void ReadFromDevice(Bme680 bme680)
        {
            // Temperature.
            TCal1 = (ushort)bme680.Read16Bits(Register.temp_cal_1);
            TCal2 = bme680.Read16Bits(Register.temp_cal_2);
            TCal3 = bme680.Read8Bits(Register.temp_cal_3);

            // Humidity.
            HCal1 = (ushort)((bme680.Read8Bits(Register.hum_cal_1_msb) << 4) | (bme680.Read8Bits(Register.hum_cal_1_lsb) & 0b_0000_1111));
            HCal2 = (ushort)((bme680.Read8Bits(Register.hum_cal_2_msb) << 4) | (bme680.Read8Bits(Register.hum_cal_2_lsb) >> 4));
            HCal3 = (sbyte)bme680.Read8Bits(Register.hum_cal_3);
            HCal4 = (sbyte)bme680.Read8Bits(Register.hum_cal_4);
            HCal5 = (sbyte)bme680.Read8Bits(Register.hum_cal_5);
            HCal6 = bme680.Read8Bits(Register.hum_cal_6);
            HCal7 = (sbyte)(bme680.Read8Bits(Register.hum_cal_7));

            // Pressure.
            PCal1 = (ushort)bme680.Read16Bits(Register.pres_cal_1_lsb);
            PCal2 = bme680.Read16Bits(Register.pres_cal_2_lsb);
            PCal3 = bme680.Read8Bits(Register.pres_cal_3);
            PCal4 = bme680.Read16Bits(Register.pres_cal_4_lsb);
            PCal5 = bme680.Read16Bits(Register.pres_cal_5_lsb);
            PCal6 = bme680.Read8Bits(Register.pres_cal_6);
            PCal7 = bme680.Read8Bits(Register.pres_cal_7);
            PCal8 = bme680.Read16Bits(Register.pres_cal_8_lsb);
            PCal9 = bme680.Read16Bits(Register.pres_cal_9_lsb);
            PCal10 = bme680.Read8Bits(Register.pres_cal_10);
        }
    }
}
