// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// This target is a SAR aircraft.
    /// These are typically very fast moving targets (100 knots or more)
    /// </summary>
    public record SarAircraft : MovingTarget
    {
        /// <summary>
        /// Unfortunately, the sar message does not include a "name" field.
        /// </summary>
        private const string SarAircraftName = "SAR Aircraft";

        /// <summary>
        /// Create a new SAR aircraft target
        /// </summary>
        /// <param name="mmsi">MMSI</param>
        public SarAircraft(uint mmsi)
            : base(mmsi)
        {
            Name = SarAircraftName;
        }
    }
}
