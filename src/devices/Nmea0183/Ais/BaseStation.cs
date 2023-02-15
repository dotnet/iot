// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.Common;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// A base station target.
    /// These targets identify the position of a base-station antenna, and therefore are the only AIS targets
    /// that typically reside on land. Such a target in range typically means that channel 16 is supervised and that traffic is controlled.
    /// </summary>
    public record BaseStation : AisTarget
    {
        /// <summary>
        ///  Base stations have no name in their data (just the country identifier)
        /// </summary>
        private const string BaseStationName = "Base Station";

        /// <summary>
        /// construct a base station instance.
        /// </summary>
        /// <param name="mmsi">The MMSI of the base station. Shall start with 00</param>
        public BaseStation(uint mmsi)
        : base(mmsi)
        {
            Position = new GeographicPosition();
            Name = BaseStationName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string s = FormatMmsi() + $"({Name})";

            if (Position.ContainsValidPosition())
            {
                s += $" {Position}";
            }

            return s;
        }
    }
}
