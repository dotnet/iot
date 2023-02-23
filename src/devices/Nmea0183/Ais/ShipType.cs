// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Original code taken from https://github.com/yellowfeather/AisParser, under MIT License
namespace Iot.Device.Nmea0183.Ais
{
    /// <summary>
    /// The type of a ship.
    /// This identifies the static type of a ship. It gives a rough estimate of how the ship looks like and what it's typically used for.
    /// This is conceptually a flag-type listing. The least significant digit (in base-10 notation) gives information about any
    /// dangerous cargo carried. This does not apply for the range 30-39.
    /// Dangerous cargo is anything that would hurt the environment if it were spoiled.
    /// </summary>
    public enum ShipType
    {
        /// <summary>
        /// The ship type has not been specified
        /// </summary>
        NotAvailable,

        /// <summary>
        /// Wing-in-ground aircraft. Very rare
        /// </summary>
        WingInGround = 20,

        /// <summary>
        /// Wing-in-ground aircraft, carrying hazardous cargo
        /// </summary>
        WingInGroundHazardousCategoryA = 21,

        /// <summary>
        /// Wing-in-ground aircraft, carrying hazardous cargo
        /// </summary>
        WingInGroundHazardousCategoryB = 22,

        /// <summary>
        /// Wing-in-ground aircraft, carrying hazardous cargo
        /// </summary>
        WingInGroundHazardousCategoryC = 23,

        /// <summary>
        /// Wing-in-ground aircraft, carrying hazardous cargo
        /// </summary>
        WingInGroundHazardousCategoryD = 24,

        /// <summary>
        /// Reserved
        /// </summary>
        WingInGroundReserved1 = 25,

        /// <summary>
        /// Reserved
        /// </summary>
        WingInGroundReserved2 = 26,

        /// <summary>
        /// Reserved
        /// </summary>
        WingInGroundReserved3 = 27,

        /// <summary>
        /// Reserved
        /// </summary>
        WingInGroundReserved4 = 28,

        /// <summary>
        /// Reserved
        /// </summary>
        WingInGroundReserved5 = 29,

        /// <summary>
        /// This is a fishing vessel. A vessel equipped with a category A transceiver should also set the <see cref="NavigationStatus"/> to
        /// <see cref="NavigationStatus.EngagedInFishing"/> when actually fishing.
        /// </summary>
        Fishing = 30,

        /// <summary>
        /// This is a tow
        /// </summary>
        Tow = 31,

        /// <summary>
        /// This is a tow, whose length exceeds 200m or breadth exceeds 25m. The change between Tow/Tug and TowLarge is the only dynamic change of
        /// the ship type that is common.
        /// </summary>
        TowLarge = 32,

        /// <summary>
        /// This vessel performs dredging or other underwater operations
        /// </summary>
        Dredger = 33,

        /// <summary>
        /// This vessel typically engages in diving operations
        /// </summary>
        DivingOps = 34,

        /// <summary>
        /// This is a military vessel. Note that military vessels may have strange MMSI codes or otherwise confusing information.
        /// </summary>
        MilitaryOps = 35,

        /// <summary>
        /// This is a sailing vessel. Used for both sailing yachts as well as large sailboats. Vessels equipped with a class A transceiver should use
        /// <see cref="NavigationStatus.UnderWaySailing"/> when they're actually underway using their sails. Some sailing yachts also use <see cref="PleasureCraft"/>
        /// as ship type instead.
        /// </summary>
        Sailing = 36,

        /// <summary>
        /// A pleasure boat. May be both a power boat or a sailboat. These vessels usually have only a class B transceiver.
        /// </summary>
        PleasureCraft = 37,

        /// <summary>
        /// Reserved
        /// </summary>
        Reserved1 = 38,

        /// <summary>
        /// Reserved
        /// </summary>
        Reserved2 = 39,

        /// <summary>
        /// A high-speed vessel. Mostly used for high-speed passenger ferries.
        /// </summary>
        HighSpeedCraft = 40,

        /// <summary>
        /// A high-speed vessel with dangerous cargo
        /// </summary>
        HighSpeedCraftHazardousCategoryA = 41,

        /// <summary>
        /// A high-speed vessel with dangerous cargo
        /// </summary>
        HighSpeedCraftHazardousCategoryB = 42,

        /// <summary>
        /// A high-speed vessel with dangerous cargo
        /// </summary>
        HighSpeedCraftHazardousCategoryC = 43,

        /// <summary>
        /// A high-speed vessel with dangerous cargo
        /// </summary>
        HighSpeedCraftHazardousCategoryD = 44,

        /// <summary>
        /// Reserved
        /// </summary>
        HighSpeedCraftReserved1 = 45,

        /// <summary>
        /// Reserved
        /// </summary>
        HighSpeedCraftReserved2 = 46,

        /// <summary>
        /// Reserved
        /// </summary>
        HighSpeedCraftReserved3 = 47,

        /// <summary>
        /// Reserved
        /// </summary>
        HighSpeedCraftReserved4 = 48,

        /// <summary>
        /// A high-speed vessel, but no additional info given
        /// </summary>
        HighSpeedCraftNoAdditionalInformation = 49,

        /// <summary>
        /// A pilot vessel. These (typically small) vessels distribute pilots to large ships near a pilot boarding point.
        /// </summary>
        PilotVessel = 50,

        /// <summary>
        /// A search-and-rescue vessel. These vessels are tasked in emergencies to recover people from sinking ships or to tow someone to safety.
        /// Very helpful if one is close if you are in need!
        /// </summary>
        SearchAndRescueVessel = 51,

        /// <summary>
        /// A tug. May be pushing/pulling a platform
        /// </summary>
        Tug = 52,

        /// <summary>
        /// A port tender.
        /// </summary>
        PortTender = 53,

        /// <summary>
        /// An anti-pollution vessel. Used to collect garbage from the sea or to collect spilled oil.
        /// </summary>
        AntiPollutionEquipment = 54,

        /// <summary>
        /// A law enforcement vessel. Police or border patrol.
        /// </summary>
        LawEnforcement = 55,

        /// <summary>
        /// Spare
        /// </summary>
        SpareLocalVessel1 = 56,

        /// <summary>
        /// Spare
        /// </summary>
        SpareLocalVessel2 = 57,

        /// <summary>
        /// A medical transport vessel
        /// </summary>
        MedicalTransport = 58,

        /// <summary>
        /// Ship not engaged in a war. Rare.
        /// </summary>
        NonCombatantShip = 59,

        /// <summary>
        /// A passenger vessel.
        /// Can be anything from a small passenger ferry to a large cruise ship.
        /// </summary>
        Passenger = 60,

        /// <summary>
        /// A passenger vessel carrying dangerous cargo
        /// </summary>
        PassengerHazardousCategoryA = 61,

        /// <summary>
        /// A passenger vessel carrying dangerous cargo
        /// </summary>
        PassengerHazardousCategoryB = 62,

        /// <summary>
        /// A passenger vessel carrying dangerous cargo
        /// </summary>
        PassengerHazardousCategoryC = 63,

        /// <summary>
        /// A passenger vessel carrying dangerous cargo
        /// </summary>
        PassengerHazardousCategoryD = 64,

        /// <summary>
        /// Reserved
        /// </summary>
        PassengerReserved1 = 65,

        /// <summary>
        /// Reserved
        /// </summary>
        PassengerReserved2 = 66,

        /// <summary>
        /// Reserved
        /// </summary>
        PassengerReserved3 = 67,

        /// <summary>
        /// Reserved
        /// </summary>
        PassengerReserved4 = 68,

        /// <summary>
        /// A passenger vessel with no further information
        /// </summary>
        PassengerNoAdditionalInformation = 69,

        /// <summary>
        /// All types of cargo vessels. Can be any size.
        /// </summary>
        Cargo = 70,

        /// <summary>
        /// A cargo vessel with dangerous cargo
        /// </summary>
        CargoHazardousCategoryA = 71,

        /// <summary>
        /// A cargo vessel with dangerous cargo
        /// </summary>
        CargoHazardousCategoryB = 72,

        /// <summary>
        /// A cargo vessel with dangerous cargo
        /// </summary>
        CargoHazardousCategoryC = 73,

        /// <summary>
        /// A cargo vessel with dangerous cargo
        /// </summary>
        CargoHazardousCategoryD = 74,

        /// <summary>
        /// Reserved
        /// </summary>
        CargoReserved1 = 75,

        /// <summary>
        /// Reserved
        /// </summary>
        CargoReserved2 = 76,

        /// <summary>
        /// Reserved
        /// </summary>
        CargoReserved3 = 77,

        /// <summary>
        /// Reserved
        /// </summary>
        CargoReserved4 = 78,

        /// <summary>
        /// A cargo vessel, no additional information
        /// </summary>
        CargoNoAdditionalInformation = 79,

        /// <summary>
        /// A tanker. A cargo ship that caries oil or other fluids, sometimes also gas.
        /// </summary>
        Tanker = 80,

        /// <summary>
        /// A tanker with hazardous cargo. Most tankers do carry at least some hazardous cargo.
        /// </summary>
        TankerHazardousCategoryA = 81,

        /// <summary>
        /// A tanker with hazardous cargo. Most tankers do carry at least some hazardous cargo.
        /// </summary>
        TankerHazardousCategoryB = 82,

        /// <summary>
        /// A tanker with hazardous cargo. Most tankers do carry at least some hazardous cargo.
        /// </summary>
        TankerHazardousCategoryC = 83,

        /// <summary>
        /// A tanker with hazardous cargo. Most tankers do carry at least some hazardous cargo.
        /// </summary>
        TankerHazardousCategoryD = 84,

        /// <summary>
        /// Reserved
        /// </summary>
        TankerReserved1 = 85,

        /// <summary>
        /// Reserved
        /// </summary>
        TankerReserved2 = 86,

        /// <summary>
        /// Reserved
        /// </summary>
        TankerReserved3 = 87,

        /// <summary>
        /// Reserved
        /// </summary>
        TankerReserved4 = 88,

        /// <summary>
        /// A tanker without additional information
        /// </summary>
        TankerNoAdditionalInformation = 89,

        /// <summary>
        /// A special type of ship not matching any of the previous categories
        /// </summary>
        OtherType = 90,

        /// <summary>
        /// Other type, hazardous cargo
        /// </summary>
        OtherTypeHazardousCategoryA = 91,

        /// <summary>
        /// Other type, hazardous cargo
        /// </summary>
        OtherTypeHazardousCategoryB = 92,

        /// <summary>
        /// Other type, hazardous cargo
        /// </summary>
        OtherTypeHazardousCategoryC = 93,

        /// <summary>
        /// Other type, hazardous cargo
        /// </summary>
        OtherTypeHazardousCategoryD = 94,

        /// <summary>
        /// Reserved
        /// </summary>
        OtherTypeReserved1 = 95,

        /// <summary>
        /// Reserved
        /// </summary>
        OtherTypeReserved2 = 96,

        /// <summary>
        /// Reserved
        /// </summary>
        OtherTypeReserved3 = 97,

        /// <summary>
        /// Reserved
        /// </summary>
        OtherTypeReserved4 = 98,

        /// <summary>
        /// Other type, no additional information
        /// </summary>
        OtherTypeNoAdditionalInformation = 99,
    }
}
