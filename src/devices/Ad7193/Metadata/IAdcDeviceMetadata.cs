namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Metadata interface for ADC (Analog-to-Digital Converter) IoT devices
    /// </summary>
    public interface IAdcDeviceMetadata
    {
        /// <summary>
        /// Number of ADCs on the device
        /// </summary>
        int ADCCount { get; }

        /// <summary>
        /// The maximum bitrate of the ADC
        /// </summary>
        int ADCBitrate { get; }

        /// <summary>
        /// The maximum sampling rate of the ADC
        /// </summary>
        uint ADCSamplerate { get; }

        /// <summary>
        /// The number of channels the ADC has
        /// </summary>
        int ADCInputChannelCount { get; }
    }
}
