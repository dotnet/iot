// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Ak8963
{
    /// <summary>
    /// Measurement used by the AK8963
    /// </summary>
    public enum MeasurementMode
    {         
        PowerDown = 0b0000,
        SingleMeasurement = 0b0001,
        //  0010 for 8 Hz
        ContinousMeasurement8Hz = 0b010,
        // 0110 for 100 Hz sample rates
        ContinousMeasurement100Hz = 0b0110,
        ExternalTriggedMeasurement = 0b0100,
        SelfTest = 0b1000,
        FuseRomAccess = 0b1111,
    }
}