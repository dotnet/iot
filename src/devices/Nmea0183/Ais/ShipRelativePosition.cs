// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Contains comparative position information between two ships, such as distance, bearing or estimated time to closest encounter.
    /// </summary>
    public class ShipRelativePosition
    {
        /// <summary>
        /// Relative position instance
        /// </summary>
        /// <param name="from">Source vessel</param>
        /// <param name="to">Target vessel</param>
        /// <param name="distance">Current distance between these vessels</param>
        /// <param name="bearing">Bearing from first vessel to second</param>
        /// <param name="state">Safety state between these vessels</param>
        /// <param name="calculationTime">Time this record was calculated</param>
        public ShipRelativePosition(AisTarget from, AisTarget to, Length distance, Angle bearing, AisSafetyState state, DateTimeOffset calculationTime)
        {
            Distance = distance;
            Bearing = bearing;
            From = from;
            To = to;
            SafetyState = state;
            CalculationTime = calculationTime;
        }

        /// <summary>
        /// Ship from which this relative position is seen (typically the own ship, but of course it's also possible to
        /// calculate possible collision vectors between arbitrary ships)
        /// </summary>
        public AisTarget From { get; }

        /// <summary>
        /// Ship to which this relative position is calculated
        /// </summary>
        public AisTarget To { get; }

        /// <summary>
        /// The current distance between the ships
        /// </summary>
        public Length Distance { get; }

        /// <summary>
        /// The bearing between the sips (Compass direction from <see cref="From"/> to <see cref="To"/>)
        /// </summary>
        public Angle Bearing { get; }

        /// <summary>
        /// Direction in which the other ship is seen from <see cref="From"/> when looking to the bow. A negative value means the other
        /// ship is on the port bow, a positive value on the starboard bow. Only available if the source ship has a valid heading.
        /// </summary>
        public Angle? RelativeDirection { get; init; }

        /// <summary>
        /// Distance at closest point of approach
        /// </summary>
        public Length? ClosestPointOfApproach { get; set; }

        /// <summary>
        /// Time at which the closest point will be reached
        /// </summary>
        public DateTimeOffset? TimeOfClosestPointOfApproach { get; set; }

        /// <summary>
        /// Safety state
        /// </summary>
        public AisSafetyState SafetyState { get; set; }

        /// <summary>
        /// The time this record was calculated.
        /// </summary>
        public DateTimeOffset CalculationTime { get; }

        /// <summary>
        /// Calculate the time of closes approach with a given "now"
        /// </summary>
        /// <param name="now">This is the current time</param>
        /// <returns>Time from now to TCPA</returns>
        public TimeSpan? TimeToClosestPointOfApproach(DateTimeOffset now)
        {
            if (!TimeOfClosestPointOfApproach.HasValue)
            {
                return null;
            }

            return TimeOfClosestPointOfApproach - now;
        }
    }
}
