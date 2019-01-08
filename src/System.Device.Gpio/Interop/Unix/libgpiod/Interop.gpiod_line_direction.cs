// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Read the GPIO line direction setting.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n691">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <returns>GPIOD_DIRECTION_INPUT or GPIOD_DIRECTION_OUTPUT.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_direction(SafeLineHandle line);
}