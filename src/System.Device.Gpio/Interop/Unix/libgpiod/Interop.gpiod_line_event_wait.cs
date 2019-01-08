// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Wait for an event on a single line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1156">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="timeout">Wait time limit.</param>
    /// <returns>0 if wait timed out, -1 if an error occurred, 1 if an event occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_event_wait(SafeLineHandle line, ref timespec timeout);
}

[StructLayout(LayoutKind.Sequential)]
internal struct timespec
{
    internal IntPtr tv_sec;
    internal IntPtr tv_nsec;

    internal timespec(IntPtr seconds, IntPtr nanoSeconds) {
        tv_sec = seconds;
        tv_nsec = nanoSeconds;
    }
}