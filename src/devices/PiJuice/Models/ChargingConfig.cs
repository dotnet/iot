// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// TODO: Fill In
    /// </summary>
    public class ChargingConfig
    {
        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool NonVolatile { get; set; }

        /// <summary>Converts to array.</summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte v = 0;
            if (Enabled)
            {
                v |= 0x01;
            }

            if (NonVolatile)
            {
                v |= 0x80;
            }

            return new byte[] { v };
        }
    }
}
