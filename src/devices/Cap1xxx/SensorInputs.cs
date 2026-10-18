// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Cap1xxx
{
    /// <summary>
    /// Capacitive touch sensor inputs of the CAP1208 (CS1 to CS8 in the data sheet).
    /// </summary>
    [Flags]
    public enum SensorInputs : byte
    {
        /// <summary>
        /// No input.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sensor input 1 (CS1).
        /// </summary>
        Input1 = 0x01,

        /// <summary>
        /// Sensor input 2 (CS2).
        /// </summary>
        Input2 = 0x02,

        /// <summary>
        /// Sensor input 3 (CS3).
        /// </summary>
        Input3 = 0x04,

        /// <summary>
        /// Sensor input 4 (CS4).
        /// </summary>
        Input4 = 0x08,

        /// <summary>
        /// Sensor input 5 (CS5).
        /// </summary>
        Input5 = 0x10,

        /// <summary>
        /// Sensor input 6 (CS6).
        /// </summary>
        Input6 = 0x20,

        /// <summary>
        /// Sensor input 7 (CS7).
        /// </summary>
        Input7 = 0x40,

        /// <summary>
        /// Sensor input 8 (CS8).
        /// </summary>
        Input8 = 0x80,

        /// <summary>
        /// All the inputs.
        /// </summary>
        All = 0xFF,
    }
}
