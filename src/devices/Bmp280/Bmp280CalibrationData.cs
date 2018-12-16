// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device
{
    internal class Bmp280CalibrationData
    {
        public uint DigT1 { get; set; }
        public int DigT2 { get; set; }
        public int DigT3 { get; set; }

        public uint DigP1 { get; set; }
        public int DigP2 { get; set; }
        public int DigP3 { get; set; }
        public int DigP4 { get; set; }
        public int DigP5 { get; set; }
        public int DigP6 { get; set; }
        public int DigP7 { get; set; }
        public int DigP8 { get; set; }
        public int DigP9 { get; set; }
    }
}
