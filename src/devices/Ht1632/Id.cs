// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ht1632
{
    /// <summary>
    /// Mode ID
    /// </summary>
    internal enum Id : byte
    {
        // References first 3-bits of command codes like:
        // 100 0000-0000-X
        // ~~~

        /// <summary>
        /// READ Mode
        /// </summary>
        READ = 0b_110,

        /// <summary>
        /// WRITE Mode
        /// </summary>
        WRITE = 0b_101,

        /// <summary>
        /// READ-MODIFY-WRITE Mode
        /// </summary>
        READ_MODIFY_WRITE = 0b_101,

        /// <summary>
        /// Command Mode
        /// </summary>
        COMMAND = 0b_100,
    }
}
