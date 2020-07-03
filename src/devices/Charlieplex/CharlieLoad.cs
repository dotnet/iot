using System;

namespace Iot.Device.Multiplex
{
    /// <summary>
    /// Provides support for Charlieplex multiplexing.
    /// https://wikipedia.org/wiki/Charlieplexing
    /// </summary>
    public struct CharlieLoad
    {
        /// <summary>
        /// Anode leg (power) for a device/load
        /// </summary>
        public int Anode;

        /// <summary>
        /// Cathode leg (ground) for a device/load
        /// </summary>
        public int Cathode;
    }
}
