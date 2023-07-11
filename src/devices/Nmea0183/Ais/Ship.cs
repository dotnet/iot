// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// A ship. This is the default AIS target type.
    /// </summary>
    public record Ship : MovingTarget
    {
        /// <summary>
        /// Create a ship.
        /// </summary>
        /// <param name="mmsi">The MMSI</param>
        public Ship(uint mmsi)
        : base(mmsi)
        {
            CallSign = string.Empty;
            Destination = string.Empty;
        }

        /// <summary>
        /// The call sign. A sequence of up to 7 letters or numbers, without blanks. Empty if unknown
        /// </summary>
        public string CallSign { get; set; }

        /// <summary>
        /// The ship type
        /// </summary>
        public ShipType ShipType { get; set; }

        /// <summary>
        /// The distance from the GNSS antenna to the bow.
        /// </summary>
        /// <remarks>Since a ship can be several 100 meters long, it is important to know the position of the GNSS antenna used for AIS
        /// relative to its hull. Note that the information in these properties is often not correct.</remarks>
        public Length DimensionToBow { get; set; }

        /// <summary>
        /// The distance from the GNSS antenna to the stern. See remarks under <see cref="DimensionToBow"/>
        /// </summary>
        public Length DimensionToStern { get; set; }

        /// <summary>
        /// The distance from the GNSS antenna to the port side. See remarks under <see cref="DimensionToBow"/>
        /// </summary>
        public Length DimensionToPort { get; set; }

        /// <summary>
        /// The distance from the GNSS antenna to the starboard side. See remarks under <see cref="DimensionToBow"/>
        /// </summary>
        public Length DimensionToStarboard { get; set; }

        /// <summary>
        /// The transceiver type this target uses.
        /// </summary>
        public AisTransceiverClass TransceiverClass { get; set; }

        /// <summary>
        /// The length of the vessel
        /// </summary>
        public Length Length => DimensionToBow + DimensionToStern;

        /// <summary>
        /// The beam of the vessel
        /// </summary>
        public Length Beam => DimensionToPort + DimensionToStarboard;

        /// <summary>
        /// Estimated time of arrival at the destination. Ships without a designated destinations or with recurring destinations often use a dummy value, such as January 1st, midnight
        /// </summary>
        public DateTimeOffset? EstimatedTimeOfArrival { get; set; }

        /// <summary>
        /// A text for the destination, often abbreviated.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// The current draught of the vessel. For large cargo vessels, this may be very variable.
        /// </summary>
        public Length? Draught { get; set; }

        /// <summary>
        /// The IMO number of the ship
        /// </summary>
        public uint ImoNumber { get; set; }

        /// <summary>
        /// Navigation status, see there
        /// </summary>
        public NavigationStatus NavigationStatus { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            string s = Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s))
            {
                s = FormatMmsi();
            }

            if (TransceiverClass == AisTransceiverClass.A)
            {
                s += " (Class A)";
            }
            else if (TransceiverClass == AisTransceiverClass.B)
            {
                s += " (Class B)";
            }

            if (Position.ContainsValidPosition())
            {
                s += $" {Position}";
            }

            return s;
        }
    }
}
