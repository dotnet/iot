// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents a node in a Charlieplexed circuit.
    /// https://wikipedia.org/wiki/Charlieplexing
    /// </summary>
    public struct CharlieplexSegmentNode
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
