// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;
using Libgpiodv2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Interop.Unix.libgpiod.v2.Binding.Handles;

internal class LineRequestSafeHandle : SafeHandle
{
    public LineRequestSafeHandle()
        : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        Libgpiodv2.gpiod_line_request_release(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override void Dispose(bool disposing)
    {
        ReleaseHandle();
    }
}
