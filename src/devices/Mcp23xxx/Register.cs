// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp23xxx
{
    public class Register
    {
        public enum Address : byte
        {
            /// <summary>
            /// Controls the direction of the data I/O.
            /// When a bit is set, the corresponding pin becomes an input.
            /// When a bit is clear, the corresponding pin becomes an output.
            /// </summary>
            IODIR = 0b0000_0000,
            /// <summary>
            /// Configures the polarity on the corresponding GPIO port bits.
            /// When a bit is set, the corresponding GPIO register bit will reflect the inverted value on the pin.
            /// </summary>
            IPOL = 0b0000_0001,
            /// <summary>
            /// Controls the interrupt-on-change feature for each pin.
            /// When a bit is set, the corresponding pin is enabled for interrupt-on-change.
            /// The DEFVAL and INTCON registers must also be configured if any pins are enabled for interrupt-on-change.
            /// </summary>
            GPINTEN = 0b0000_0010,
            /// <summary>
            /// Configures the default comparison value.
            /// If enabled (via GPINTEN and INTCON) to compare against the DEFVAL register,
            /// an opposite value on the associated pin will cause an interrupt to occur.
            /// </summary>
            DEFVAL = 0b0000_0011,
            /// <summary>
            /// Controls how the associated pin value is compared for the interrupt-on-change feature.
            /// When a bit is set, the corresponding I/O pin is compared against the associated bit in the DEFVAL register.
            /// When a bit value is clear, the corresponding I/O pin is compared against the previous value.
            /// </summary>
            INTCON = 0b0000_0100,
            /// <summary>
            /// Contains several bits for configuring the device.  See respective datasheet for more details.
            /// </summary>
            IOCON = 0b0000_0101,
            /// <summary>
            /// Controls the pull-up resistors for the port pins.
            /// When a bit is set and the corresponding pin is configured as an input,
            /// the corresponding port pin is internally pulled up with a 100 kΩ resistor.
            /// </summary>
            GPPU = 0b0000_0110,
            /// <summary>
            /// Reflects the interrupt condition on the port pins of any pin that is enabled for interrupts via the GPINTEN register.
            /// A 'set' bit indicates that the associated pin caused the interrupt.
            /// This register is read-only. Writes to this register will be ignored.
            /// </summary>
            INTF = 0b0000_0111,
            /// <summary>
            /// The INTCAP register captures the GPIO port value at the time the interrupt occurred.
            /// The register is read-only and is updated only when an interrupt occurs.
            /// The register will remain unchanged until the interrupt is cleared via a read of INTCAP or GPIO.
            /// </summary>
            INTCAP = 0b0000_1000,
            /// <summary>
            /// Reflects the value on the port. Reading from this register reads the port.
            /// Writing to this register modifies the Output Latch (OLAT) register.
            /// </summary>
            GPIO = 0b0000_1001,
            /// <summary>
            /// Provides access to the output latches.
            /// A read from this register results in a read of the OLAT and not the port itself.
            /// A write to this register modifies the output latches that modify the pins configured as outputs.
            /// </summary>
            OLAT = 0b0000_1010
        }

        /// <summary>
        /// Gets the mapped address for a register.
        /// </summary>
        /// <param name="address">The register address.</param>
        /// <param name="port">The I/O port used with the register.</param>
        /// <param name="bank">The bank type that determines how the register is mapped.</param>
        /// <returns></returns>
        public static byte GetMappedAddress(Address address, Port port = Port.PortA, Bank bank = Bank.Bank1)
        {
            byte mappedAddress;

            if (bank == Bank.Bank1)
            {
                mappedAddress = GetMappedAddressBank1(address, port);
            }
            else
            {
                mappedAddress = GetMappedAddressBank0(address, port);
            }

            return mappedAddress;
        }

        private static byte GetMappedAddressBank0(Address address, Port port)
        {
            byte mappedAddress;

            if (port == Port.PortA)
            {
                mappedAddress = GetMappedAddressBank0PortA(address);
            }
            else
            {
                mappedAddress = GetMappedAddressBank0PortB(address);
            }

            return mappedAddress;
        }

        private static byte GetMappedAddressBank0PortA(Address address)
        {
            
            byte mappedAddress = 0;

            switch (address)
            {
                case Address.IODIR:
                    mappedAddress = 0b0000_0000;
                    break;
                case Address.IPOL:
                    mappedAddress = 0b0000_0010;
                    break;
                case Address.GPINTEN:
                    mappedAddress = 0b0000_0100;
                    break;
                case Address.DEFVAL:
                    mappedAddress = 0b0000_0110;
                    break;
                case Address.INTCON:
                    mappedAddress = 0b0000_1000;
                    break;
                case Address.IOCON:
                    mappedAddress = 0b0000_1010;
                    break;
                case Address.GPPU:
                    mappedAddress = 0b0000_1100;
                    break;
                case Address.INTF:
                    mappedAddress = 0b0000_1110;
                    break;
                case Address.INTCAP:
                    mappedAddress = 0b0001_0000;
                    break;
                case Address.GPIO:
                    mappedAddress = 0b0001_0010;
                    break;
                case Address.OLAT:
                    mappedAddress = 0b0001_0100;
                    break;
            }

            return mappedAddress;
        }

        private static byte GetMappedAddressBank0PortB(Address address)
        {
            byte mappedAddress = 0;

            switch (address)
            {
                case Address.IODIR:
                    mappedAddress = 0b0000_0001;
                    break;
                case Address.IPOL:
                    mappedAddress = 0b0000_0011;
                    break;
                case Address.GPINTEN:
                    mappedAddress = 0b0000_0101;
                    break;
                case Address.DEFVAL:
                    mappedAddress = 0b0000_0111;
                    break;
                case Address.INTCON:
                    mappedAddress = 0b0000_1001;
                    break;
                case Address.IOCON:
                    mappedAddress = 0b0000_1011;
                    break;
                case Address.GPPU:
                    mappedAddress = 0b0000_1101;
                    break;
                case Address.INTF:
                    mappedAddress = 0b0000_1111;
                    break;
                case Address.INTCAP:
                    mappedAddress = 0b0001_0001;
                    break;
                case Address.GPIO:
                    mappedAddress = 0b0001_0011;
                    break;
                case Address.OLAT:
                    mappedAddress = 0b0001_0101;
                    break;
            }

            return mappedAddress;
        }

        private static byte GetMappedAddressBank1(Address address, Port port)
        {
            byte mappedAddress = (byte)address;

            if (port == Port.PortB)
            {
                mappedAddress |= 0b0001_0000;
            }

            return mappedAddress;
        }
    }
}
