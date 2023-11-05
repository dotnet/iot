// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Configurable parameters that define the behavior of the AIS Manager: Movement estimation, Warning distances, etc.
    /// </summary>
    [Serializable]
    public record TrackEstimationParameters
    {
        /// <summary>
        /// How much time to calculate backwards. Default: 20 minutes
        /// </summary>
        public TimeSpan StartTimeOffset { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// Default step size. Default: 10 seconds
        /// </summary>
        public TimeSpan NormalStepSize { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// How much to calculate ahead. Default: 1 hour
        /// </summary>
        public TimeSpan EndTimeOffset { get; set; } = TimeSpan.FromMinutes(60);

        /// <summary>
        /// True to issue a warning if no position data for the own ship is available. Default: true
        /// </summary>
        public bool WarnIfGnssMissing { get; set; } = true;

        /// <summary>
        /// Time span between AIS safety checks. Default: 5 seconds
        /// </summary>
        public TimeSpan AisSafetyCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Minimum CPA distance to issue a warning. Default: 1 nm.
        /// </summary>
        public Length WarningDistance { get; set; } = Length.FromNauticalMiles(1);

        /// <summary>
        /// Minimum TCPA to issue a warning (when <see cref="WarningDistance"/> is also reached). Default: 10 minutes
        /// </summary>
        public TimeSpan WarningTime { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Maximum age of the position record for a given ship to consider it valid.
        /// If this is set to a high value, there's a risk of calculating TCPA/CPA based on outdated data. Default: 5 minutes
        /// </summary>
        public TimeSpan TargetLostTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Maximum age of our own position to consider it valid. Default: 20 seconds.
        /// </summary>
        public TimeSpan MaximumPositionAge { get; set; } = TimeSpan.FromSeconds(20);

        /// <summary>
        /// Warn if a vessel is lost within this range. Default: 1 nm
        /// </summary>
        public Length VesselLostWarningRange { get; set; } = Length.FromNauticalMiles(1);

        /// <summary>
        /// Even if a vessel is lost within <see cref="VesselLostWarningRange"/>, do not warn if
        /// the last known speed was less than this. This prevents a lot of warnings from
        /// people switching off their AIS while moored. Set to null to disable. Default: 0.5 knots
        /// </summary>
        public Speed? VesselLostMinSpeed { get; set; } = Speed.FromKnots(0.5);

        /// <summary>
        /// If a target has not been updated for this time, it is deleted from the list of valid targets.
        /// Additionally, client software should consider targets as lost whose <see cref="AisTarget.LastSeen"/> value is older than 5 minutes or so.
        /// A value of 0 or less means infinite. Default: 20 minutes.
        /// </summary>
        public TimeSpan DeleteTargetAfterTimeout { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// Time between repeats of the same warning. If this is set to a short value, the same proximity warning will be shown very often,
        /// which is typically annoying. Default: 10 minutes.
        /// </summary>
        public TimeSpan WarningRepeatTimeout { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Controls how often lost targets are removed completely from the target list. The timespan after which a target is considered lost
        /// is controlled via <see cref="TargetLostTimeout"/>. Default: 25 minutes
        /// </summary>
        public TimeSpan CleanupLatency { get; set; } = TimeSpan.FromMinutes(25);
    }
}
