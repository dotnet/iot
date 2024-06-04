// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Libgpiod.V1;

/// <summary>
/// Pointer to a pin.
/// </summary>
internal class SafeLineHandle : SafeHandle
{
    public PinMode PinMode { get; set; }

    public SafeLineHandle()
        : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        // Contrary to intuition, this does not invalidate the handle (see comment on declaration)
        LibgpiodV1.gpiod_line_release(handle);
        return true;
    }

    /// <summary>
    /// Release the lock on the line handle. <see cref="LibgpiodV1.gpiod_line_release"/>
    /// </summary>
    public void ReleaseLock()
    {
        ReleaseHandle();
    }

    public override bool IsInvalid => handle == IntPtr.Zero || handle == LibgpiodV1.InvalidHandleValue;
}
