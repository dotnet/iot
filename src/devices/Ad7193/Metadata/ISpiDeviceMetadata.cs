using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Text;

namespace Ad7193.Metadata
{
    /// <summary>
    /// Metadata interface for SPI IoT devices
    /// </summary>
    public interface ISpiDeviceMetadata : IDeviceMetadata
    {
        /// <summary>
        /// The list of SPI modes that are valid for this device
        /// </summary>
        SpiMode ValidSpiModes { get; }

        /// <summary>
        /// The maximum frequency that can be used on the SPI bus
        /// </summary>
        int MaximumSpiFrequency { get; }
    }
}
