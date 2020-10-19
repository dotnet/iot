// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Device.Pwm
{
    /// <summary>
    /// Represents a single PWM channel.
    /// </summary>
    public abstract partial class PwmChannel
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static PwmChannel CreateWindows10PwmChannel(int chip, int channel, int frequency, double dutyCyclePercentage)
        {
            // If we land in this method it means the console application is running on Windows and targetting net5.0 (without specifying Windows platform)
            // In order to call WinRT code in net5.0 it is required for the application to target the specific platform
            // so we throw the bellow exception with a detailed message in order to instruct the consumer on how to move forward.
            throw new PlatformNotSupportedException("In order to use PwmChannel on Windows with .NET 5.0 it is required for your application to target net5.0-windows10.0.17763.0. Please add that to your target frameworks in your project file.");
        }
    }
}
