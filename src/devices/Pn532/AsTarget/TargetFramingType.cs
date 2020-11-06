// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Target Framing Type
    /// </summary>
    public enum TargetFramingType
    {
        /// <summary>
        /// Mifare
        /// </summary>
        Mifare = 0b0000_0000,

        /// <summary>
        /// Active Mode
        /// </summary>
        ActiveMode = 0b0000_0001,

        /// <summary>
        /// FeliCa
        /// </summary>
        FeliCa = 0b0000_0010
    }
}
