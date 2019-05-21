// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bno055
{
    public enum Status
    {
        Idle = 0,
        SystemError,
        InitializingPeripherals,
        SystemInitialization,
        ExcecutingSelftest,
        SensorFusionAlgorithmRunning,
        SystemRunningWithoutFusionAlgorithm
    }
}
