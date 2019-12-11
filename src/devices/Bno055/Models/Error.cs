// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    /// <summary>
    /// BNO055 error
    /// </summary>
    public enum Error
    {
        /// <summary>No error</summary>
        NoError = 0,

        /// <summary>Peripheral initialization error</summary>
        PeripheralInitializationError,

        /// <summary>System initialization error</summary>
        SystemInitializationError,

        /// <summary>Self test result failed</summary>
        SelftTestResultFailed,

        /// <summary>Register map value out of range</summary>
        RegisterMapValueOutOfRange,

        /// <summary>Register map address out of range</summary>
        RegisterMapAddressOutOfRange,

        /// <summary>Register map write error</summary>
        RegisterMapWriteError,

        /// <summary>BNO055 low power mode not available</summary>
        BnoLowPowerModeNotAvailable,

        /// <summary>Accelerometer power mode not available</summary>
        AccelerometerPowerModeNotAvailable,

        /// <summary>Fusion algorithm configuration error</summary>
        FusionAlgorithmConfigurationError,

        /// <summary>Sensor configuration error</summary>
        SensorConfigurationError
    }
}
