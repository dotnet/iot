// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Amg88xx
{
    /// <summary>
    /// Status flags of status register (0x04)
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Defines the status bits of the status register
        /// </summary>
        [Flags]
        public enum Flag : byte
        {
            /// <summary>
            /// None of the flags has been raised
            /// </summary>
            NONE = 0,

            /// <summary>
            /// Temperature output overflow of at least one pixel
            /// </summary>
            OVF_IRS = 0b0000_0100,

            /// <summary>
            /// IRGENDWAS MIT INTERRRUPT ==> DATENBLATT UNKLAR, AUSPROBIEREN
            /// </summary>
            INTF = 0b0000_0010,

            /// <summary>
            /// All status flags
            /// </summary>
            ALL = 0b0000_0110
        }

        /// <summary>
        /// Initializes a new Status instance.
        /// No flag is set by default.
        /// </summary>
        public Status()
        {
        }

        /// <summary>
        /// Initializes a new Status instance from a status register reading.
        /// </summary>
        /// <param name="status">Status register value</param>
        public Status(byte status)
        {
            TemperatureOverflow = ((Flag)status & Flag.OVF_IRS) == Flag.OVF_IRS;
            Interrupt = ((Flag)status & Flag.INTF) == Flag.INTF;
        }

        /// <summary>
        /// Gets the temperature overflow flag.
        /// </summary>
        /// <value></value>
        public bool TemperatureOverflow { get; set; } = false;

        /// <summary>
        /// Gets the interrupt flag.
        /// </summary>
        /// <value></value>
        public bool Interrupt { get; set; } = false;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"OVF_IRS={TemperatureOverflow} INTF={Interrupt}";
        }

        /// <summary>
        /// Returns a byte representing the flags.
        /// </summary>
        public byte ToByte()
        {
            return (byte)((TemperatureOverflow ? (byte)Status.Flag.OVF_IRS : 0) | (Interrupt ? (byte)Status.Flag.INTF : 0));
        }
    }
}