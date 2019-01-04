// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Device.Gpio;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibgpiodLibrary, SetLastError = true)]
    internal static extern SafeLineHandle gpiod_chip_get_line(SafeChipHandle chip, int offset);
}