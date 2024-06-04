// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

internal class EdgeEventSafeHandle : SafeHandle
{
    public EdgeEventSafeHandle()
        : this(true)
    {
    }

    protected EdgeEventSafeHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    protected override bool ReleaseHandle()
    {
        LibgpiodV2.gpiod_edge_event_free(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}
