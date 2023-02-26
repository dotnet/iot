// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Returned when trying to identify the type of MMSI. A normal 9-digit MMSI is a ship, one that starts
    /// with a 0 is a special group code, one starting with 00 is a base station. See <see cref="AisTarget.IdentifyMmsiType"/> for details.
    /// </summary>
    /// <remarks>
    /// Any system that uses AIS-Sart, MOB or Epirb type is only activated in case of an emergency. When receiving any message from such
    /// a transmitter an alert should be generated and the nearest MRCC should be contacted immediately. Somebody might be in imminent danger!
    ///
    /// Some of these types are rarely or only in very limited areas used.
    /// </remarks>
    public enum MmsiType
    {
        /// <summary>
        /// The MMSI uses a reserved range.
        /// </summary>
        Unknown,

        /// <summary>
        /// This is the default
        /// </summary>
        Ship,

        /// <summary>
        /// A group of ships (e.g. all SAR vessels in range)
        /// </summary>
        Group,

        /// <summary>
        /// A base station uses this ID, e.g DSC broadcast messages from rescue coordination centers (M)RCC
        /// </summary>
        BaseStation,

        /// <summary>
        /// A SAR aircraft/helicopter
        /// </summary>
        SarAircraft,

        /// <summary>
        /// Aid-to-navigation AIS transponder. These typically are boys sending out AIS messages, so they can be found in fog.
        /// </summary>
        AtoN,

        /// <summary>
        /// The vessel is an auxiliary vessel of another.
        /// </summary>
        Auxiliary,

        /// <summary>
        /// This is an AIS-SART transponder.
        /// </summary>
        AisSart,

        /// <summary>
        /// An (increasingly popular) AIS-MOB device was activated.
        /// </summary>
        Mob,

        /// <summary>
        /// Similar to the above: An Epirb with Ais was activated.
        /// </summary>
        Epirb,

        /// <summary>
        /// Handheld radio of a diver
        /// </summary>
        DiversRadio
    }
}
