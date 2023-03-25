// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mfrc522
{
    /// <summary>
    /// Command to send to card
    /// </summary>
    internal enum CardCommand
    {
        ReqA = 0x26,
        HaltA = 0x50,
        WupA = 0x52,
        SelectCascadeLevel1 = 0x93, // Used for anticollision as well
        SelectCascadeLevel2 = 0x95,
        SelectCascadeLevel3 = 0x97
    }
}
