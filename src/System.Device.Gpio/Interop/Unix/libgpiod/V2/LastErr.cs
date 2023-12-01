// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;

namespace System.Device.Gpio.Interop.Unix.libgpiod.V2;

internal static class LastErr
{
    public static string GetMsg()
    {
#if NET7_0_OR_GREATER
        string err = Marshal.GetLastPInvokeErrorMessage();
#else
        string err = Marshal.GetLastWin32Error().ToString();
#endif
        return string.IsNullOrWhiteSpace(err) ? string.Empty : $"Error: '{err}'";
    }
}
