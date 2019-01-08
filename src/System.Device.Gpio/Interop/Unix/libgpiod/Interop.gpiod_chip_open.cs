// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Open a gpiochip by path.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n398">here</see>.
    /// </summary>
    /// <param name="path">Path to the gpiochip device file</param>
    /// <returns>GPIO chip handle or NULL if an error occurred.</returns>
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeChipHandle gpiod_chip_open(string path);
}