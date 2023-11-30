// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Interop.Unix.libgpiod.V1;

/// <summary>
/// Pointer to an iterator of all GPIO chips available on the device.
/// </summary>
internal class SafeChipIteratorHandle : SafeHandle
{
    public SafeChipIteratorHandle()
        : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        LibgpiodV1.gpiod_chip_iter_free(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero || handle == LibgpiodV1.InvalidHandleValue;
}
