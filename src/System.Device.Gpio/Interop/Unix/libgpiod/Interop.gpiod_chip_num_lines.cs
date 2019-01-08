// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Get the number of GPIO lines exposed by this chip.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n464">here</see>.
    /// </summary>
    /// <param name="chip">The GPIO chip handle.</param>
    /// <returns>Number of GPIO lines.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_chip_num_lines(SafeChipHandle chip);
}