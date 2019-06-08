// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Iot.Device.Bmx280.Register;

namespace Iot.Device.Bmx280
{
    internal class CalibrationData
    {
        public ushort DigT1 { get; set; }
        public short DigT2 { get; set; }
        public short DigT3 { get; set; }

        public ushort DigP1 { get; set; }
        public short DigP2 { get; set; }
        public short DigP3 { get; set; }
        public short DigP4 { get; set; }
        public short DigP5 { get; set; }
        public short DigP6 { get; set; }
        public short DigP7 { get; set; }
        public short DigP8 { get; set; }
        public short DigP9 { get; set; }

        internal virtual void ReadFromDevice(BmxBase bmxBase)
        {
            // Read temperature calibration data
            DigT1 = bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_T1);
            DigT2 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_T2);
            DigT3 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_T3);

            // Read pressure calibration data
            DigP1 = bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P1);
            DigP2 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P2);
            DigP3 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P3);
            DigP4 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P4);
            DigP5 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P5);
            DigP6 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P6);
            DigP7 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P7);
            DigP8 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P8);
            DigP9 = (short)bmxBase.Read16BitsFromRegister((byte)Bmx280Register.DIG_P9);
        }
    }
}
