// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Qmc5883l
{
    /// <summary>
    /// Pointer roll-over function is controlled by ROL_PNT register. When the point roll-over function is enabled, the I2C
    /// data pointer automatically rolls between 00H ~ 06H, if I2C read begins at any address among 00H~06H.
    /// ROL_PNT: “0”: Normal, “1”: Enable pointer roll-over function
    /// </summary>
    public enum RollPointer : byte
    {
        /// <summary>
        /// Rolls the I2C pointer/address
        /// </summary>
        Enable = 0x01,

        /// <summary>
        /// Doesn't roll the I2C pointer/address
        /// </summary>
        Disable = 0x00
    }
}
