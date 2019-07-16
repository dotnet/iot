// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Pn532
{
    /// <summary>
    /// Diagnostic modes supported by the PN532
    /// </summary>
    public enum DiagnoseMode
    {
        CommunicationLineTest = 0x00,        
        ROMTest = 0x01,
        RAMTest = 0x02,
        PollingTestToTarget = 0x04,
        EchoBackTest = 0x05,
        AttentionRequestTest = 0x06,
        SelfAntenaTest = 0x07
    }
}
