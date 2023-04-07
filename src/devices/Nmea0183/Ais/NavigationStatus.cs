// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// The navigation status defines what the ship is currently doing. This information is relevant for the determination of the relevant rules
    /// for the collision avoidance regulations (COLREGS)
    /// Only class A type transceivers provide this field
    /// </summary>
    public enum NavigationStatus
    {
        /// <summary>
        /// A ship using its engine. These have the lowest priority.
        /// </summary>
        UnderWayUsingEngine,

        /// <summary>
        /// The ship is at anchor
        /// </summary>
        AtAnchor,

        /// <summary>
        /// The ship is not under command. This is used by ships that have lost their propulsion or have a steering problem.
        /// Ships showing this state must be avoided
        /// </summary>
        NotUnderCommand,

        /// <summary>
        /// The ship has restricted maneuverability. The ship may be performing underwater operations or may otherwise not be free to stay clear.
        /// </summary>
        RestrictedManeuverability,

        /// <summary>
        /// The ship is constrained by her draught. Used by large ships in narrow channels.
        /// </summary>
        ConstrainedByHerDraught,

        /// <summary>
        /// The ship is moored in port.
        /// </summary>
        Moored,

        /// <summary>
        /// The ship is aground. Not good.
        /// </summary>
        Aground,

        /// <summary>
        /// The ship is fishing. These ships have precedence and are restricted in maneuverability. Avoid sailing right behind such a ship,
        /// as it may be dragging nets.
        /// </summary>
        EngagedInFishing,

        /// <summary>
        /// This is a sailing vessel that is currently sailing. Has precedence over ships under engine. This status is relatively rare, since
        /// there are not so many sailing vessels with class A transceivers.
        /// </summary>
        UnderWaySailing,

        /// <summary>
        /// Reserved, may be used for high speed craft
        /// </summary>
        ReservedForFutureAmendmentOfNavigationalStatusForHsc,

        /// <summary>
        /// Reserved, may be used for wing-in-ground aircraft
        /// </summary>
        ReservedForFutureAmendmentOfNavigationalStatusForWig,

        /// <summary>
        /// Reserved
        /// </summary>
        ReservedForFutureUse1,

        /// <summary>
        /// Reserved
        /// </summary>
        ReservedForFutureUse2,

        /// <summary>
        /// Reserved
        /// </summary>
        ReservedForFutureUse3,

        /// <summary>
        /// Ais Sart target. Seing a target of this type most likely means there's a man overboard!
        /// </summary>
        AisSartIsActive,

        /// <summary>
        /// Not set
        /// </summary>
        NotDefined
    }
}
