using System.Device.Spi;

namespace Iot.Device.Adg731
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
