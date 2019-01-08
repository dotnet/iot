// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Request all event type notifications on a single line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n860">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="consumer">Name of the consumer.</param>
    /// <returns>0 the operation succeeds, -1 on failure.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern int gpiod_line_request_both_edges_events(SafeLineHandle line, string consumer);
}
