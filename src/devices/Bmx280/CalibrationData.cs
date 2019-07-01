// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmx280
{
    internal class CalibrationData
    {
        // Temperature calibration data
        public ushort DigT1 { get; set; }
        public short DigT2 { get; set; }
        public short DigT3 { get; set; }

        // Pressure calibration data
        public ushort DigP1 { get; set; }
        public short DigP2 { get; set; }
        public short DigP3 { get; set; }
        public short DigP4 { get; set; }
        public short DigP5 { get; set; }
        public short DigP6 { get; set; }
        public short DigP7 { get; set; }
        public short DigP8 { get; set; }
        public short DigP9 { get; set; }

        // Humidity calibration data (BME280 Only)
        public byte DigH1 { get; set; }
        public short DigH2 { get; set; }
        public ushort DigH3 { get; set; }
        public short DigH4 { get; set; }
        public short DigH5 { get; set; }
        public sbyte DigH6 { get; set; }
    }
}
