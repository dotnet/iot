// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Libgpiod.V1;

/// <summary>
/// Pointer to an iterator of all GPIO chips available on the device.
/// </summary>
internal class SafeChipIteratorHandle : SafeHandle
{
    public SafeChipIteratorHandle(IntPtr handle)
        : base(handle, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        // We can't close the chip here, as this would possibly result in it being freed twice, which causes a crash
        LibgpiodV1.gpiod_chip_iter_free_noclose(handle);
        handle = IntPtr.Zero;
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero || handle == LibgpiodV1.InvalidHandleValue;
}
