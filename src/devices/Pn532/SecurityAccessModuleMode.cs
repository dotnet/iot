// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Securioty Access Module Mode
    /// </summary>
    public enum SecurityAccessModuleMode
    {
        // 0x01: Normal mode, the SAM is not used; this is the default mode,
        Normal = 0x01,
        // 0x02: Virtual Card, the couple PN532+SAM is seen as only one
        // contactless SAM card from the external world,
        VirtualCard = 0x02,
        // 0x03: Wired Card, the host controller can access to the SAM with
        // standard PCD commands (InListPassiveTarget,InDataExchange, …),
        WiredCard = 0x03,
        // 0x04: Dual Card, both the PN532 and the SAM are visible from the
        // external world as two separated targets. 
        DualCard = 0x04
    }
}
