// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// Type of navigational aid for AtoN targets.
    /// Aid types 1-19 are for fixed targets, 20-31 for floating targets (buoys)
    /// </summary>
    public enum NavigationalAidType
    {
        /// <summary>
        /// Unknown type. This should not be used
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// A navigation point
        /// </summary>
        ReferencePoint = 1,

        /// <summary>
        /// A Racon. That is a target that has a very good radar response
        /// </summary>
        Racon = 2,

        /// <summary>
        /// A fixed off-shore structure (oil platform, wind farm, etc)
        /// </summary>
        FixedStructureOffShore = 3,

        /// <summary>
        /// Spare value
        /// </summary>
        Spare = 4,

        /// <summary>
        /// A lighthouse without sectors
        /// </summary>
        LightWithoutSectors = 5,

        /// <summary>
        /// A lighthouse with sectors
        /// </summary>
        LightWithSectors = 6,

        /// <summary>
        /// A leasing light, front
        /// </summary>
        LeadingLightFront = 7,

        /// <summary>
        /// A leading light, rear
        /// </summary>
        LeadingLigthRear = 8,

        /// <summary>
        /// Cardinal north beacon
        /// </summary>
        BeaconCardinalN = 9,

        /// <summary>
        /// Cardinal east beacon
        /// </summary>
        BeaconCardinalE = 10,

        /// <summary>
        /// Cardinal south beacon
        /// </summary>
        BeaconCardinalS = 11,

        /// <summary>
        /// Cardinal west beacon
        /// </summary>
        BeaconCardinalW = 12,

        /// <summary>
        /// A port-hand beacon
        /// </summary>
        BeaconPortHand = 13,

        /// <summary>
        /// A starboard-hand beacon
        /// </summary>
        BeaconStarboardHand = 14,

        /// <summary>
        /// A port-hand beacon
        /// </summary>
        BeaconPreferredChannelPortHand = 15,

        /// <summary>
        /// A starboard-hand beacon
        /// </summary>
        BeaconPreferredChannelStarboardHand = 16,

        /// <summary>
        /// An isolated-danger beacon
        /// </summary>
        BeaconIsolatedDanger = 17,

        /// <summary>
        /// Safe water/approach beacon
        /// </summary>
        BeaconSafeWater = 18,

        /// <summary>
        /// Special beacon (yellow)
        /// </summary>
        BeaconSpecialMark = 19,

        /// <summary>
        /// North cardinal mark
        /// </summary>
        CardinalMarkN = 20,

        /// <summary>
        /// East cardinal mark
        /// </summary>
        CardinalMarkE = 21,

        /// <summary>
        /// South cardinal mark
        /// </summary>
        CardinalMarkS = 22,

        /// <summary>
        /// West cardinal mark
        /// </summary>
        CardinalMarkW = 23,

        /// <summary>
        /// Port hand mark
        /// </summary>
        PortHandMark = 24,

        /// <summary>
        /// Starboard hand mark
        /// </summary>
        StarboardHandMark = 25,

        /// <summary>
        /// Port hand channel mark
        /// </summary>
        PreferredChannelPortHand = 26,

        /// <summary>
        /// Starboard hand channel mark
        /// </summary>
        PreferredChannelStarboardHand = 27,

        /// <summary>
        /// Isolated danger mark
        /// </summary>
        IsolatedDanger = 28,

        /// <summary>
        /// Safe water/approach mark
        /// </summary>
        SafeWater = 29,

        /// <summary>
        /// Special mark (yellow)
        /// </summary>
        SpecialMark = 30,

        /// <summary>
        /// A light ship
        /// </summary>
        LightVessel = 31
    }
}
