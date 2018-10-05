// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Devices.Gpio
{
    [Flags]
    public enum PinEvent
    {
        None = 0,
        Low = 1,
        High = 2,
        SyncFallingEdge = 4,
        SyncRisingEdge = 8,
        AsyncFallingEdge = 16,
        AsyncRisingEdge = 32,

        LowHigh = Low | High,
        SyncFallingRisingEdge = SyncFallingEdge | SyncRisingEdge,
        AsyncFallingRisingEdge = AsyncFallingEdge | AsyncRisingEdge,

        Any = LowHigh | SyncFallingRisingEdge | AsyncFallingRisingEdge
    }
}
