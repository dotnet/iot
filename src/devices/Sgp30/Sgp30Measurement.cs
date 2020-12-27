namespace Iot.Device.Sgp30
{
    /// <summary>
    /// SGP30 Gss sensor measurement result.
    /// </summary>
    public struct Sgp30Measurement
    {
        /// <summary>
        /// Total VOC result (parts per billion).
        /// /// </summary>
        public ushort Tvoc;

        /// <summary>
        /// Equivalent CO2 result (parts per million).
        /// </summary>
        public ushort Eco2;
    }
}
