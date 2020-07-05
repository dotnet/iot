using System;
using System.Device.Gpio;

namespace Iot.Device.Multiplex
{
    /// <summary>
    /// Represents a node in a Charlieplexed circuit.
    /// https://wikipedia.org/wiki/Charlieplexing
    /// </summary>
    public struct CharlieplexNode
    {
        /// <summary>
        /// Anode leg (power) for a device/load
        /// </summary>
        public int Anode;

        /// <summary>
        /// Cathode leg (ground) for a device/load
        /// </summary>
        public int Cathode;

        /// <summary>
        /// Value of node
        /// </summary>
        public PinValue Value;
    }
}
