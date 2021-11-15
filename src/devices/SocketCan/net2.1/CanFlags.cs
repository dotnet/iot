// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.SocketCan
{
    [Flags]
    internal enum CanFlags : uint
    {
        ExtendedFrameFormat = 0x80000000,
        RemoteTransmissionRequest = 0x40000000,
        Error = 0x20000000,
    }
}
