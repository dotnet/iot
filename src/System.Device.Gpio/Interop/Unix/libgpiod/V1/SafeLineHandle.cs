// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using LibgpiodV1 = Interop.LibgpiodV1;

namespace System.Device.Gpio.Libgpiod.V1;

/// <summary>
/// Pointer to a pin (Not a real SafeLineHandle, because we need to align its finalization with the owning object)
/// </summary>
internal sealed class SafeLineHandle : IDisposable
{
    private IntPtr _handle;
    public SafeLineHandle()
    {
        _handle = IntPtr.Zero;
    }

    public SafeLineHandle(IntPtr handle)
    {
        _handle = handle;
        PinMode = PinMode.Input;
    }

    public PinMode PinMode { get; set; }

    public IntPtr Handle
    {
        get
        {
            return _handle;
        }
        set
        {
            _handle = value;
        }
    }

    /// <summary>
    /// Release the lock on the line handle. <see cref="Interop.libgpiod.gpiod_line_release"/>
    /// </summary>
    public void ReleaseLock()
    {
        // Contrary to intuition, this does not invalidate the handle (see comment on declaration)
        Interop.libgpiod.gpiod_line_release(_handle);
    }

    public bool IsInvalid => _handle == IntPtr.Zero || _handle == Interop.libgpiod.InvalidHandleValue;

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            Interop.libgpiod.gpiod_line_release(_handle);
            _handle = IntPtr.Zero;
        }
    }
}
