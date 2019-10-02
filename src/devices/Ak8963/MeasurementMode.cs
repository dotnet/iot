// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Measurement used by the AK8963
    /// </summary>
    public enum MeasurementMode
    {       
        /// <summary>
        /// Power Down
        /// </summary>
        PowerDown = 0b0000,
        /// <summary>
        /// Single Measurement
        /// </summary>
        SingleMeasurement = 0b0001,
        /// <summary>
        /// Continuous Measurement at 8Hz
        /// </summary>
        ContinuousMeasurement8Hz = 0b010,
        /// <summary>
        /// Continuous Measurement at 100Hz
        /// </summary>
        ContinuousMeasurement100Hz = 0b0110,
        /// <summary>
        /// External Trigged Measurement
        /// </summary>
        ExternalTriggedMeasurement = 0b0100,
        /// <summary>
        /// Self Test
        /// </summary>
        SelfTest = 0b1000,
        /// <summary>
        /// Fuse Rom Access
        /// </summary>
        FuseRomAccess = 0b1111,
    }
}