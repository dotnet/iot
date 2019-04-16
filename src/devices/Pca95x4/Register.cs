// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pca95x4
{
    public enum Register
    {
        /// <summary>
        /// This register is a read-only port. It reflects the incoming logic levels of the pins,
        /// regardless of whether the pin is defined as an input or an output by Register 3.
        /// Writes to this register have no effect.
        /// </summary>
        InputPort = 0x00,
        /// <summary>
        /// This register reflects the outgoing logic levels of the pins defined as outputs by Register 3.
        /// Bit values in this register have no effect on pins defined as inputs. Reads from this register
        /// return the value that is in the flip-flop controlling the output selection, not the actual pin value.
        /// </summary>
        OutputPort = 0x01,
        /// <summary>
        /// This register allows the user to invert the polarity of the Input Port register data.
        /// If a bit in this register is set (written with ‘1’), the corresponding Input Port data is inverted.
        /// If a bit in this register is cleared (written with a '0'), the Input Port data polarity is retained.
        /// </summary>
        PolarityInversion = 0x02,
        /// <summary>
        /// This register configures the directions of the I/O pins. If a bit in this register is set,
        /// the corresponding port pin is enabled as an input with high-impedance output driver.
        /// If a bit in this register is cleared, the corresponding port pin is enabled as an output.
        /// At reset, the I/Os are configured as inputs with a weak pull-up to VDD.
        /// </summary>
        Configuration = 0x03
    }
}
