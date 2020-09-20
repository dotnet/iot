// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// Measurement configuration.
    /// This enum defines the measurement flow of the device, specifically whether or not to incorporate an applied bias to the sensor into the measurement.
    /// </summary>
    public enum MeasurementConfiguration : byte
    {
        /// <summary>
        /// Normal measurement configuration (default).
        /// In normal measurement configuration the device follows normal measurement flow.
        /// The positive and negative pins of the resistive load are left floating and high impedance.
        /// </summary>
        Normal = 0b_0000_0000,

        /// <summary>
        /// Positive bias configuration for X and Y axes, negative bias configuration for Z axis.
        /// In this configuration, a positive current is forced across the resistive load for X and Y axes, a negative current for Z axis.
        /// </summary>
        PositiveBiasConfiguration = 0b_0000_0001,

        /// <summary>
        /// Negative bias configuration for X and Y axes, positive bias configuration for Z axis.
        /// In this configuration, a negative current is forced across the resistive load for X and Y axes, a positive current for Z axis.
        /// </summary>
        NegativeBias = 0b_0000_0010
    }
}