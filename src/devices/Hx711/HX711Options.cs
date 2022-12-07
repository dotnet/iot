// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.HX711
{
    /// <summary>
    /// Hx711 options for all manufacturers
    /// </summary>
    public sealed class HX711Options
    {
        /// <summary>
        /// Hx711 has 3 modes of operation, choose the one based on the fisical connection with load cell.
        /// Default value: <code>Mode = Hx711Mode.ChannelAGain128</code>
        /// </summary>
        public Hx711Mode Mode { get; private set; }

        /// <summary>
        /// If <code>true</code> bytes read from Hx711 made by LSB format.
        /// Some Hx711 manufacturers return bytes in LSB, but most in MSB.
        /// Default value: <code>UseByteLittleEndian = false</code>
        /// </summary>
        public bool UseByteLittleEndian { get; private set; }

        public HX711Options()
        {
            this.Mode = Hx711Mode.ChannelAGain128;
            this.UseByteLittleEndian = false;
        }

        public HX711Options(Hx711Mode mode, bool useByteLittleEndian)
        {
            this.Mode = mode;
            this.UseByteLittleEndian = useByteLittleEndian;
        }
    }
}
