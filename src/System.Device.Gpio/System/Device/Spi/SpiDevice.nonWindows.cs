// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Device.Spi
{
    /// <summary>
    /// The communications channel to a device on a SPI bus.
    /// </summary>
    public abstract partial class SpiDevice
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static SpiDevice CreateWindows10SpiDevice(SpiConnectionSettings settings)
        {
            // If we land in this method it means the console application is running on Windows and targetting net5.0 (without specifying Windows platform)
            // In order to call WinRT code in net5.0 it is required for the application to target the specific platform
            // so we throw the bellow exception with a detailed message in order to instruct the consumer on how to move forward.
            throw new PlatformNotSupportedException(CommonHelpers.GetFormattedWindowsPlatformTargetingErrorMessage(nameof(SpiDevice)));
        }
    }
}
