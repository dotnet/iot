// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Iot.Device.GoPiGo3.Models
{
    public class GoPiGoInfo
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
        /// Hardware version
        /// </summary>
        public Version HardwareVersion { get; set; }

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version SoftwareVersion { get; set; }

        /// <summary>
        /// Id of the GoPiGo3
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Get the Hardware version as an int array
        /// </summary>
        /// <returns>Returns an int array of the hardware version</returns>
        public int[] GetHardwareVersion()
        {
            return GetVersionsFromString(HardwareVersion.ToString());
        }

        /// <summary>
        /// Get the firmware version as an int array
        /// </summary>
        /// <returns>Returns an int array of the firmware version</returns>
        public int[] GetSoftwareVersion()
        {
            return GetVersionsFromString(SoftwareVersion.ToString());
        }

        private int[] GetVersionsFromString(string toconvert)
        {
            if (toconvert == "")
                return null;
            var split = toconvert.Split('.');
            List<int> ret = new List<int>();
            foreach (var elem in split)
                ret.Add(int.Parse(elem));
            return ret.ToArray();
        }
    }
}
