// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;
using Libgpiodv2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.v2.Binding.Handles;

internal class LineInfoEventSafeHandle : SafeHandle
{
    public LineInfoEventSafeHandle()
        : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        Libgpiodv2.gpiod_info_event_free(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}
