// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(library, SetLastError = true)]
    internal static extern int gpiod_line_event_read(SafeLineHandle line, out gpiod_line_event consumer);
}

internal struct gpiod_line_event
{
    timespec ts;
    public event_type event_type;
}

internal enum event_type
{
    GPIOD_LINE_EVENT_RISING_EDGE = 1,
    GPIOD_LINE_EVENT_FALLING_EDGE
}