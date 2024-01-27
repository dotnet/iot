// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV2 = Interop.LibgpiodV2;

namespace System.Device.Gpio.Libgpiod.V2;

internal class RequestConfigSafeHandle : SafeHandle
{
    public RequestConfigSafeHandle()
        : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        LibgpiodV2.gpiod_request_config_free(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}
