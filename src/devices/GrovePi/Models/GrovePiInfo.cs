using System;
using System.Collections.Generic;
using System.Text;

namespace Iot.Device.GrovePiDevice.Models
{
    public class GrovePiInfo
    {
        /// <summary>
        /// Manufacturer information
        /// </summary>
        public string Manufacturer => "Dexter Industries";

        /// <summary>
        /// Board information
        /// </summary>
        public string Board => "GrovePi+";

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version SoftwareVersion { get; set; }
    }
}
