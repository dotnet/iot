// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Bh1750fvi
{
    /// <summary>
    /// BH1750FVI I2C Address
    /// </summary>
    public enum I2cAddress : byte
    {
        /// <summary>
        /// ADD Pin connect to high power level
        /// </summary>
        AddPinHigh = 0x5C,
        /// <summary>
        /// ADD Pin connect to low power level 
        /// </summary>     
        AddPinLow = 0x23
    }
}