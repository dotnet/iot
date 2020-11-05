// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Iot.Device.BrickPi3.Models
{
    /// <summary>
    /// Class containing the board information
    /// </summary>
    public class BrickPiInfo
    {
        /// <summary>
        /// Instantiate BrickPiInfo object
        /// <param name="manufacturer">Manufacturer information</param>
        /// <param name="board">Board information</param>
        /// <param name="hardwareVersion">Hardware version</param>
        /// <param name="softwareVersion">Software version</param>
        /// <param name="id">Id of the brick, can be 1 to 255</param>
        /// </summary>
        public BrickPiInfo(string manufacturer, string board, string hardwareVersion, string softwareVersion, string id)
        {
            Manufacturer = manufacturer;
            Board = board;
            HardwareVersion = hardwareVersion;
            SoftwareVersion = softwareVersion;
            Id = id;
        }

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
