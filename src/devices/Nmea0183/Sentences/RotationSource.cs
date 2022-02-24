namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Source for the engine revolution data
    /// </summary>
    public enum RotationSource
    {
        /// <summary>
        /// The source is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Engine revolutions
        /// </summary>
        Engine,

        /// <summary>
        /// Shaft revolutions
        /// </summary>
        Shaft
    }
}
