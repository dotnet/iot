// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Disable these StyleCop rules for this file, as we are using native names here.
#pragma warning disable SA1300 // Element should begin with upper-case letter

using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern int access([MarshalAs(UnmanagedType.LPStr)] string pathname, AccessModeFlags flags);
}

internal enum AccessModeFlags
{
    F_OK = 0,
    X_OK = 1,
    W_OK = 2,
    R_OK = 4
}
