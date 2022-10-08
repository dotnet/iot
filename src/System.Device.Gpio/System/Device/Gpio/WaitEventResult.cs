// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio;

internal enum WaitEventResult
{
    Error = -1,
    TimedOut = 0,
    EventOccured = 1
}
