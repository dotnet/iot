// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183
{
    /// <summary>
    /// Type of AIS warnings
    /// </summary>
    public enum AisWarningType
    {
        /// <summary>
        /// No wawrning. This is not used.
        /// </summary>
        None,

        /// <summary>
        /// The Ais Manager is missing GNSS information. Check the input stream and the GNSS antenna
        /// </summary>
        NoGnss,

        /// <summary>
        /// An exceptional target was seen (e.g. an AIS man-overboard beacon)
        /// </summary>
        ExceptionalTargetSeen,

        /// <summary>
        /// Another vessel is dangerously close
        /// </summary>
        DangerousVessel,

        /// <summary>
        /// A vessel that is close was lost
        /// </summary>
        VesselLost,

        /// <summary>
        /// A message from the application layer
        /// </summary>
        UserMessage
    }
}
