// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bmxx80.CalibrationData
{
    /// <summary>
    /// Calibration data for the Bmxx80 family.
    /// </summary>
    internal abstract class Bmxx80CalibrationData
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
        
        /// <summary>
        /// Read coefficient data from device.
        /// </summary>
        /// <param name="bmxx80Base">The <see cref="Bmxx80Base"/> to read coefficient data from.</param>
        protected internal abstract void ReadFromDevice(Bmxx80Base bmxx80Base);
    }
}
