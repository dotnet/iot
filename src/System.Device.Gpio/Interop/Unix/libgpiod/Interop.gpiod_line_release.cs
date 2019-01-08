// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Release a previously reserved line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1045">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    [DllImport(LibgpiodLibrary)]
    internal static extern void gpiod_line_release(SafeLineHandle lineHandle);
}