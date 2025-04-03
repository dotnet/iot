// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Libgpiod.V1;

/// <summary>
/// Pointer to a general-purpose I/O (GPIO) chip.
/// </summary>
internal class SafeChipHandle : SafeHandle
{
    public SafeChipHandle(IntPtr h)
        : base(h, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        LibgpiodV1.gpiod_chip_close(handle);
        SetHandle(IntPtr.Zero);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero || handle == LibgpiodV1.InvalidHandleValue;
}
