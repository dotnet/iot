// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// LED blink pattern
    /// </summary>
    public class LedBlink
    {
        /// <summary>
        /// LED designator
        /// </summary>
        public LED Led { get; set; }

        /// <summary>
        /// Blink indefinite
        /// </summary>
        public bool BlinkIndefinite { get; set; }

        /// <summary>
        /// Number of blinks between [1 - 254]
        /// </summary>
        public short Count { get; set; }

        /// <summary>
        /// Color for first period of blink
        /// </summary>
        public Color RGB1 { get; set; }

        /// <summary>
        /// Duration of first blink period in milliseconds between [10 – 2550]
        /// </summary>
        public int Period1 { get; set; }

        /// <summary>
        /// Color for second period of blink
        /// </summary>
        public Color RGB2 { get; set; }

        /// <summary>
        /// Duration of second blink period in milliseconds between [10 – 2550]
        /// </summary>
        public int Period2 { get; set; }
    }
}
