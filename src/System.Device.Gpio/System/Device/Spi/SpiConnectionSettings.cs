// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Device.Gpio;

namespace System.Device.Spi
{
    /// <summary>
    /// The connection settings of a device on a SPI bus.
    /// </summary>
    public sealed class SpiConnectionSettings
    {
        private SpiConnectionSettings() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiConnectionSettings"/> class.
        /// </summary>
        /// <param name="busId">The bus ID the device is connected to.</param>
        /// <param name="chipSelectLine">The chip select line used on the bus.</param>
        public SpiConnectionSettings(int busId, int chipSelectLine)
        {
            BusId = busId;
            ChipSelectLine = chipSelectLine;
        }

        internal SpiConnectionSettings(SpiConnectionSettings other)
        {
            BusId = other.BusId;
            ChipSelectLine = other.ChipSelectLine;
            Mode = other.Mode;
            DataBitLength = other.DataBitLength;
            ClockFrequency = other.ClockFrequency;
            DataFlow = other.DataFlow;
            ChipSelectLineActiveState = other.ChipSelectLineActiveState;
        }

        /// <summary>
        /// The bus ID the device is connected to.
        /// </summary>
        public int BusId { get; set; }

        /// <summary>
        /// The chip select line used on the bus.
        /// </summary>
        public int ChipSelectLine { get; set; }

        /// <summary>
        /// The SPI mode being used.
        /// </summary>
        public SpiMode Mode { get; set; } = SpiMode.Mode0;

        /// <summary>
        /// The length of the data to be transfered.
        /// </summary>
        public int DataBitLength { get; set; } = 8;  // 1 byte

        /// <summary>
        /// The frequency in which the data will be transferred.
        /// </summary>
        public int ClockFrequency { get; set; } = 500_000; // 500 KHz

        /// <summary>
        /// Specifies order in which bits are transferred first on the SPI bus.
        /// </summary>
        public DataFlow DataFlow { get; set; } = DataFlow.MsbFirst;

        /// <summary>
        /// Specifies which value on chip select pin means "active".
        /// </summary>
        public PinValue ChipSelectLineActiveState { get; set; } = PinValue.Low;
    }
}
