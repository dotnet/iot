// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
            // This wrapper is needed to prevent Mono from loading Windows10PwmChannel
            // which causes all fields to be loaded - one of such fields is WinRT type which does not
            // exist on Linux which causes TypeLoadException.
            // Using NoInlining and no explicit type prevents this from happening.
            return new Channels.Windows10PwmChannel(
                    chip,
                    channel,
                    frequency,
                    dutyCyclePercentage);
        }
    }
}
