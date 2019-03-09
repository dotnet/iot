// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Bno055
{
    [Flags]
    public enum TestResult
    {
        AcceleratorSuccess = 0b0000_0001,
        MagentometerSuccess = 0b0000_0010,
        GyroscopeSuccess = 0b0000_0100,
        McuSuccess = 0b0000_1000,
    }
}
