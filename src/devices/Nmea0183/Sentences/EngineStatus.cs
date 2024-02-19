// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Nmea0183.Sentences
{
    /// <summary>
    /// Engine error status bits. If this is 0, the engine is running fine, anything else indicates
    /// a possible problem that should be addressed as soon as possible.
    /// </summary>
    /// <remarks>
    /// The meaning of some of these flags is derived from its name and may not be accurate for all types of engines.
    /// Older engines do not have all the sensors required and may never return some of the errors.
    /// It is unclear what status an engine reports when it's off. If the electronics is directly coupled to
    /// the engine it will probably not send any messages at all in that state.
    /// </remarks>
    [Flags]
    public enum EngineStatus : UInt32
    {
        /// <summary>
        /// No status bit set = all is well
        /// </summary>
        None,

        /// <summary>
        /// Engine should be checked, or engine is performing self test.
        /// </summary>
        CheckEngine = 0b_0000_0001_0000_0000,

        /// <summary>
        /// Engine is too hot
        /// </summary>
        OverTemperature = 0b_0010_0000_0000_0000,

        /// <summary>
        /// Engine lubrication oil pressure is low
        /// </summary>
        LowOilPressure = 0b_0000_0100_0000_0000,

        /// <summary>
        /// Not enough lubrication oil
        /// </summary>
        LowOilLevel = 0b_0000_1000_0000_0000,

        /// <summary>
        /// Low fuel pressure
        /// </summary>
        LowFuelPressure = 0b_0001_0000_0000_0000,

        /// <summary>
        /// Low battery voltage
        /// </summary>
        LowSystemVoltage = 0b0010_0000_0000_0000,

        /// <summary>
        /// Not enough coolant water in the coolant loop
        /// </summary>
        LowCoolantLevel = 0b0100_0000_0000_0000,

        /// <summary>
        /// Low external cooling water flow. Check inlet valves.
        /// </summary>
        WaterFlow = 0b1000_0000_0000_0000,

        /// <summary>
        /// There's water in the fuel.
        /// </summary>
        WaterInFuel = 0b0000_0000_0000_0001,

        /// <summary>
        /// The alternator is not charging. This is expected to be on when the engine main switch
        /// is enabled but the engine is not (yet) running. If it is on when the engine is running,
        /// the alternator is probably broken.
        /// </summary>
        ChargeIndicator = 0b_0000_0000_0000_0010,

        /// <summary>
        /// The engine is pre-heating. One should wait until this signal goes off before attempting
        /// to start the engine.
        /// </summary>
        PreheatIndicator = 0b0000_0000_0000_0100,

        /// <summary>
        /// ???
        /// </summary>
        HighBoostPressure = 0b0000_0000_0000_1000,

        /// <summary>
        /// The engine is running above it's designated safe RPM limit.
        /// </summary>
        RevLimitExceeded = 0b0000_0000_0001_0000,

        /// <summary>
        /// EGR System failure?
        /// </summary>
        EgrSystem = 0b0000_0000_0010_0000,

        /// <summary>
        /// The throttle position sensor is broken
        /// </summary>
        ThrottlePositionSensor = 0b0000_0000_0100_0000,

        /// <summary>
        /// The engine entered emergency stop mode
        /// </summary>
        EngineEmergencyStopMode = 0b0000_0000_1000_0000,

        /// <summary>
        /// This is a level 1 warning
        /// </summary>
        WarningLevel1 = 0b0001_0000_0000_0000_0000,

        /// <summary>
        /// This is a level 2 warning
        /// </summary>
        WarningLevel2 = 0b0010_0000_0000_0000_0000,

        /// <summary>
        /// The engine power is reduced
        /// </summary>
        PowerReduction = 0b0100_0000_0000_0000_0000,

        /// <summary>
        /// Engine maintenance is needed
        /// </summary>
        MaintenanceNeeded = 0b1000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// There was an error communicating with the engine controls
        /// </summary>
        EngineCommError = 0b0001_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// ???
        /// </summary>
        SecondaryThrottle = 0b0010_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// The engine cannot be started because the gear is not in "neutral" position. Set gear and throttle
        /// to neutral and try again.
        /// </summary>
        NeutralStartProtect = 0b0100_0000_0000_0000_0000_0000_0000_0000,

        /// <summary>
        /// The engine is shutting down.
        /// </summary>
        EngineShuttingDown = 0b1000_0000_0000_0000_0000_0000_0000_0000,
    }
}
