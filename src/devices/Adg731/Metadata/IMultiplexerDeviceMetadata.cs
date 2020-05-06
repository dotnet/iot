namespace Iot.Device.Adg731
{
    /// <summary>
    /// Metadata interface for multiplexer IoT devices
    /// </summary>
    public interface IMultiplexerDeviceMetadata
    {
        /// <summary>
        /// Number of multiplexers on the device
        /// </summary>
        int MultiplexerCount { get; }

        /// <summary>
        /// The number of channels the multiplexer has
        /// </summary>
        int MultiplexerChannelCount { get; }
    }
}
