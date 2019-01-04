// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_ctxless_event_loop(string device, uint offset, 
        bool active_low, string consumer, ref timespec timeout, 
        [MarshalAs(UnmanagedType.FunctionPtr)]gpiod_ctxless_event_poll_cb poll_cb, 
        [MarshalAs(UnmanagedType.FunctionPtr)]gpiod_ctxless_event_handle_cb event_cb, IntPtr data);
}

[StructLayout(LayoutKind.Sequential)]
internal struct gpiod_ctxless_event_poll_fd
{
    private int fd;
    private bool eventOccured;
}

internal delegate int gpiod_ctxless_event_poll_cb(uint number_of_lines, gpiod_ctxless_event_poll_fd fd, ref timespec timeout, IntPtr data);

internal delegate int gpiod_ctxless_event_handle_cb(int eventType, uint lineOffset, ref timespec timeout, IntPtr data);
