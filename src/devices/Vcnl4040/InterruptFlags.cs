// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Interrupt flags of the VCNL4040 device.
    /// </summary>
    /// <param name="PsProtectionMode">The official VCNL4040 documentation does not describe purpose and use of this flag.</param>
    /// <param name="AlsLow">Indicates that an interrupt (INT-pin) was triggered, because the lower illuminance
    /// threshold has been undershot starting from a higher value.</param>
    /// <param name="AlsHigh">Indicates than an interrupt (INT-pin) was triggered, because the upper illuminance
    /// threshold has been exceeded starting from a lower value.</param>
    /// <param name="PsClose">Indicates than an interrupt (INT-pin) was triggered, because a close proximity event
    /// has been occurred (object approached).</param>
    /// <param name="PsAway">Indicates than an interrupt (INT-pin) was triggered, because a far proximity event
    /// has been occurred (object moved away).</param>
    public record InterruptFlags(bool PsProtectionMode, bool AlsLow, bool AlsHigh, bool PsClose, bool PsAway);
}
