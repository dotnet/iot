// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    /// <summary>
    ///     Set the value of a single GPIO line.
    ///     <see href="https://git.kernel.org/pub/scm/libs/libgpiod/libgpiod.git/tree/include/gpiod.h#n1107">here</see>.
    /// </summary>
    /// <param name="line">GPIO line handle</param>
    /// <param name="value">New value.</param>
    /// <returns>0 if the operation succeeds. In case of an error this routine returns -1 and sets the last error number.</returns>
    [DllImport(LibgpiodLibrary)]
    internal static extern int gpiod_line_set_value(SafeLineHandle line, int value);
}