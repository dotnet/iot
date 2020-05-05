using System.Device.Spi;

namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Metadata class for AD7193
    /// </summary>
    public class Ad7193Metadata : IDeviceMetadata, ISpiDeviceMetadata, IAdcDeviceMetadata
    {
        /// <inheritdoc/>
        public string Manufacturer => "Analog Devices";

        /// <inheritdoc/>
        public string Product => "AD7193";

        /// <inheritdoc/>
        public string ProductCategory => "ADC";

        /// <inheritdoc/>
        public string ProductDescription => "4-Channel, 4.8 kHz, Ultralow Noise, 24-Bit Sigma-Delta ADC with PGA";

        /// <inheritdoc/>
        public string DataSheetURI => "https://www.analog.com/media/en/technical-documentation/data-sheets/AD7193.pdf";

        /// <inheritdoc/>
        public SpiMode ValidSpiModes => SpiMode.Mode3;

        /// <inheritdoc/>
        public int MaximumSpiFrequency => 10000000;

        /// <inheritdoc/>
        public int ADCCount => 1;

        /// <inheritdoc/>
        public int ADCBitrate => 24;

        /// <inheritdoc/>
        public uint ADCSamplerate => 4800;

        /// <inheritdoc/>
        public int ADCInputChannelCount => 8;
    }
}
