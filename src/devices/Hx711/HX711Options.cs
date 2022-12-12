// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hx711
{
    /// <summary>
    /// Hx711 options for all manufacturers
    /// </summary>
    public sealed class Hx711Options
    {
        /// <summary>
        /// Hx711 has 3 modes of operation, choose the one based on the fisical connection with load cell.
        /// Default value: <code>Mode = Hx711Mode.ChannelAGain128</code>
        /// </summary>
        public Hx711Mode Mode { get; private set; }

        /// <summary>
        /// If <code>true</code> bytes read from Hx711 made by Lsb format.
        /// Some Hx711 manufacturers return bytes in Lsb, but most in Msb.
        /// Default value: <code>UseByteLittleEndian = false</code>
        /// </summary>
        public bool UseByteLittleEndian { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hx711Options"/> class with default values.
        /// </summary>
        public Hx711Options()
        {
            Mode = Hx711Mode.ChannelAGain128;
            UseByteLittleEndian = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Hx711Options"/> class.
        /// </summary>
        /// <param name="mode">Hx711 has 3 modes of operation, choose the one based on the fisical connection with load cell.</param>
        /// <param name="useByteLittleEndian">If <code>true</code> bytes read from Hx711 made by Lsb format.</param>
        public Hx711Options(Hx711Mode mode, bool useByteLittleEndian)
        {
            Mode = mode;
            UseByteLittleEndian = useByteLittleEndian;
        }
    }
}
