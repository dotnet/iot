// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Com modes
    /// </summary>
    public enum ComOption : byte
    {
        /// <summary>
        /// N-MOS open drain output and 8 COM option (default)
        /// </summary>
        NMos8Com = Command.COM_Option_N8,

        /// <summary>
        /// N-MOS open drain output and 16 COM option
        /// </summary>
        NMos16Com = Command.COM_Option_N16,

        /// <summary>
        /// P-MOS open drain output and 8 COM option
        /// </summary>
        PMos8Com = Command.COM_Option_P8,

        /// <summary>
        /// P-MOS open drain output and 16 COM option
        /// </summary>
        PMos16Com = Command.COM_Option_P16,
    }
}
