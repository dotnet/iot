// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Register.MessageReceive
{
    /// <summary>
    /// Receive Buffer Operating mode bits.
    /// </summary>
    public enum OperatingMode
    {
        /// <summary>
        /// Receives all valid messages using either Standard or Extended Identifiers that meet filter criteria.
        /// </summary>
        ReceivesAllValidMessages = 0,
        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved1 = 1,
        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved2 = 2,
        /// <summary>
        /// Turns mask/filters off; receives any message.
        /// </summary>
        TurnsMaskFiltersOff = 3
    }
}
