// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Response from READ STATUS instruction.
    /// </summary>
    [Flags]
    public enum ReadStatusResponse : byte
    {
        /// <summary>
        /// RX0IF (CANINTF Register).
        /// </summary>
        Rx0If = 1,

        /// <summary>
        /// RX1IF (CANINTF Register).
        /// </summary>
        Rx1If = 2,

        /// <summary>
        /// TXREQ (TXB0CTRL Register).
        /// </summary>
        Tx0Req = 4,

        /// <summary>
        /// TX0IF (CANINTF Register).
        /// </summary>
        Tx0If = 8,

        /// <summary>
        /// TXREQ (TXB1CTRL Register).
        /// </summary>
        Tx1Req = 16,

        /// <summary>
        /// TX1IF (CANINTF Register).
        /// </summary>
        Tx1If = 32,

        /// <summary>
        /// TXREQ (TXB2CTRL Register).
        /// </summary>
        Tx2Req = 64,

        /// <summary>
        /// TX2IF (CANINTF Register).
        /// </summary>
        Tx2If = 128
    }
}
