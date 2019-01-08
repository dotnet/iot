// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Read the last event from the GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1179">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="eventInfo">Buffer to which the event data will be copied.</param>
    /// <returns>0 if the event was read correctly, -1 on error.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_event_read(SafeLineHandle line, out gpiod_line_event eventInfo);
}

internal struct gpiod_line_event
{
    internal timespec ts;
    internal int event_type;
}