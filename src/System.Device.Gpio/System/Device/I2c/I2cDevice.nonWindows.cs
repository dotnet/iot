// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace System.Device.I2c
{
    /// <summary>
    /// The communications channel to a device on an I2C bus.
    /// </summary>
    public abstract partial class I2cDevice
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static I2cDevice CreateWindows10I2cDevice(I2cConnectionSettings settings)
        {
            // If we land in this method it means the console application is running on Windows and targetting net5.0 (without specifying Windows platform)
            // In order to call WinRT code in net5.0 it is required for the application to target the specific platform
            // so we throw the bellow exception with a detailed message in order to instruct the consumer on how to move forward.
            throw new PlatformNotSupportedException(CommonHelpers.GetFormattedWindowsPlatformTargetingErrorMessage(nameof(I2cDevice)));
        }
    }
}
