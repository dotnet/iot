using System.Device.Spi;

namespace Iot.Device.Adg731
{
    /// <summary>
    /// Metadata class for ADG731
    /// </summary>
    public class Adg731Metadata : IDeviceMetadata, ISpiDeviceMetadata, IMultiplexerDeviceMetadata
    {
        /// <inheritdoc/>
        public string Manufacturer => "Analog Devices";

        /// <inheritdoc/>
        public string Product => "ADG731";

        /// <inheritdoc/>
        public string ProductCategory => "Analog Multiplexer";

        /// <inheritdoc/>
        public string ProductDescription => "32-Channel, Serially Controlled 4 Ohm 1.8 V to 5.5 V, +/- 2.5 V, Analog Multiplexer";

        /// <inheritdoc/>
        public string DataSheetURI => "https://www.analog.com/media/en/technical-documentation/data-sheets/ADG725_731.pdf";

        /// <inheritdoc/>
        public SpiMode ValidSpiModes => SpiMode.Mode1 | SpiMode.Mode2;

        /// <inheritdoc/>
        public int MaximumSpiFrequency => 30000000;

        /// <inheritdoc/>
        public int MultiplexerCount => 1;

        /// <inheritdoc/>
        public int MultiplexerChannelCount => 32;
    }
}
