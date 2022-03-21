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
        NMos8Com = Command.NMos8Com,

        /// <summary>
        /// N-MOS open drain output and 16 COM option
        /// </summary>
        NMos16Com = Command.NMos16Com,

        /// <summary>
        /// P-MOS open drain output and 8 COM option
        /// </summary>
        PMos8Com = Command.PMos8Com,

        /// <summary>
        /// P-MOS open drain output and 16 COM option
        /// </summary>
        PMos16Com = Command.PMos16Com,
    }
}
