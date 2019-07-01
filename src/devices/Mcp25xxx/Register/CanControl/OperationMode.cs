// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Mcp25xxx.Tests.Register.CanControl
{
    /// <summary>
    /// Operation Mode.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>
        /// Device is in the Normal Operation mode.
        /// </summary>
        NormalOperation = 0,
        /// <summary>
        /// Device is in Sleep mode.
        /// </summary>
        Sleep = 1,
        /// <summary>
        /// Device is in Loopback mode.
        /// </summary>
        Loopback = 2,
        /// <summary>
        /// Device is in Listen-Only mode.
        /// </summary>
        ListenOnly = 3,
        /// <summary>
        /// Device is in Configuration mode.
        /// </summary>
        Configuration = 4
    }
}
