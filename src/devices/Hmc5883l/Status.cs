// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// The status of HMC5883L device
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Regulator Enabled Bit. This bit is set when the internal voltage regulator is enabled.
        /// This bit is cleared when the internal regulator is disabled.
        /// </summary>
        RegulatorEnabled,
        /// <summary>
        /// Data output register lock. This bit is set when some but not all for of the six data output registers have been read.
        /// When this bit is set, the six data output registers are locked and any new data will not be placed in these register until
        /// one of four conditions are met: all six have been read or the mode changed, a POR is issued, the mode is changed, the measurement is changed.
        /// </summary>
        Lock,
        /// <summary>
        /// Ready Bit. Set when data is written to all six data registers. Cleared when device initiates a write to the data output
        /// registers, when in off mode, and after one or more of the data output registers are written to. 
        /// When RDY bit is clear it shall remain cleared for a minimum of a 250 μs. 
        /// </summary>
        Ready
    }
}