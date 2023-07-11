// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// An Aid-To-Navigation or AtoN target.
    /// There are two kinds of AtoN targets:
    /// - Physical targets. These are Buoys or lighthouses that are equipped with AIS transmitters so they can be easier located.
    /// - Virtual targets. The transmitter is located somewhere else, reporting a buoy in open water. No real buoy needs to be there.
    /// </summary>
    public record AidToNavigation : AisTarget
    {
        /// <summary>
        /// Create an AtoN target
        /// </summary>
        /// <param name="mmsi">The MMSI</param>
        public AidToNavigation(uint mmsi)
        : base(mmsi)
        {
            Position = new GeographicPosition();
        }

        /// <summary>
        /// Dimension of the target from the transmitter to its bow. Since most AtoN targets are navigational marks, this is usually not set.
        /// </summary>
        public Length DimensionToBow { get; set; }

        /// <summary>
        /// Dimension of the target from the transmitter to its stern. Since most AtoN targets are navigational marks, this is usually not set.
        /// </summary>
        public Length DimensionToStern { get; set; }

        /// <summary>
        /// Dimension of the target from the transmitter to its port side. Since most AtoN targets are navigational marks, this is usually not set.
        /// </summary>
        public Length DimensionToPort { get; set; }

        /// <summary>
        /// Dimension of the target from the transmitter to its starboard side. Since most AtoN targets are navigational marks, this is usually not set.
        /// </summary>
        public Length DimensionToStarboard { get; set; }

        /// <summary>
        /// The length of the object
        /// </summary>
        public Length Length => DimensionToBow + DimensionToStern;

        /// <summary>
        /// The beam of the object
        /// </summary>
        public Length Beam => DimensionToPort + DimensionToStarboard;

        /// <summary>
        /// True if the beacon is off position. Can only happen for real AtoN targets.
        /// If this is true, caution is advised, because the beacon is floating in a wrong
        /// position, endangering ships, and it's missing where it should be.
        /// </summary>
        public bool OffPosition { get; set; }

        /// <summary>
        /// True if this is a virtual aid-to-navigation target. There's typically no
        /// visible buoy at the given position, and the signal is sent from a remote base station.
        /// </summary>
        public bool Virtual { get; set; }

        /// <summary>
        /// The type of navigational aid this target indicates.
        /// </summary>
        public NavigationalAidType NavigationalAidType { get; set; }
    }
}
