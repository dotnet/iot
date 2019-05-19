namespace Iot.Device.Bmx280
{
    public class DeviceStatus
    {
        /// <summary>
        /// True whenever a conversion is running and False when the results have been transferred to the data registers.
        /// </summary>
        public bool Measuring { get; set; }

        /// <summary>
        /// True when the NVM data is being copied to images registers and False whe the copying is done.
        /// The data is copied at power-on-reset and before every conversion.
        /// </summary>
        public bool ImageUpdating { get; set; }
    }
}
