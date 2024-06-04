// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Device.Gpio.Libgpiod.V2;

/// <summary>
/// Used in case Libgpiod forbids the release
/// </summary>
internal sealed class LineInfoSafeHandleNotFreeable : LineInfoSafeHandle
{
    public LineInfoSafeHandleNotFreeable()
        : base(false)
    {
    }

    protected override bool ReleaseHandle()
    {
        return true;
    }
}
