// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Target Mode Initialization
    /// </summary>
    [Flags]
    public enum TargetModeInitialization
    {
        /// <summary>
        /// Uses all the modes
        /// </summary>
        Default = 0b0000_0000,

        /// <summary>
        /// PassiveOnly flag is used to configure the PN532 to accept to be
        /// initialized only in passive mode, i.e. to refuse active communication mode
        /// /// </summary>
        PassiveOnly = 0b0000_0001,

        /// <summary>
        /// PassiveOnly flag is used to configure the PN532 to accept to be initialized
        /// only in passive mode, i.e. to refuse active communication mode
        /// </summary>
        DepOnly = 0b0000_0010,

        /// <summary>
        /// PICCOnly flag is used to configure the PN532 to accept to be initialized only as
        /// ISO/IEC14443-4 PICC, i.e. receiving an RATS frame. If the PN532 receives another
        /// command frame as first command following AutoColl process, it will be rejected
        /// and the PN532 returns automatically in the AutoColl state
        /// </summary>
        PiccOnly = 0b0000_0100,
    }
}
