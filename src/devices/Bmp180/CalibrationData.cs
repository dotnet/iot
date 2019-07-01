// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmp180
{
    internal class CalibrationData
    {
        public short AC1 { get; set; }
        public short AC2 { get; set; }
        public short AC3 { get; set; }
        public ushort AC4 { get; set; }
        public ushort AC5 { get; set; }
        public ushort AC6 { get; set; }

        public short B1 { get; set; }
        public short B2 { get; set; }

        public short MB { get; set; }
        public short MC { get; set; }
        public short MD { get; set; }

        internal void ReadFromDevice(Bmp180 bmp180)
        {
            AC1 = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.AC1);
            AC2 = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.AC2);
            AC3 = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.AC3);
            AC4 = bmp180.Read16BitsFromRegisterBE((byte)Register.AC4);
            AC5 = bmp180.Read16BitsFromRegisterBE((byte)Register.AC5);
            AC6 = bmp180.Read16BitsFromRegisterBE((byte)Register.AC6);

            B1 = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.B1);
            B2 = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.B2);
           
            MB = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.MB);
            MC = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.MC);
            MD = (short)bmp180.Read16BitsFromRegisterBE((byte)Register.MD);            
        }
    }
}
