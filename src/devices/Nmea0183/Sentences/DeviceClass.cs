// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// NMEA2000 Device Class codes
    /// Further categorizes devices within their function group
    /// </summary>
    public enum DeviceClass : byte
    {
        /// <summary>Reserved by ISO</summary>
        [Description("Reserved by ISO")]
        Reserved = 0,

        /// <summary>System tools</summary>
        [Description("System tools")]
        SystemTools = 10,

        /// <summary>Safety systems</summary>
        [Description("Safety systems")]
        SafetySystems = 20,

        /// <summary>Inter/Intranetwork Device</summary>
        [Description("Inter/Intranetwork Device")]
        InterIntranetworkDevice = 25,

        /// <summary>Electrical Distribution</summary>
        [Description("Electrical Distribution")]
        ElectricalDistribution = 30,

        /// <summary>Electrical Generation</summary>
        [Description("Electrical Generation")]
        ElectricalGeneration = 35,

        /// <summary>Steering and Control surfaces</summary>
        [Description("Steering and Control surfaces")]
        SteeringAndControlSurfaces = 40,

        /// <summary>Propulsion</summary>
        [Description("Propulsion")]
        Propulsion = 50,

        /// <summary>Navigation systems</summary>
        [Description("Navigation systems")]
        NavigationSystems = 60,

        /// <summary>Communication systems</summary>
        [Description("Communication systems")]
        CommunicationSystems = 70,

        /// <summary>Instrumentation/general systems</summary>
        [Description("Instrumentation/general systems")]
        InstrumentationGeneralSystems = 75,

        /// <summary>External Environment systems</summary>
        [Description("External Environment systems")]
        ExternalEnvironmentSystems = 80,

        /// <summary>Internal Environment systems</summary>
        [Description("Internal Environment systems")]
        InternalEnvironmentSystems = 85,

        /// <summary>Deck, cargo and fishing equipment systems</summary>
        [Description("Deck, cargo and fishing equipment systems")]
        DeckCargoFishingEquipmentSystems = 90,

        /// <summary>
        /// Human Interface
        /// </summary>
        [Description("Human Interface")]
        HumanInterface = 110,

        /// <summary>
        /// Displays, plotters, etc.
        /// </summary>
        [Description("Display")]
        Display = 120,

        /// <summary>
        /// Entertainment equipment (music systems and the like)
        /// </summary>
        [Description("Entertainment")]
        Entertainment = 125,
    }
}
