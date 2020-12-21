// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace System.Device.I2c
{
    /// <summary>
    /// The communications channel to a device on an I2C bus.
    /// </summary>
    public abstract partial class I2cBus : IDisposable
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static I2cBus CreateWindows10I2cBus(int busId)
        {
            // This wrapper is needed to prevent Mono from loading Windows10I2cDevice
            // which causes all fields to be loaded - one of such fields is WinRT type which does not
            // exist on Linux which causes TypeLoadException.
            // Using NoInlining and no explicit type prevents this from happening.
            return new Windows10I2cBus(busId);
        }
    }
}
