// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Class containing the board information
    /// </summary>
    public class BrickPiInfo
    {
        /// <summary>
        /// Manufacturer information
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Board information
        /// </summary>
        public string Board { get; set; }

        /// <summary>
        /// HArdware version
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// Firmware version
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// Id of the brick, can be 1 to 255
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Get the Hardware version as an int array
        /// </summary>
        /// <returns>Returns an int array of the hardware version</returns>
        public int[] GetHardwareVersion()
        {
            return GetVersionsFromString(HardwareVersion);
        }

        /// <summary>
        /// Get the firmware version as an int array
        /// </summary>
        /// <returns>Returns an int array of the firmware version</returns>
        public int[] GetSoftwareVersion()
        {
            return GetVersionsFromString(SoftwareVersion);
        }

        private int[] GetVersionsFromString(string toconvert)
        {
            if (toconvert == string.Empty)
            {
                return null;
            }

            var split = toconvert.Split('.');
            List<int> ret = new List<int>();
            foreach (var elem in split)
            {
                ret.Add(int.Parse(elem));
            }

            return ret.ToArray();
        }
    }
}
