// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Status flags for QMC5883L first register
    /// </summary>
    [Flags]
    public enum Status : byte
    {
        /// <summary>
        /// Data Ready Register (DRDY), it is set when all three axis data is ready,
        /// and loaded to the output data registers in the continuous measurement mode
        /// It is reset to “0” by reading any data register (00H~05H) through I2C commands
        /// DRDY: “0”: no new data, “1”: new data is ready
        /// </summary>
        DRDY = 0x01,

        /// <summary>
        /// Overflow flag (OVL) is set to “1” if any data of three axis magnetic sensor channels is out of range.
        /// The output data of each axis saturates at -32768 and 32767, if any of the axis exceeds this range, OVL flag is set to “1”
        /// This flag is reset to “0” if next measurement goes back to the range of (-32768, 32767), otherwise, it keeps as “1”
        /// OVL: “0”: normal, “1”: data overflow
        /// </summary>
        OVL = 0x02,

        /// <summary>
        /// Data Skip (DOR) bit is set to “1” if all the channels of output data registers are skipped in reading in the
        /// continuous-measurement mode. It is reset to “0” by reading any data register (00H~05H) through I2C.
        /// DOR: “0”: normal, “1”: data skipped for reading
        /// </summary>
        DOR = 0x04,

    }
}
