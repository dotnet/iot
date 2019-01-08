// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Get the handle to the GPIO line at given offset.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n473">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip handle</param>
    /// <param name="offset">The offset of the GPIO line</param>
    /// <returns>Handle to the GPIO line or NULL if an error occured.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeLineHandle gpiod_chip_get_line(SafeChipHandle chip, int offset);
}