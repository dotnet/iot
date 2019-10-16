// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bmxx80.Register;

namespace Iot.Device.Bmxx80.CalibrationData
{
    /// <summary>
    /// Calibration data for the BME280.
    /// </summary>
    internal class Bme280CalibrationData : Bmp280CalibrationData
    {
        public byte DigH1 { get; set; }
        public short DigH2 { get; set; }
        public ushort DigH3 { get; set; }
        public short DigH4 { get; set; }
        public short DigH5 { get; set; }
        public sbyte DigH6 { get; set; }

        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bmxx80Base">The <see cref="Bmxx80Base"/> to read coefficient data from.</param>
        protected internal override void ReadFromDevice(Bmxx80Base bmxx80Base)
        {
            // Read humidity calibration data.
            DigH1 = bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H1);
            DigH2 = (short)bmxx80Base.Read16BitsFromRegister((byte)Bme280Register.DIG_H2);
            DigH3 = bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H3);
            DigH4 = (short)((bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H4) << 4) | (bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H4 + 1) & 0xF));
            DigH5 = (short)((bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H5 + 1) << 4) | (bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H5) >> 4));
            DigH6 = (sbyte)bmxx80Base.Read8BitsFromRegister((byte)Bme280Register.DIG_H6);

            base.ReadFromDevice(bmxx80Base);
        }
    }
}
