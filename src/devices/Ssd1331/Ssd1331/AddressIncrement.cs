// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd1331
{
    /// <summary>
    /// Address increment mode
    /// </summary>
    public enum AddressIncrement
    {
        /// <summary>
        /// Horizontal address increment mode
        /// After display RAM is read/written, the column address pointer is increased automatically by 1.
        /// </summary>
        Horizontal = 0,

        /// <summary>
        /// Vertical address increment mode
        /// After display RAM is read/written, the row address pointer is increased automatically by 1.
        /// </summary>
        Vertical = 1
    }
}
