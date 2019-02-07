// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Hmc5883l
{
    /// <summary>
    /// The mode of HMC5883L measuring
    /// </summary>
    public enum MeasuringMode
    {
        Continuous = 0x00,
        Single = 0x01,
        Idle = 0x02
    }
}
