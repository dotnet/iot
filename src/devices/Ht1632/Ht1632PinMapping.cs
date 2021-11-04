// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Represents pin mapping for the Ht1632 binding
    /// </summary>
    public struct Ht1632PinMapping
    {
        /// <param name="cs">Chip select input with pull-high resistor</param>
        /// <param name="wr">WRITE clock input with pull-high resistor</param>
        /// <param name="data">Serial data input or output with pull-high resistor</param>
        public Ht1632PinMapping(int cs, int wr, int data)
        {
            ChipSelect = cs;
            WriteClock = wr;
            SerialData = data;
        }

        /// <summary>
        /// Chip select input with pull-high resistor
        /// </summary>
        public int ChipSelect { get; set; }

        /// <summary>
        /// WRITE clock input with pull-high resistor
        /// </summary>
        public int WriteClock { get; set; }

        /// <summary>
        /// Serial data input or output with pull-high resistor
        /// </summary>
        public int SerialData { get; set; }
    }
}
