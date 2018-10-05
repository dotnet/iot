// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Devices.Spi
{
    /// <summary>
    /// Represents the settings for the connection with an <see cref="SpiDevice"/>.
    /// </summary>
    public sealed class SpiConnectionSettings
    {
        /// <summary>
        /// Gets or sets the Spi bus id for this connection.
        /// </summary>
        public uint BusId { get; set; }

        /// <summary>
        /// Gets or sets the chip select line for this connection.
        /// </summary>
        public uint ChipSelectLine { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SpiMode"/> for this connection.
        /// </summary>
        public SpiMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the bit length for data on this connection.
        /// </summary>
        public uint DataBitLength { get; set; }

        /// <summary>
        /// Gets or sets the clock frequency in Hz for the connection.
        /// </summary>
        public uint ClockFrequency { get; set; }

        private const SpiMode DefaultMode = SpiMode.Mode0;
        private const uint DefaultDataBitLength = 8; // 1 byte
        private const uint DefaultClockFrequency = 500_000; // 500 KHz

        /// <summary>
        /// Initializes new instance of <see cref="SpiConnectionSettings"/>.
        /// </summary>
        /// <param name="busId">The Spi bus id on which the connection will be made.</param>
        /// <param name="chipSelectLine">The chip select line on which the connection will be made.</param>
        public SpiConnectionSettings(uint busId, uint chipSelectLine)
        {
            BusId = busId;
            ChipSelectLine = chipSelectLine;
            DataBitLength = DefaultDataBitLength;
            ClockFrequency = DefaultClockFrequency;
            Mode = DefaultMode;
        }

        internal SpiConnectionSettings(SpiConnectionSettings other)
        {
            BusId = other.BusId;
            ChipSelectLine = other.ChipSelectLine;
            DataBitLength = other.DataBitLength;
            ClockFrequency = other.ClockFrequency;
            Mode = other.Mode;
        }
    }
}
