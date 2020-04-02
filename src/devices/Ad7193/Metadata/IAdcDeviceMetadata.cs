using System;
using System.Collections.Generic;
using System.Text;

namespace Ad7193.Metadata
{
    /// <summary>
    /// Metadata interface for ADC (Analog-to-Digital Converter) IoT devices
    /// </summary>
    public interface IAdcDeviceMetadata
    {
        /// <summary>
        /// Number of ADCs on the device
        /// </summary>
        public int ADCCount { get; }

        /// <summary>
        /// The maximum bitrate of the ADC
        /// </summary>
        public int ADCBitrate { get; }

        /// <summary>
        /// The maximum sampling rate of the ADC
        /// </summary>
        public uint ADCSamplerate { get; }

        /// <summary>
        /// The number of channels the ADC has
        /// </summary>
        public int ADCInputChannelCount { get; }
    }
}
