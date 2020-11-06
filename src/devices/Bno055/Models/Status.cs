// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bno055
{
    /// <summary>
    /// Status
    /// </summary>
    public enum Status
    {
        /// <summary>Idle</summary>
        Idle = 0,

        /// <summary>System error</summary>
        SystemError,

        /// <summary>Initializing peripherals</summary>
        InitializingPeripherals,

        /// <summary>System initialization</summary>
        SystemInitialization,

        /// <summary>Executing self test</summary>
        ExcecutingSelftest,

        /// <summary>Sensor fusion algorithm running</summary>
        SensorFusionAlgorithmRunning,

        /// <summary>System running without fusion algorithm</summary>
        SystemRunningWithoutFusionAlgorithm
    }
}
