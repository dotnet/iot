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
        /// <summary>
        /// Communication Line Test
        /// </summary>
        CommunicationLineTest = 0x00,
        /// <summary>
        /// ROM Test
        /// </summary>
        ROMTest = 0x01,
        /// <summary>
        /// RAM Test
        /// </summary>
        RAMTest = 0x02,
        /// <summary>
        /// Polling Test To Target
        /// </summary>
        PollingTestToTarget = 0x04,
        /// <summary>
        /// Echo Back Test
        /// </summary>
        EchoBackTest = 0x05,
        /// <summary>
        /// Attention Request Test
        /// </summary>
        AttentionRequestTest = 0x06,
        /// <summary>
        /// Self Antena Test
        /// </summary>
        SelfAntenaTest = 0x07
    }
}
