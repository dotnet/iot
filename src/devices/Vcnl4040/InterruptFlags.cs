// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// BLABLABLA
    /// </summary>
    /// <param name="PsProtectionMode">BLABLABLA</param>
    /// <param name="AlsLow">BLABLABLA</param>
    /// <param name="AlsHigh">BLABLABLA</param>
    /// <param name="PsClose">BLABLABLA</param>
    /// <param name="PsAway">BLABLABLA</param>
    public record InterruptFlags(bool PsProtectionMode, bool AlsLow, bool AlsHigh, bool PsClose, bool PsAway);
}
