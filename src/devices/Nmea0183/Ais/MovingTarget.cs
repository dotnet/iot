// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Base class for moving targets
    /// </summary>
    public abstract record MovingTarget : AisTarget
    {
        /// <summary>
        /// A moving target
        /// </summary>
        /// <param name="mmsi">The MMSI</param>
        protected MovingTarget(uint mmsi)
            : base(mmsi)
        {
        }

        /// <summary>
        /// The rate of turn of the vessel. May be null, because particularly smaller vessels do not have the sensor for this.
        /// </summary>
        public RotationalSpeed? RateOfTurn { get; set; }

        /// <summary>
        /// Course over ground, GNSS derived
        /// </summary>
        public Angle CourseOverGround { get; set; }

        /// <summary>
        /// Speed over ground, GNSS derived
        /// </summary>
        public Speed SpeedOverGround { get; set; }

        /// <summary>
        /// Heading relative to true north. May be null, because not all vessels are equipped with the necessary heading sensor.
        /// </summary>
        public Angle? TrueHeading { get; set; }
    }
}
