// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// Mcp23xxx Register
    /// </summary>
    public enum Register : byte
    {
        /// <summary>
        /// Controls the direction of the data I/O.
        /// When a bit is set, the corresponding pin becomes an input.
        /// When a bit is clear, the corresponding pin becomes an output.
        /// </summary>
        /// <remarks>
        /// On reset/power on all bits are set (all pins are input).
        /// </remarks>
        IODIR = 0x00,

        /// <summary>
        /// Configures the polarity on the corresponding GPIO port bits.
        /// When a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.
        /// </summary>
        IPOL = 0x01,

        /// <summary>
        /// Controls the interrupt-on-change feature for each pin.
        /// When a bit is set, the corresponding pin is enabled for interrupt-on-change.
        /// The DEFVAL and INTCON registers must also be configured if any pins are enabled for interrupt-on-change.
        /// </summary>
        GPINTEN = 0x02,

        /// <summary>
        /// Configures the default comparison value.
        /// If enabled (via GPINTEN and INTCON) to compare against the DEFVAL register,
        /// an opposite value on the associated pin will cause an interrupt to occur.
        /// </summary>
        DEFVAL = 0x03,

        /// <summary>
        /// Controls how the associated pin value is compared for the interrupt-on-change feature.
        /// When a bit is set, the corresponding I/O pin is compared against the associated bit in the DEFVAL register.
        /// When a bit value is clear, the corresponding I/O pin is compared against the previous value.
        /// </summary>
        INTCON = 0x04,

        /// <summary>
        /// Contains several bits for configuring the device.  See respective datasheet for more details.
        /// </summary>
        IOCON = 0x05,

        /// <summary>
        /// Controls the pull-up resistors for the port pins.
        /// When a bit is set and the corresponding pin is configured as an input,
        /// the corresponding port pin is internally pulled up with a 100 kΩ resistor.
        /// </summary>
        GPPU = 0x06,

        /// <summary>
        /// Reflects the interrupt condition on the port pins of any pin that is enabled for interrupts via the GPINTEN register.
        /// A 'set' bit indicates that the associated pin caused the interrupt.
        /// This register is read-only. Writes to this register will be ignored.
        /// </summary>
        INTF = 0x07,

        /// <summary>
        /// The INTCAP register captures the GPIO port value at the time the interrupt occurred.
        /// The register is read-only and is updated only when an interrupt occurs.
        /// The register will remain unchanged until the interrupt is cleared via a read of INTCAP or GPIO.
        /// </summary>
        INTCAP = 0x08,

        /// <summary>
        /// Reflects the value on the port. Reading from this register reads the port.
        /// Writing to this register modifies the Output Latch (OLAT) register.
        /// </summary>
        GPIO = 0x09,

        /// <summary>
        /// Provides access to the output latches.
        /// A read from this register results in a read of the OLAT and not the port itself.
        /// A write to this register modifies the output latches that modify the pins configured as outputs.
        /// </summary>
        /// <remarks>
        /// On reset/power on all bits are not set (all pins are low).
        /// </remarks>
        OLAT = 0x0a
    }
}
