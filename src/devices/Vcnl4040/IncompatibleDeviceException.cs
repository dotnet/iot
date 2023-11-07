// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Exception that indicates that an incompatible device was found.
    /// </summary>
    public class IncompatibleDeviceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IncompatibleDeviceException"/> class.
        /// </summary>
        public IncompatibleDeviceException(int expectedDeviceId, int actualDeviceId)
            : base($"Incompatible device found (expected ID: {expectedDeviceId}, actual ID: {actualDeviceId})")
        {
        }
    }
}
