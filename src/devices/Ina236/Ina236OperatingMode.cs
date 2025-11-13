// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc
{
    /// <summary>
    /// Current device operating mode
    /// </summary>
    public enum Ina236OperatingMode
    {
        /// <summary>
        /// The device is off
        /// </summary>
        Shutdown = 0,

        /// <summary>
        /// Generate a single measurement of the shunt voltage value, then wait for a command again
        /// </summary>
        SingeShuntVoltage = 0b001,

        /// <summary>
        /// Generate s single measurement of the shunt voltage value, then wait for a command again
        /// </summary>
        SingleBusVoltage = 0b010,

        /// <summary>
        /// Generate a single measurement of both bus voltage and shunt voltage, then wait for a command again
        /// </summary>
        SingleShuntAndBusVoltage = 0b011,

        /// <summary>
        /// Enter shutdown mode
        /// </summary>
        Shutdown2 = 0b100,

        /// <summary>
        /// Continuously measure the shunt voltage
        /// </summary>
        ContinuousShuntVoltage = 0b101,

        /// <summary>
        /// Continuously measure the bus voltage
        /// </summary>
        ContinuousBusVoltage = 0b110,

        /// <summary>
        /// Continuously measure both bus and shut voltages.
        /// This is the default setting.
        /// </summary>
        ContinuousShuntAndBusVoltage = 0b111,

        /// <summary>
        /// A mask field
        /// </summary>
        ModeMask = 0b111
    }
}
