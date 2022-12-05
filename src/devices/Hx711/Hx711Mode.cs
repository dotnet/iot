namespace Iot.Device.HX711
{
    /// <summary>
    /// HX711 has 3 modes of operation, choose the one based on the fisical connection with load cell.
    /// </summary>
    public enum Hx711Mode
    {
        /// <summary>
        /// Load cell link in channel A and use gain of 128, default mode
        /// </summary>
        ChannelAGain128,

        /// <summary>
        /// Load cell link in channel A and use gain of 128 
        /// </summary>
        ChannelAGain64,

        /// <summary>
        /// Load cell link in channel A and use gain of 128 
        /// </summary>
        ChannelBGain32
    }
}
