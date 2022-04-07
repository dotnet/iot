// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Information about a satellite, part of a satellite constellation
    /// </summary>
    public class SatelliteInfo
    {
        /// <summary>
        /// Construct an instance using a satellite ID
        /// </summary>
        /// <param name="id">Id of the satellite</param>
        public SatelliteInfo(string id)
        {
            Id = id;
        }

        /// <summary>
        /// ID of the satellite
        /// </summary>
        public string Id
        {
            get;
            internal set;
        }

        /// <summary>
        /// Azimuth where the satellite is. This gives the true direction to the satellite from the current position
        /// </summary>
        public Angle? Azimuth
        {
            get;
            internal set;
        }

        /// <summary>
        /// Elevation of the satellite. This gives the elevation of the satellite (values close to 0 mean the satellite
        /// is near the horizon, values close to 90 mean it is right above the observer)
        /// </summary>
        public Angle? Elevation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Signal to noise ratio of this satellite. A large value is better.
        /// </summary>
        public double? Snr
        {
            get;
            internal set;
        }
    }
}
