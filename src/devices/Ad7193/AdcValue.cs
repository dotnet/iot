namespace Iot.Device.Ad7193
{
    /// <summary>
    /// Represents a complete ADC value, including the originally received raw integer value
    /// </summary>
    public class AdcValue
    {
        /// <summary>
        /// The channel on which the value was received
        /// </summary>
        public byte Channel { get; set; }

        /// <summary>
        /// The raw (24-bit integer) value of the ADC
        /// </summary>
        public uint Raw { get; set; }

        /// <summary>
        /// The voltage value of the ADC
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// A relative time when the sample was received from the ADC
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// A simple text-representation of the measured voltage
        /// </summary>
        /// <returns>The channel number and voltage value on that channel</returns>
        public override string ToString()
        {
            return $"Ch{Channel.ToString().PadLeft(5, ' ')} {Voltage.ToString("0.0000").PadLeft(10, ' ')} V";
        }
    }
}
